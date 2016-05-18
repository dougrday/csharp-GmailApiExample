using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;

namespace GmailApiExample
{
    class Program
    {
        private static void SendIt(BaseClientService.Initializer initializer)
        {
            var msg = new AE.Net.Mail.MailMessage
            {
                ContentType = "text/html",
                Subject = "Your Subject",
                Body = @"<html>
<body>
Hello <strong>World!</strong>
</body>
</html>",
                From = new MailAddress("your-email@gmail.com")
            };
            msg.To.Add(new MailAddress("buddy-email@gmail.com"));
            msg.ReplyTo.Add(msg.From); // Bounces without this!!
            var msgStr = new StringWriter();
            msg.Save(msgStr);

            var gmail = new GmailService(initializer);
            var result = gmail.Users.Messages.Send(new Message
            {
                Raw = Base64UrlEncode(msgStr.ToString())
            }, "me").Execute();
            Console.WriteLine("Message ID {0} sent.", result.Id);
        }

        private static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }

        static void Main(string[] args)
        {
            // Request Gmail IMAP/SMTP scope and the e-mail address scope.
            string[] scopes = new string[] { "https://mail.google.com/", Oauth2Service.Scope.UserinfoEmail };

            Console.WriteLine("Requesting authorization");
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = "client-id-here",
                    ClientSecret = "client-secret-here"
                },                
                scopes,
                "your-email@gmail.com",
                CancellationToken.None).Result;

            Console.WriteLine("Authorization granted or not required (if the saved access token already available)");

            if (credential.Token.IsExpired(credential.Flow.Clock))
            {
                Console.WriteLine("The access token has expired, refreshing it");
                if (credential.RefreshTokenAsync(CancellationToken.None).Result)
                {
                    Console.WriteLine("The access token is now refreshed");
                }
                else
                {
                    Console.WriteLine("The access token has expired but we can't refresh it :(");
                    return;
                }
            }
            else
            {
                Console.WriteLine("The access token is OK, continue");
            }

            SendIt(new BaseClientService.Initializer() { HttpClientInitializer = credential });            
        }
    }
}
