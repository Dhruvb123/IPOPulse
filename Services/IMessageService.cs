namespace IPOPulse.Services
{
    public interface IMessageService
    {
       Task SendMailAsync(string subject, string stockName, string stockSymbol, string price);
    }
}
