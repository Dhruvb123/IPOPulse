using IPOPulse.DBContext;
using MimeKit;
using MailKit.Net.Smtp;


namespace IPOPulse.Services
{
    public class MailService : IMessageService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDBContext _context;
        public MailService(IConfiguration configuration, AppDBContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        public async Task SendMailAsync(string subject, string stockName, string stockSymbol, string price)
        {
            try
            {
                var customers = _context.Users.ToList();
                foreach (var customer in customers)
                {
                    var mailSettings = _configuration.GetSection("EmailSettings");

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(
                        mailSettings["SenderName"],
                        mailSettings["SenderEmail"]
                    ));
                    message.To.Add(MailboxAddress.Parse(customer.Email));
                    message.Subject = subject;
                    string body = "";
                    if (subject.Contains("Opportunity"))
                    {
                        body = $"""
                                Dear {customer.Name},

                                I wanted to alert you that {stockName}({stockSymbol}) has entered a solid bull run. 
                    
                                Currently at {price} it is showing strong momentum and short-term upside potential. This is a prime opportunity to consider buying before the rally gains further steam.

                                Given current market dynamics, this move could offer quick gains. Don't worry we got you covered with Sell Alerts as well.

                                Best regards,
                                IPOPulse.
                                """;
                    }
                    else
                    {
                        if (subject.Contains("Alert"))
                        {

                            body = $"""
                                Dear {customer.Name},

                                {stockName}({stockSymbol}) has just crossed below a key support level, signaling potential downside risk ahead.
                                This is a good moment to consider selling to avoid further losses by selling it at {price}.

                                Best regards,
                                IPOPulse.
                                """;
                        }
                        else
                        {
                            body = $"""
                                Dear {customer.Name},

                                {stockName}({stockSymbol}) has just given strong profits in last few days.

                                This is a good moment to consider selling it at {price}, thus booking decent profits.

                                Best regards,
                                IPOPulse.
                                """;
                        }
                    }
                    message.Body = new TextPart("html")
                    {
                        Text = body
                    };

                    using (var client = new SmtpClient())
                    {
                        // Connect to the SMTP server
                        await client.ConnectAsync(
                            mailSettings["Server"],
                            int.Parse(mailSettings["Port"]),
                            MailKit.Security.SecureSocketOptions.StartTls
                        );

                        // Authenticate with app password
                        await client.AuthenticateAsync(
                            mailSettings["Username"],
                            mailSettings["Password"]
                        );

                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);

                        Console.WriteLine("Email sent successfully!");
                    }
                }

            }
            catch (Exception ex) {
                Console.WriteLine("Could not send email.\nException: "+ex.ToString());
                throw;
            }

        }


    }
}
