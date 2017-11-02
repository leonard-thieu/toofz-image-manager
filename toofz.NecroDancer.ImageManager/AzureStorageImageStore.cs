#if FEATURE_IMAGEFILE
using System;
using System.Collections.Generic;
using System.Linq;
#endif
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace toofz.NecroDancer.ImageManager
{
    internal sealed class AzureStorageImageStore
    {
        public AzureStorageImageStore(string storageConnectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            blobClient = storageAccount.CreateCloudBlobClient();
        }

        private readonly CloudBlobClient blobClient;
        private CloudBlobContainer container;

        private async Task InitializeAsync()
        {
            if (container != null) { return; }

            container = blobClient.GetContainerReference("crypt");
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);
            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob,
            };
            await container.SetPermissionsAsync(permissions).ConfigureAwait(false);
        }

#if FEATURE_IMAGEFILE
        public async Task SaveImageAsync(ImageFile image)
        {
            await InitializeAsync().ConfigureAwait(false);

            var blockBlob = container.GetBlockBlobReference(image.Name);
            blockBlob.Properties.ContentType = "image/png";
            blockBlob.Properties.CacheControl = "max-age=604800";
            await blockBlob.UploadFromByteArrayAsync(image.Data, 0, image.Data.Length).ConfigureAwait(false);

            Console.WriteLine($"Uploaded {image.Name}.");
        }

        public Task SaveImagesAsync(IEnumerable<ImageFile> images)
        {
            var tasks = images.Select(SaveImageAsync);

            return Task.WhenAll(tasks);
        }
#endif
    }
}
