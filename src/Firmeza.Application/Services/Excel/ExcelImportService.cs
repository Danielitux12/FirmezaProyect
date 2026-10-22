using System.Globalization;
using System.Text.RegularExpressions;
using Firmeza.Application.DTOs.Excel;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firmeza.Application.Services.Excel;

/// <summary>
/// Servicio de aplicación para el procesamiento, validación y persistencia de datos desnormalizados de Excel.
/// Cumple con Clean Architecture y desacopla la lógica de negocio de las librerías de hojas de cálculo.
/// </summary>
public class ExcelImportService : IExcelImportService
{
    private readonly IExcelReaderService _excelReader;
    private readonly IClientRepository _clientRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<ExcelImportService> _logger;

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ExcelImportService(
        IExcelReaderService excelReader,
        IClientRepository clientRepository,
        IProductRepository productRepository,
        ISaleRepository saleRepository,
        ILogger<ExcelImportService> logger)
    {
        _excelReader = excelReader;
        _clientRepository = clientRepository;
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _logger = logger;
    }

    public async Task<ExcelImportResultDto> ImportDesnormalizedDataAsync(Stream fileStream)
    {
        var result = new ExcelImportResultDto();

        var rows = await _excelReader.ReadExcelRowsAsync(fileStream);
        result.TotalRowsRead = rows.Count;

        if (rows.Count == 0)
        {
            result.Errors.Add(new ExcelRowErrorDto
            {
                RowNumber = 0,
                SheetName = "N/A",
                Error = "El archivo Excel está vacío o no contiene filas con datos válidos."
            });
            return result;
        }

        // Cargar registros existentes en memoria para optimizar búsquedas y evitar N+1 queries
        var existingClients = (await _clientRepository.GetAllAsync()).ToList();
        var clientByEmail = new Dictionary<string, Client>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in existingClients)
        {
            if (!string.IsNullOrWhiteSpace(c.Email))
            {
                clientByEmail[c.Email.Trim()] = c;
            }
        }

