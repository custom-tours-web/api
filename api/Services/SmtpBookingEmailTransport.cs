using System.Net;
using System.Net.Mail;
using api.Interfaces;

namespace api.Services;

/// <summary>
/// Provides an implementation of the IBookingEmailTransport interface for sending emails using SMTP.
/// </summary>
public class SmtpBookingEmailTransport : IBookingEmailTransport
{

    /// <summary>
    /// Sends an email using the specified SMTP server details and message content.
    /// </summary>
    /// <param name="host">The SMTP server host.</param>
    /// <param name="port">The SMTP server port.</param>
    /// <param name="credentials">The credentials for authenticating with the SMTP server.</param>
    /// <param name="message">The email message to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
