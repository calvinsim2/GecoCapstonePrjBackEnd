using CapstoneProjectBlog.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CapstoneProjectBlog.Services
{
    public class MailService : IMailService
    {
        public readonly IConfiguration _config;

        public MailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendEmailAsync(MailModel mailModel)
        {
            // mailRequest (model) ---> email (MimeMessage) -- so we can send emails. 
            // we have to import MimeKit to use MimeMessage
            var email = new MimeMessage();
            // _config, we need this because we are using config files in appsettings.json
            email.Sender = MailboxAddress.Parse(_config["MailSettings:Mail"]);

            // to whom we are sending to, it will come from the body.
            email.To.Add(MailboxAddress.Parse(mailModel.ToEmail));
            // subject will be coming from the body as well.
            email.Subject = mailModel.Subject;

            // BodyBuilder comes from MimeKit

            var builder = new BodyBuilder();

            if (mailModel.Attachments != null)
            {
                // if there is file attachment, we create an array of bytes.
                byte[] fileBytes;
                // remember in model, attachement is a list. 
                foreach (var file in mailModel.Attachments)
                {
                    if (file.Length > 0)
                    {
                        // MemoryStream helps to copy the file, which we store in our array of bytes.
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        // file name, file bytes and the content type, which is an email. 
                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }

            // we store the body, into this builder.HtmlBody
            builder.HtmlBody = mailModel.Body;
            // we pass the body into our email, which is a MimeMessage
            email.Body = builder.ToMessageBody();

            // To use smtp, we need to import from MailKit.Net.Smtp
            // sending a mail to smtp server. etc, gmail has a host, which is 587.
            using var smtp = new SmtpClient();
            // StartTls is an encryption model, to protect our data.
            smtp.Connect(_config["MailSettings:Host"], Convert.ToInt32(_config["MailSettings:Port"]), SecureSocketOptions.StartTls);
            // sender email and password.
            smtp.Authenticate(_config["MailSettings:Mail"], _config["MailSettings:Password"]);
            await smtp.SendAsync(email);
            // once we sent our email, we need to remember to disconnect our server.
            smtp.Disconnect(true);
        }
    }
}
