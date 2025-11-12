using System.Net;
using System.Net.Mail;

namespace IPOPulse.Services
{
    public class MessageService
    {
        public Task SendMailAsync()
        {
            try
            {
                var mail = "bhedadhruv71@gmail.com";
                var pass = "#Google1234";

                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(mail, pass)
                };

                client.SendCompleted += (s, e) =>
                {
                    if (e.Error != null)
                        Console.WriteLine($"Send failed: {e.Error.Message}");
                    else if (e.Cancelled)
                        Console.WriteLine("Send canceled.");
                    else
                        Console.WriteLine("Message sent successfully.");
                };

                return client.SendMailAsync(new MailMessage(from: mail, to: "drdoom1003@gmail.com", subject: "Hi From Code", body: "This is the message generated from my code"));
            }
            catch (Exception ex) {
                Console.WriteLine($"Send failed !!!!: {ex.Message}");
                return Task.FromException(ex);
            }

        }


    }
}
