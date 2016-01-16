using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobSearchHelper
{
    public interface ICloudBlockBlob
    {
        Task UploadFromByteArrayAsync(byte[] buffer, int index, int count);
        IDictionary<string, string> Metadata { get; }
        Task SetMetadataAsync();
    }

    public class CloudBlockBlobWrapper : ICloudBlockBlob
    {
        private Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob _actualBlockBlob;

        public CloudBlockBlobWrapper(Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob actualBlockBlob)
        {
            _actualBlockBlob = actualBlockBlob;

        }

        public Task UploadFromByteArrayAsync(byte[] buffer, int index, int count)
        {
            return _actualBlockBlob.UploadFromByteArrayAsync(buffer, index, count);
        }

        public IDictionary<string,string> Metadata => _actualBlockBlob.Metadata;
        public Task SetMetadataAsync()
        {
            return _actualBlockBlob.SetMetadataAsync();
        }
    }
}