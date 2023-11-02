using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Equinor.ProCoSys.Common.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;

        private readonly ILogger _logger;
        private readonly string _mailCredentialTenantId;
        private readonly string _mailCredentialClientId;
        private readonly string _mailCredentialSecret;
        private readonly string _mailUserOid;

        public EmailService(IOptionsMonitor<EmailOptions> emailOptions, IOptionsMonitor<GraphOptions> graphOptions, ILogger<EmailService> logger)
        {
            _mailCredentialTenantId = graphOptions.CurrentValue.TenantId;
            _mailCredentialClientId = graphOptions.CurrentValue.ClientId;
            _mailCredentialSecret = graphOptions.CurrentValue.ClientSecret;
            _mailUserOid = emailOptions.CurrentValue.MailUserOid;

            _logger = logger;
            _emailOptions = emailOptions.CurrentValue;
        }

        public async Task SendEmailsAsync(List<string> emails, string subject, string body,
            CancellationToken token = default)
        {
            EmailValidator.ValidateEmails(emails);

            var credentials = new ClientSecretCredential(
                _mailCredentialTenantId,
                _mailCredentialClientId,
                _mailCredentialSecret
                );

            var graphServiceClient = new GraphServiceClient(credentials);
            var graphMessage = new Message
            {
                From = new Recipient {
                    EmailAddress = new EmailAddress { Address = emails[0] }
                },
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = body
                },
                ToRecipients = emails.Skip(1).Select(x => new Recipient { EmailAddress = new EmailAddress { Address = x } }).ToList()
            };
            try
            {
                await graphServiceClient.Users[_mailUserOid]
                       .SendMail
                       .PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody()
                       {
                           Message = graphMessage,
                           SaveToSentItems = false
                       });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"An email with subject {subject.Substring(0,25)} could not be sent.");
                throw new Exception($"It was not possible to send an email (subject: {subject.Substring(0,25)})", ex);
            }
        }

        public async Task SendMessageAsync(Message graphMessage,
            CancellationToken token = default)
        {
            EmailValidator.ValidateEmails(graphMessage.ToRecipients.Select(x => x.EmailAddress.Address).ToList());

            var credentials = new ClientSecretCredential(
                _mailCredentialTenantId,
                _mailCredentialClientId,
                _mailCredentialSecret
                );

            var graphServiceClient = new GraphServiceClient(credentials);

            await graphServiceClient.Users[_mailUserOid]
                .SendMail
                .PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody() {
                    Message = graphMessage, 
                    SaveToSentItems = false 
                });
        }
    }
}
