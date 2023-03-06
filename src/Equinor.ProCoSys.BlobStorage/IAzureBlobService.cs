using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.BlobStorage
{
    public interface IAzureBlobService
    {
        Task<bool> DownloadAsync(string container, string blobPath, Stream destination, CancellationToken cancellationToken = default);
        Task UploadAsync(string container, string blobPath, Stream content, bool overWrite = false, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string container, string blobPath, CancellationToken cancellationToken = default);
        Task<List<string>> ListAsync(string container, CancellationToken cancellationToken = default);

        Uri GetDownloadSasUri(string container, string blobPath, DateTimeOffset startsOn, DateTimeOffset expiresOn);
        Uri GetUploadSasUri(string container, string blobPath, DateTimeOffset startsOn, DateTimeOffset expiresOn);
        Uri GetDeleteSasUri(string container, string blobPath, DateTimeOffset startsOn, DateTimeOffset expiresOn);
        Uri GetListSasUri(string container, DateTimeOffset startsOn, DateTimeOffset expiresOn);
    }
}
