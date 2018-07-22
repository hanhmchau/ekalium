using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kalium.Shared.Consts;
using Kalium.Shared.Email;
using Kalium.Shared.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MoreLinq;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Kalium.Server.Repositories
{
    public class AuthMessageSenderOptions
    {
        public string SendGridUser { get; set; }
        public string SendGridKey { get; set; }
    }

    public class EmailSender: IEmailSender  
    {
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(Options.SendGridKey, subject, message, email);
        }

//        public Task SendEmailAsync(ICollection<string> email, string subject, string message)
//        {
//            return Execute(Options.SendGridKey, subject, message, email);
//        }

        public async Task<bool> SendProductEmail(string subject, Product product, ICollection<string> email)
        {
            var client = new SendGridClient(Options.SendGridKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(Consts.Email.Address, Consts.Email.Sender),
                Subject = subject,
                TemplateId = "fbc1a4b9-ba73-42cb-a8ac-17569030a01e",
//                HtmlContent = message
            };

            msg.Personalizations = new List<Personalization>();
            email.ForEach(e =>
            {
                var p = new Personalization
                {
                    Substitutions = new Dictionary<string, string>()
                };
                p.Substitutions.Add("{{title}}", subject);
                p.Substitutions.Add("{{today}}", DateTime.Today.ToString("dd.MM.yyyy"));
                p.Substitutions.Add("{{productName}}", product.Name);
                var imageUrl = product.MainImage?.Url;
                if (imageUrl == null)
                {
                    imageUrl = "http://www.stampready.net/dashboard/editor/user_uploads/zip_uploads/2018/01/09/huiGyY6IU5oem731WAQgBHz4/StampReady/img/pic-left-bg.jpg";
                }
                if (!imageUrl.Contains("http"))
                {
                    imageUrl = $"http://localhost:52747/{product.MainImage?.Url.Replace("\\", "/")}";
                }
                p.Substitutions.Add("{{image}}", imageUrl);
                p.Substitutions.Add("{{description}}", product.Description);
                p.Substitutions.Add("{{normalPrice}}", product.DiscountedPrice.ToString("N1"));
                p.Substitutions.Add("{{lowerPrice}}", Math.Max(product.DiscountedPrice - 10, 0).ToString("N1"));
                p.Substitutions.Add("{{higherPrice}}", (product.DiscountedPrice + 10).ToString("N1"));
                p.Substitutions.Add("{{productUrl}}", product.NameUrl);
                msg.Personalizations.Add(p);
            });

            var addresses = email.Select(e => new EmailAddress(e)).ToList();
            msg.AddTos(addresses);

            msg.TrackingSettings = new TrackingSettings
            {
                ClickTracking = new ClickTracking { Enable = false }
            };

            var response = await client.SendEmailAsync(msg);
            var statusCode = (int)response.StatusCode;
            return statusCode >= 200 && statusCode < 300;
        }


        public async Task<bool> SendOrderEmail(string subject, Order order, string email)
        {
            var client = new SendGridClient(Options.SendGridKey);
            var orderEmail = new OrderEmail
            {
                Order = order
            };
            var content = orderEmail.GetContent();
            var items = orderEmail.GetItems();
            content = content
                .Replace("{row}", items)
                .Replace("{title}", subject)
                .Replace("{total}", order.PostCouponTotal.ToString("N1"))
                .Replace("{address}", order.Address)
                .Replace("{username}", order.User.UserName)
                .Replace("{orderId}", order.Id.ToString())
                .Replace("{date}", order.DateCreated.ToLongDateString());
            var msg = new SendGridMessage
            {
                From = new EmailAddress(Consts.Email.Address, Consts.Email.Sender),
                Subject = subject,
//                TemplateId = "39dfd02b-b1dc-4f26-b032-878c5e4d3b77"
                HtmlContent = content
            };

//            var content = new OrderEmail {Order = order}.GetContent();
//            var p = new Personalization
//            {
//                Substitutions = new Dictionary<string, string>()
//            };
            msg.AddTo(new EmailAddress(email));

            msg.TrackingSettings = new TrackingSettings
            {
                ClickTracking = new ClickTracking { Enable = false }
            };

            var response = await client.SendEmailAsync(msg);
            var statusCode = (int) response.StatusCode;
            return statusCode >= 200 && statusCode < 300;
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(Consts.Email.Address, Consts.Email.Sender),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            msg.TrackingSettings = new TrackingSettings
            {
                ClickTracking = new ClickTracking { Enable = false }
            };

            return client.SendEmailAsync(msg);
        }
    }
}

