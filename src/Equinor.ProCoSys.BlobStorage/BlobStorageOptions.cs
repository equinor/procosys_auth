namespace Equinor.ProCoSys.BlobStorage
{
    public class BlobStorageOptions
    {
        private readonly string BlobStorageUrlSuffix = ".blob.core.windows.net";
        public string AccountName { get; set; }
        public string AccountDomain => AccountName + BlobStorageUrlSuffix;
        public string AccountUrl => "https://" + AccountDomain;
        public int MaxSizeMb { get; set; }
        public string BlobContainer { get; set; }
        public int BlobClockSkewMinutes { get; set; }
        public string[] BlockedFileSuffixes { get; set; }
    }
}
