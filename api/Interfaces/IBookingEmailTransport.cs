using System.Net;
using System.Net.Mail;

namespace api.Interfaces;

/// <summary>
/// Defines a contract for sending emails related to booking requests.
/// </summary>
public interface IBookingEmailTransport
{

    /// <summary>
    /// Sends an email using the specified SMTP server details & message content.
    /// </summary>
    /// <param name="host">The SMTP server host.</param>
    /// <param name="port">The SMTP server port.</param>
    /// <param name="credentials">The SMTP server credentials.</param>
    /// <param name="message">The email message to send.</param>
    Task SendAsync(string host, int port, NetworkCredential credentials, MailMessage message);
}
