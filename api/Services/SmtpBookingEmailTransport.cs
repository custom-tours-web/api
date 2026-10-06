using System.Net;
using System.Net.Mail;
using api.Interfaces;

namespace api.Services;

public class SmtpBookingEmailTransport : IBookingEmailTransport
{
    public async Task SendAsync(string host, int port, NetworkCredential credentials, MailMessage message)
    {
        using var smtpClient = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = credentials
        };

        await smtpClient.SendMailAsync(message);
    }
}
