using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.Utility
{
    public class Message
    {

        //Construtor For Sending Message to Single Person
        public Message(string to, string subject, string content)
        {
            To = new List<MailboxAddress>();

            //Adding  MailboxAddress Objects in In list oF MailboxAddress
            To.Add(new MailboxAddress(to));

            Subject = subject;
            Content = content;

        }


        //Construtor For Sending Message to Multiple Persons
        public Message(IEnumerable<string> to , string subject , string content)
        {
            To = new List<MailboxAddress>();

            //Adding  MailboxAddress Objects in In list oF MailboxAddress
            To.AddRange(to.Select (x=> new MailboxAddress(x)));

            Subject = subject;
            Content = content;

        }

        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }

        public string Content { get; set; }
    }
}
