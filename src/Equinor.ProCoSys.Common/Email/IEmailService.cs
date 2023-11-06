using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Common.Email
{
    public interface IEmailService
    {
        Task SendEmailsAsync(List<string> emails, string subject, string body, CancellationToken token = default);
        Task SendMessageAsync(Message graphMessage, CancellationToken token = default);

    }
}
