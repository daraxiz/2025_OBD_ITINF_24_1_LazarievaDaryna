using System.Net;
using System.Net.Mail;

namespace Mankura.Services
{
    public class EmailService
    {
        public void Send(string to, string subject, string body)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(
                    "lazarevadarina629@gmail.com",
                    "hztqtnxjnjmmdtys" 
                ),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress("lazarevadarina629@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mail.To.Add(to);

            client.Send(mail);
        }
    }
}