        var existingProducts = (await _productRepository.GetAllAsync()).ToList();
        var productByName = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in existingProducts)
        {
            if (!string.IsNullOrWhiteSpace(p.Name))
            {
                productByName[p.Name.Trim()] = p;
            }
        }

        // Procesar cada fila desnormalizada
        foreach (var row in rows)
        {
            try
            {
                bool processedAny = false;
                var rowErrors = new List<string>();

                Client? currentClient = null;
                Product? currentProduct = null;

                // 1. Detectar y procesar datos de CLIENTE
                var clientName = row.GetValue("nombrecliente", "clientenombre", "cliente", "customer", "clientname", "customername", "razonsocial", "nombredecliente");
                var clientEmail = row.GetValue("email", "correo", "correoelectronico", "clientemail", "clienteemail", "mail");
                var clientPhone = row.GetValue("telefono", "phone", "celular", "movil", "clientetelefono", "contacto");
                var clientAddress = row.GetValue("direccion", "address", "clientedireccion", "ubicacion", "domicilio");

                bool hasClientData = !string.IsNullOrWhiteSpace(clientEmail) || !string.IsNullOrWhiteSpace(clientName);

                if (hasClientData)
                {
                    if (!string.IsNullOrWhiteSpace(clientEmail) && !EmailRegex.IsMatch(clientEmail))
                    {
                        rowErrors.Add($"El correo del cliente '{clientEmail}' no tiene un formato válido.");
                    }
                    else
                    {
                        var emailKey = !string.IsNullOrWhiteSpace(clientEmail) ? clientEmail.Trim() : $"{clientName?.Trim().ToLowerInvariant()}@firmeza.local";

                        if (clientByEmail.TryGetValue(emailKey, out var existingClient))
                        {
                            // Actualizar cliente existente
                            bool changed = false;
                            if (!string.IsNullOrWhiteSpace(clientName) && existingClient.Name != clientName)
                            {
                                existingClient.Name = clientName;
                                changed = true;
                            }
                            if (!string.IsNullOrWhiteSpace(clientPhone) && existingClient.Phone != clientPhone)
                            {
                                existingClient.Phone = clientPhone;
                                changed = true;
                            }
                            if (!string.IsNullOrWhiteSpace(clientAddress) && existingClient.Address != clientAddress)
                            {
                                existingClient.Address = clientAddress;
                                changed = true;
                            }
                            if (!existingClient.IsActive)
                            {
                                existingClient.IsActive = true;
                                changed = true;
                            }

                            if (changed)
                            {
                                await _clientRepository.UpdateAsync(existingClient);
                                result.ClientsUpdated++;
                            }
                            currentClient = existingClient;
                        }
                        else
                        {
                            // Crear nuevo cliente
                            var newClient = new Client
                            {
                                Id = Guid.NewGuid(),
                                Name = !string.IsNullOrWhiteSpace(clientName) ? clientName : emailKey.Split('@')[0],
                                Email = emailKey,
                                Phone = clientPhone ?? string.Empty,
                                Address = clientAddress ?? string.Empty,
                                FechaCreacion = DateTime.UtcNow,
                                IsActive = true
                            };

                            await _clientRepository.AddAsync(newClient);
                            clientByEmail[emailKey] = newClient;
                            result.ClientsCreated++;
                            currentClient = newClient;
                        }
                        processedAny = true;
                    }
                }

                // 2. Detectar y procesar datos de PRODUCTO
                var productName = row.GetValue("nombreproducto", "productonombre", "producto", "product", "productname", "material", "item", "articulo", "descripcionproducto");
                var productDesc = row.GetValue("descripcion", "detalle", "detalles", "productdescription", "observaciones");
                var productPriceRaw = row.GetValue("precio", "price", "preciounitario", "unitprice", "valor", "costo", "valorunitario");
                var productStockRaw = row.GetValue("stock", "inventario", "cantidadstock", "stockdisponible", "existencias", "quantityinstock");

                bool hasProductData = !string.IsNullOrWhiteSpace(productName);

                if (hasProductData)
                {
                    decimal? parsedPrice = null;
                    if (!string.IsNullOrWhiteSpace(productPriceRaw))
                    {
                        parsedPrice = ParseDecimal(productPriceRaw);
                        if (parsedPrice == null || parsedPrice < 0)
                        {
                            rowErrors.Add($"El precio del producto '{productPriceRaw}' no es un número válido o es menor a cero.");
                        }
                    }

                    int? parsedStock = null;
                    if (!string.IsNullOrWhiteSpace(productStockRaw))
                    {
                        if (int.TryParse(productStockRaw, out var s) && s >= 0)
                        {
                            parsedStock = s;
                        }
                        else
                        {
                            rowErrors.Add($"El stock del producto '{productStockRaw}' no es un entero válido o es negativo.");
                        }
                    }

                    if (rowErrors.Count == 0 && !string.IsNullOrWhiteSpace(productName))
                    {
                        var nameKey = productName.Trim();

                        if (productByName.TryGetValue(nameKey, out var existingProduct))
                        {
                            // Actualizar producto existente
                            bool changed = false;
                            if (parsedPrice.HasValue && existingProduct.Price != parsedPrice.Value)
                            {
                                existingProduct.Price = parsedPrice.Value;
                                changed = true;
                            }
                            if (parsedStock.HasValue && existingProduct.Stock != parsedStock.Value)
                            {
                                existingProduct.Stock = parsedStock.Value;
                                changed = true;
                            }
                            if (!string.IsNullOrWhiteSpace(productDesc) && existingProduct.Description != productDesc)
                            {
                                existingProduct.Description = productDesc;
                                changed = true;
                            }
                            if (!existingProduct.IsActive)
                            {
                                existingProduct.IsActive = true;
                                changed = true;
                            }

                            if (changed)
                            {
                                await _productRepository.UpdateAsync(existingProduct);
                                result.ProductsUpdated++;
                            }
                            currentProduct = existingProduct;
                        }
                        else
                        {
                            // Crear nuevo producto
                            var newProduct = new Product
                            {
                                Id = Guid.NewGuid(),
                                Name = nameKey,
                                Description = productDesc ?? string.Empty,
                                Price = parsedPrice ?? 0m,
                                Stock = parsedStock ?? 0,
                                FechaCreacion = DateTime.UtcNow,
                                IsActive = true
                            };

                            await _productRepository.AddAsync(newProduct);
                            productByName[nameKey] = newProduct;
                            result.ProductsCreated++;
                            currentProduct = newProduct;
                        }
                        processedAny = true;
                    }
                }

                // 3. Detectar y procesar VENTA (opcional si la fila incluye Cantidad > 0)
                var saleQtyRaw = row.GetValue("cantidad", "quantity", "cant", "cantidadvendida", "unidades", "qty", "ventacantidad");
                if (!string.IsNullOrWhiteSpace(saleQtyRaw) && int.TryParse(saleQtyRaw, out var saleQty) && saleQty > 0)
                {
                    if (currentClient == null)
                    {
                        rowErrors.Add("No se puede registrar la venta: falta asociar un Cliente válido en esta fila.");
                    }
                    else if (currentProduct == null)
                    {
                        rowErrors.Add("No se puede registrar la venta: falta asociar un Producto válido en esta fila.");
                    }
                    else if (currentProduct.Stock < saleQty)
                    {
                        rowErrors.Add($"Stock insuficiente para '{currentProduct.Name}': disponible {currentProduct.Stock}, solicitado {saleQty}.");
                    }
                    else
                    {
                        // Descontar stock y registrar venta
                        currentProduct.Stock -= saleQty;
                        await _productRepository.UpdateAsync(currentProduct);

                        var saleId = Guid.NewGuid();
                        var unitPrice = currentProduct.Price;
                        var subtotal = saleQty * unitPrice;

                        var sale = new Sale
                        {
                            Id = saleId,
                            ClientId = currentClient.Id,
                            Date = DateTime.UtcNow,
                            Total = subtotal,
                            FechaCreacion = DateTime.UtcNow,
                            IsActive = true,
                            SaleDetails = new List<SaleDetail>
                            {
                                new SaleDetail
                                {
                                    Id = Guid.NewGuid(),
                                    SaleId = saleId,
                                    ProductId = currentProduct.Id,
                                    Quantity = saleQty,
                                    UnitPrice = unitPrice,
                                    Subtotal = subtotal,
                                    FechaCreacion = DateTime.UtcNow,
                                    IsActive = true
                                }
                            }
                        };

                        await _saleRepository.AddAsync(sale);
                        result.SalesCreated++;
                        processedAny = true;
                    }
                }

                if (!hasClientData && !hasProductData)
                {
                    rowErrors.Add("La fila no contiene columnas reconocibles de Cliente ni de Producto.");
                }

                if (rowErrors.Count > 0)
                {
                    result.Errors.Add(new ExcelRowErrorDto
                    {
                        RowNumber = row.RowIndex,
                        SheetName = row.SheetName,
                        Error = string.Join(" | ", rowErrors)
                    });
                }

                if (processedAny)
                {
                    result.TotalRowsProcessed++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar fila {RowIndex} en hoja {SheetName}", row.RowIndex, row.SheetName);
                result.Errors.Add(new ExcelRowErrorDto
                {
                    RowNumber = row.RowIndex,
                    SheetName = row.SheetName,
                    Error = $"Error inesperado al procesar la fila: {ex.Message}"
                });
            }
        }

        return result;
    }

    private static decimal? ParseDecimal(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var clean = input.Replace("$", "").Replace("COP", "", StringComparison.OrdinalIgnoreCase).Trim();

        // 1. Probar invariante directo
        if (decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out var valInv))
        {
            return valInv;
        }

        // 2. Si tiene formato con separador de miles o coma decimal (ej: 35.000 o 35.000,50)
        var normalized = clean.Replace(".", "").Replace(",", ".");
        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var valNorm))
        {
            return valNorm;
        }

        try
        {
            var esCulture = new CultureInfo("es-CO");
            if (decimal.TryParse(clean, NumberStyles.Any, esCulture, out var valEs))
            {
                return valEs;
            }
        }
        catch
        {
            // Ignorar excepción en modo invariante
        }

        return null;
    }
}
