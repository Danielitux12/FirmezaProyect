namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato para el envío de notificaciones y correos electrónicos.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía el comprobante de venta al correo del cliente con el archivo PDF adjunto.
    /// </summary>
    Task SendSaleReceiptAsync(
        string toEmail,
        string clientName,
        Guid saleId,
        decimal total,
        byte[] pdfAttachmentBytes,
        string pdfFileName);
}
