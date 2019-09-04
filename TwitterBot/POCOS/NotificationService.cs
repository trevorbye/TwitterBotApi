using TwitterBot.Models;
using System.Net.Mail;
using System.Net;

namespace TwitterBot.POCOS
{
    public class NotificationService
    {
        public static void SendNotificationToHandle(TweetQueue tweetQueue)
        {
            SmtpClient client = new SmtpClient("smtp.office365.com", 587);
            client.Credentials = new NetworkCredential("tweet@microsoft.com", "5tyJ)7jx3220!pWn6");
            client.EnableSsl = true;

            MailMessage message = new MailMessage();
            message.From = new MailAddress("tweet@microsoft.com");
            message.IsBodyHtml = true;
            message.To.Add(tweetQueue.HandleUser);
            message.Subject = "MS Twitter Bot Notification";

            string body = "A tweet has been routed to your Twitter handle " +
                "<strong>" + tweetQueue.TwitterHandle + "</strong>" +
                " requested by user " + 
                tweetQueue.TweetUser + ". " +
                "Sign in to your account to approve. <br /> " +
                "<br /> " +
                "https://mstwitterbot.azurewebsites.net/";

            message.Body = body;
            client.Send(message);
        }
    }
}