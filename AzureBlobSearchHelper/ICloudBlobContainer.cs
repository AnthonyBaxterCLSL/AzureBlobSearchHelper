using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobSearchHelper
{
    //Lame but until this is made virtual (https://github.com/Azure/azure-storage-net/issues/80)

    public interface ICloudBlobContainer
    {
        Task<bool> CreateIfNotExistsAsync();
        ICloudBlockBlob GetBlockBlobReference(string blobName);
        ICloudBlob GetBlobReference(string blobName);
    }


    public class CloudBlobContainerWrapper : ICloudBlobContainer
    {
        private CloudBlobContainer _actualContainer;
        public CloudBlobContainerWrapper(CloudBlobContainer actualContainer)
        {
            _actualContainer = actualContainer;
        }

        public Task<bool> CreateIfNotExistsAsync()
        {
            return _actualContainer.CreateIfNotExistsAsync();
        }

        public ICloudBlockBlob GetBlockBlobReference(string blobName)
        {
            return new CloudBlockBlobWrapper( _actualContainer.GetBlockBlobReference                                    (blobName));
        }

        public ICloudBlob GetBlobReference(string blobName)
        {
            return new CloudBlobWrapper(_actualContainer.GetBlobReference(blobName));
        }

    }

}
