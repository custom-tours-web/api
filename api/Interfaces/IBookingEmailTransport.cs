using System.Net;
using System.Net.Mail;

namespace api.Interfaces;

public interface IBookingEmailTransport
{
    Task SendAsync(string host, int port, NetworkCredential credentials, MailMessage message);
}
