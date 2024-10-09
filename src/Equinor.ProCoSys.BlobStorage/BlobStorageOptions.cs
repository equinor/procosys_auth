namespace Equinor.ProCoSys.BlobStorage
{
    public class BlobStorageOptions
    {
        public string BlobStorageAccountUrl { get; set; }
        public string BlobStorageAccountName { get; set; }
        public int MaxSizeMb { get; set; }
        public string BlobContainer { get; set; }
        public int BlobClockSkewMinutes { get; set; }
        public string[] BlockedFileSuffixes { get; set; }
    }
}
