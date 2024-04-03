﻿using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth.Authentication
{
    public interface IBearerTokenProvider
    {
        ValueTask<string> GetBearerTokenAsync(CancellationToken cancellationToken = default);
        AuthenticationType AuthenticationType { get; set; }
    }
}
