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
            var displayName = _config["Smtp:DisplayName"] ?? "Medico KSA";

            Console.WriteLine("EMAIL STEP 1: Config loaded");

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(port) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(pass))
            {
                Console.WriteLine("EMAIL ERROR: SMTP config missing");
                return;
            }

            using var smtp = new SmtpClient(host, int.Parse(port))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 10000
            };

            Console.WriteLine("EMAIL STEP 2: SMTP ready");

            using var message = new MailMessage
            {
                From = new MailAddress(user, displayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);

            Console.WriteLine("EMAIL STEP 3: Sending email");

            await smtp.SendMailAsync(message);

            Console.WriteLine("EMAIL STEP 4: Email sent successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine("EMAIL ERROR: " + ex.Message);
        }
    }
}