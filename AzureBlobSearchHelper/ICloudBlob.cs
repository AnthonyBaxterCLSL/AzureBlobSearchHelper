using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobSearchHelper
{
    public interface ICloudBlob
    {
        Task FetchAttributesAsync();
        IDictionary<string,string> Metadata { get; }
        int StreamMinimumReadSizeInBytes { get; set; }
        Task<int> DownloadToByteArrayAsync(byte[] target, int index);
        Task<bool> DeleteIfExistsAsync();
        Task<bool> ExistsAsync();
        Task SetMetadataAsync();
    }

    public class CloudBlobWrapper : ICloudBlob
    {

        private CloudBlob _actualBlob;
        public CloudBlobWrapper(CloudBlob actualBlob)
        {
            _actualBlob = actualBlob;
        }

        public Task<int> DownloadToByteArrayAsync(byte[] target, int index)
        {
            return _actualBlob.DownloadToByteArrayAsync(target, index);
        }

        public Task<bool> ExistsAsync()
        {
            return _actualBlob.ExistsAsync();
        }

        public Task FetchAttributesAsync()
        {
            return _actualBlob.FetchAttributesAsync();
        }

        public Task SetMetadataAsync()
        {
            return _actualBlob.SetMetadataAsync();
        }

        public Task<bool> DeleteIfExistsAsync()
        {
            return _actualBlob.DeleteIfExistsAsync();
        }

        public IDictionary<string, string> Metadata => _actualBlob.Metadata;

        public int StreamMinimumReadSizeInBytes
        {
            get { return _actualBlob.StreamMinimumReadSizeInBytes; }
            set { _actualBlob.StreamMinimumReadSizeInBytes = value; }
        }
    }
}