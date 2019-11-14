using TwitterBot.Models;
using System.Net.Mail;
using System.Net;
using System;

namespace TwitterBot.POCOS
{
    public class NotificationService
    {
        static readonly string SmtpPass = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

        public static void SendNotificationToHandle(TweetQueue tweetQueue) =>
            SendMessage(
                tweetQueue,
                () => "A tweet has been routed to your Twitter handle " +
                    "<strong>" + tweetQueue.TwitterHandle + "</strong>" +
                    " requested by user " +
                    tweetQueue.TweetUser + ". " + "Requested tweet body: " + "<br /> <br />" +
                    "<em>" + tweetQueue.StatusBody + "</em>" + "<br /> <br />" +
                    "Scheduled for: " + tweetQueue.ScheduledStatusTime + " UTC" +
                    "<br /> <br />" +
                    "Sign in to your account to approve. <br /> " +
                    "https://aka.ms/tweet");

        public static void SendApprovalNotif(TweetQueue tweetQueue) =>
            SendMessage(
                tweetQueue,
                () => "The handle owner of account " +
                    "<strong>" + tweetQueue.TwitterHandle + "</strong>" +
                    " has <strong>approved</strong> your tweet: <br />" +
                    "<br />" +
                    "<em>" + tweetQueue.StatusBody + "</em>" + "<br /> " +
                    "<br />" +
                    "Scheduled for: " + tweetQueue.ScheduledStatusTime + " UTC" +
                    "<br /> " +
                    "https://aka.ms/tweet");

        public static void SendCancelNotif(TweetQueue tweetQueue) =>
            SendMessage(
                tweetQueue,
                () => "The handle owner of account " +
                    "<strong>" + tweetQueue.TwitterHandle + "</strong>" +
                    " has <strong>cancelled</strong> your tweet: <br />" +
                    "<br />" +
                    "<em>" + tweetQueue.StatusBody + "</em>" + "<br /> " +
                    "<br />" +
                    "Scheduled for: " + tweetQueue.ScheduledStatusTime + " UTC" +
                    "<br /> " +
                    "https://aka.ms/tweet");

        public static void SendEditNotif(TweetQueue tweetQueue, string originalStatus) => 
            SendMessage(
                tweetQueue, 
                () => "The handle owner of account " +
                    "<strong>" + tweetQueue.TwitterHandle + "</strong>" +
                    " has <strong>edited</strong> your tweet. <br />" +
                    "<br />" + "Updated status: " + "<br />" + "<br />" +
                    "<em>" + tweetQueue.StatusBody + "</em>" + "<br /> " +
                    "<br />" + "<br />" +
                    "Original status: " + "<br />" + "<br />" +
                    "<em>" + originalStatus + "</em>" + "<br /> " + "<br />" +
                    "Scheduled for: " + tweetQueue.ScheduledStatusTime + " UTC" +
                    "<br /> " +
                    "https://aka.ms/tweet");

        static void SendMessage(TweetQueue tweetQueue, Func<string> createBodyText)
        {
            using (var client = new SmtpClient("smtp.office365.com", 587)
                                {
                                    Credentials = new NetworkCredential("tweet@microsoft.com", SmtpPass),
                                    EnableSsl = true
                                })
            {
                var message = new MailMessage
                {
                    From = new MailAddress("tweet@microsoft.com"),
                    IsBodyHtml = true,
                    Subject = "MS Twitter Bot Notification"
                };
                message.To.Add(tweetQueue.TweetUser);
                message.Body = createBodyText();
                client.Send(message);
            }
        }
    }
}