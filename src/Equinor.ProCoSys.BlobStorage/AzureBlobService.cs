﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.BlobStorage
{
    public class AzureBlobService : IAzureBlobService
    {
        private class ResourceTypes
        {
            public const string BLOB = "b";
            public const string CONTAINER = "c";
        }
        private readonly string _connectionString;
        private readonly string _endpoint;
        private readonly string _accountName;
        private readonly string _accountKey;

        public AzureBlobService(IOptionsSnapshot<BlobStorageOptions> options)
        {
            if (string.IsNullOrEmpty(options.Value.ConnectionString))
            {
                throw new ArgumentNullException(nameof(options.Value.ConnectionString));
            }

            _connectionString = options.Value.ConnectionString;
            _accountName = Regex.Match(_connectionString, @"AccountName=(.+?)(;|\z)", RegexOptions.Singleline).Groups[1].Value;
            _accountKey = Regex.Match(_connectionString, @"AccountKey=(.+?)(;|\z)", RegexOptions.Singleline).Groups[1].Value;
            _endpoint = "blob." + Regex.Match(_connectionString, @"EndpointSuffix=(.+?)(;|\z)", RegexOptions.Singleline).Groups[1].Value;
        }

        public async Task<bool> DownloadAsync(string container, string blobPath, Stream destination, CancellationToken cancellationToken = default)
        {
            var client = new BlobClient(_connectionString, container, blobPath);
            var res = await client.DownloadToAsync(destination, cancellationToken);
            return res.Status > 199 && res.Status < 300;
        }

        public async Task UploadAsync(string container, string blobPath, Stream content, bool overWrite = false, CancellationToken cancellationToken = default)
        {
            var client = new BlobClient(_connectionString, container, blobPath);
            await client.UploadAsync(content, overWrite, cancellationToken);
        }

        public async Task<bool> DeleteAsync(string container, string blobPath, CancellationToken cancellationToken = default)
        {
            var client = new BlobClient(_connectionString, container, blobPath);
            var res = await client.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, cancellationToken);
            return res.Value;
        }

        public async Task<List<string>> ListAsync(string container, CancellationToken cancellationToken = default)
        {
            var client = new BlobContainerClient(_connectionString, container);
            var blobNames = new List<string>();
            await foreach (var blob in client.GetBlobsAsync(BlobTraits.None, BlobStates.None, null, cancellationToken))
            {
                blobNames.Add(blob.Name);
            }
            return blobNames;
        }

        public Uri GetDownloadSasUri(string container, string blobPath, DateTimeOffset startsOn, DateTimeOffset expiresOn)
        {
            var sasToken = GetSasToken(container, blobPath, ResourceTypes.BLOB, BlobAccountSasPermissions.Read, startsOn, expiresOn);
            var fullUri = new UriBuilder
            {
                Scheme = "https",
                Host = string.Format($"{_accountName}.{_endpoint}"),
                Path = Path.Combine(container, blobPath),
                Query = sasToken
            };
            return fullUri.Uri;
        }

        public Uri GetUploadSasUri(string container, string blobPath, DateTimeOffset startsOn, DateTimeOffset expiresOn)
        {
            var sasToken = GetSasToken(container, blobPath, ResourceTypes.BLOB, BlobAccountSasPermissions.Create | BlobAccountSasPermissions.Write, startsOn, expiresOn);
            var fullUri = new UriBuilder
            {
                Scheme = "https",
                Host = string.Format($"{_accountName}.{_endpoint}"),
                Path = Path.Combine(container, blobPath),
                Query = sasToken
            };
            return fullUri.Uri;
        }

        public Uri GetDeleteSasUri(string container, string blobPath, DateTimeOffset startsOn, DateTimeOffset expiresOn)
        {
            var sasToken = GetSasToken(container, blobPath, ResourceTypes.BLOB, BlobAccountSasPermissions.Delete, startsOn, expiresOn);
            var fullUri = new UriBuilder
            {
                Scheme = "https",
                Host = string.Format($"{_accountName}.{_endpoint}"),
                Path = Path.Combine(container, blobPath),
                Query = sasToken
            };
            return fullUri.Uri;
        }

        public Uri GetListSasUri(string container, DateTimeOffset startsOn, DateTimeOffset expiresOn)
        {
            var sasToken = GetSasToken(container, string.Empty, ResourceTypes.CONTAINER, BlobAccountSasPermissions.List, startsOn, expiresOn);
            var fullUri = new UriBuilder
            {
                Scheme = "https",
                Host = string.Format($"{_accountName}.{_endpoint}"),
                Path = container,
                Query = $"restype=container&comp=list&{sasToken}"
            };
            return fullUri.Uri;
        }

        private string GetSasToken(string containerName, string blobName, string resourceType, BlobAccountSasPermissions permissions, DateTimeOffset startsOn, DateTimeOffset expiresOn)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = resourceType,
                StartsOn = startsOn,
                ExpiresOn = expiresOn
            };
            sasBuilder.SetPermissions(permissions);
            return sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_accountName, _accountKey)).ToString();
        }
    }
}
