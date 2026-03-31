using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using PrivateECommerce.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        try
        {
            var host = _config["Smtp:Host"];
            var port = _config["Smtp:Port"];
            var user = _config["Smtp:User"];
            var pass = _config["Smtp:Pass"];

            Console.WriteLine("SMTP HOST = " + host);
            Console.WriteLine("SMTP PORT = " + port);
            Console.WriteLine("SMTP USER = " + user);
            Console.WriteLine("SMTP PASS LENGTH = " + (pass?.Length ?? 0));

            using var smtp = new SmtpClient(host, int.Parse(port))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass),
                Timeout = 10000
            };

            Console.WriteLine("SMTP CLIENT CREATED");

            using var message = new MailMessage
            {
                From = new MailAddress(user, "Medico KSA"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);

            Console.WriteLine("SENDING EMAIL NOW");

            await smtp.SendMailAsync(message);

            Console.WriteLine("EMAIL SENT SUCCESSFULLY");
        }
        catch (Exception ex)
        {
            Console.WriteLine("EMAIL ERROR:");
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}