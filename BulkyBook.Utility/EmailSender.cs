using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {

        private readonly EmailSettings _emailSettings;

        public EmailSender(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
        }


        //MAIN METHOD WHICH WE ARE GOING TO USE TO SEND MESSAGE IN DIFFERNT CLASSES
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Message message = new Message(email, subject, htmlMessage);
         
            var emailMessage = CreateEmailMessage(message);

            await SendAsync(emailMessage);
        }


        //FUNCTION FOR CREATING EMAIL MESSAGE 
        private MimeMessage CreateEmailMessage(Message message)
        {

            //CREATING MESSAGR WITH MimeMessage class
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Content };

            return emailMessage;

        }


        //FUNCTION FOR SEDNING EMAIL 
        private async Task SendAsync(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await  client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);

                    client.Send(mailMessage);
                }
                catch
                {
                    throw;
                }
                finally
                {
                   await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }

        }



    }
}
