using Firmeza.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Firmeza.Infrastructure.Services;

/// <summary>
/// Implementación de envío de correos vía SMTP (Gmail) usando MailKit con adjunto PDF de comprobante.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendSaleReceiptAsync(
        string toEmail,
        string clientName,
        Guid saleId,
        decimal total,
        byte[] pdfAttachmentBytes,
        string pdfFileName)
    {
        var emailSection = _configuration.GetSection("EmailSettings");
        var appPassword = emailSection["AppPassword"] ?? emailSection["SenderPassword"];
        var senderEmail = emailSection["SenderEmail"] ?? emailSection["Username"] ?? "";
        var senderName = emailSection["SenderName"] ?? "Firmeza Materiales";
        var smtpServer = emailSection["SmtpServer"] ?? "smtp.gmail.com";
        var port = int.TryParse(emailSection["Port"], out var p) ? p : 587;

        // Si la contraseña de aplicación de Gmail no está configurada aún
        if (string.IsNullOrWhiteSpace(appPassword) || 
            appPassword.Contains("tu_contraseña", StringComparison.OrdinalIgnoreCase) ||
            appPassword.Contains("tu_contrasena", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("EmailService: Las credenciales SMTP de Gmail (EmailSettings:SenderPassword o AppPassword) no están configuradas en appsettings.json. Se omite el envío real a {Email}.", toEmail);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(clientName, toEmail));
            message.Subject = $"Comprobante de Compra #{saleId.ToString().Substring(0, 8).ToUpper()} - Firmeza Materiales";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; color: #1e293b; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden;'>
                        <div style='background-color: #0f172a; padding: 20px; color: #ffffff;'>
                            <h1 style='margin: 0; font-size: 24px;'>Firmeza</h1>
                            <p style='margin: 5px 0 0 0; color: #94a3b8; font-size: 14px;'>Soluciones y Materiales para la Construcción</p>
                        </div>
                        <div style='padding: 24px;'>
                            <h2 style='color: #0f172a; font-size: 18px;'>¡Hola, {clientName}!</h2>
                            <p>Tu compra ha sido confirmada y procesada con éxito en nuestro sistema.</p>
                            <div style='background-color: #f8fafc; border-left: 4px solid #0284c7; padding: 12px 16px; margin: 20px 0;'>
                                <p style='margin: 0; font-size: 14px;'><strong>Número de Orden:</strong> #{saleId.ToString().Substring(0, 8).ToUpper()}</p>
                                <p style='margin: 5px 0 0 0; font-size: 14px;'><strong>Total:</strong> ${total:N2} COP</p>
                            </div>
                            <p>Adjunto a este correo encontrarás tu <strong>comprobante oficial de venta en formato PDF</strong> con el desglose detallado de tus productos e IVA.</p>
                            <p style='color: #64748b; font-size: 13px; margin-top: 24px;'>Si tienes alguna pregunta sobre tu pedido, contáctanos a través de soporte@firmeza.com.</p>
                        </div>
                        <div style='background-color: #f1f5f9; padding: 12px; text-align: center; color: #64748b; font-size: 12px;'>
                            © {DateTime.UtcNow.Year} Firmeza. Todos los derechos reservados.
                        </div>
                    </div>"
            };

            if (pdfAttachmentBytes != null && pdfAttachmentBytes.Length > 0)
            {
                builder.Attachments.Add(pdfFileName, pdfAttachmentBytes, ContentType.Parse("application/pdf"));
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, appPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Comprobante de venta #{SaleId} enviado exitosamente por correo a {Email}", saleId, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar el correo con el comprobante de venta a {Email}", toEmail);
            // No relanzamos la excepción para preservar la venta registrada
        }
    }
}
