using Tavu.Exceptions;
using Microsoft.Extensions.Options;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Identity;

namespace Tavu.Storage
{
    public interface IBlobStore
    {
        Task<string?> GetBlobContentAsync(string path);
        IAsyncEnumerable<string> GetBlobListAsync(string path);
        Task SetBlobAsync(string path, string content);
        Task DeleteBlobAsync(string path);
    }

    public class AzureStorageOptions
    {
        public const string AzureStorage = "AzureStorage";
        public string BlobServiceUri {get; set; } = string.Empty;
        public string AccountKey {get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
    }

    public class AzureStorageBlobStore : IBlobStore
    {
        public string BlobServiceUri { get; private set; }
        private string? AccountKey { get; set; }
        public string ContainerName { get; set; }

        private Lazy<Task<BlobContainerClient>> containerClient;

        public AzureStorageBlobStore(IOptions<AzureStorageOptions> options)
        {
            var optionsValue = options.Value ?? throw new TavuServiceConfigurationException();
            this.BlobServiceUri = !string.IsNullOrWhiteSpace(optionsValue.BlobServiceUri) 
                ? optionsValue.BlobServiceUri
                : throw new TavuServiceConfigurationException();
            this.ContainerName = !string.IsNullOrWhiteSpace(optionsValue.ContainerName) 
                ? optionsValue.ContainerName
                : throw new TavuServiceConfigurationException();
            this.AccountKey = optionsValue.AccountKey;
            containerClient = new (GetContainerClientAsync, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public async IAsyncEnumerable<string> GetBlobListAsync(string path)
        {
            var client = await this.containerClient.Value;
            var enumerator = client.GetBlobsAsync(prefix: path)
                .ConfigureAwait(false)
                .GetAsyncEnumerator();

            while (await enumerator.MoveNextAsync())
            {
                yield return enumerator.Current.Name;
            }
        }

        public async Task<string?> GetBlobContentAsync(string path)
        {
            var client = await this.containerClient.Value;
            var blobClient = client.GetBlobClient(path);
            if (await blobClient.ExistsAsync())
            {
                var blob = await blobClient.DownloadContentAsync().ConfigureAwait(false);
                return blob?.Value.Content.ToString();
            }
            
            return null;
        }

        public async Task SetBlobAsync(string path, string content)
        {
            var client = await this.containerClient.Value;
            
        }

        public Task DeleteBlobAsync(string path)
        {
            throw new NotImplementedException();
        }

        private async Task<BlobContainerClient> GetContainerClientAsync()
        {
            BlobServiceClient blobClient;
            if(string.IsNullOrWhiteSpace(this.AccountKey))
            {
                blobClient = new(new Uri(this.BlobServiceUri), new DefaultAzureCredential());
            }
            else
            {
                Uri accountUri = new Uri(this.BlobServiceUri);
                    string accountName = accountUri.Host.Split('.')[0];

                StorageSharedKeyCredential sharedKeyCredential =
                new StorageSharedKeyCredential(accountName, this.AccountKey);

                blobClient = new BlobServiceClient(accountUri, sharedKeyCredential);
            }

            var containerClient = blobClient.GetBlobContainerClient(this.ContainerName);
            await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
            return containerClient;
        }
    }
}