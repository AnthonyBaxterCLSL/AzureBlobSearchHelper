
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobSearchHelper
{
    public class AzureFileStorage<T> where T : new()
    {

        private class PropMap
        {
            public Type Type { get; set; }

            public string Value { get; set; }
        }

        private ICloudBlobContainer _container;
        private Func<T, byte[]> _getBytes;

        //string connection, string container

        public AzureFileStorage(ICloudBlobContainer container, Func<T, byte[]> getBytes)
        {
            _container = container;
            _container.CreateIfNotExistsAsync();
            _getBytes = getBytes;

            if (!CheckValid())
                throw new ArgumentException($"Type {typeof(T)} not valid ");
        }

        private bool CheckValid()
        {
            if (typeof (T).GetRuntimeProperties().All(info => info.GetCustomAttribute<MetaNameAttribute>() == null)) 
                return false;

            return true;
        }

        private Dictionary<string, PropMap> GetMetaData(T item)
        {
            return typeof(T).GetRuntimeProperties()
                .Where(info => info.GetCustomAttributes(typeof(MetaDataAttribute), false).Any())
                .ToDictionary(info => info.Name, info => new PropMap() { Type = info.PropertyType, Value = (info.GetValue(item) ?? "").ToString() });

        }

        private string GetName(T item)
        {
            var v = typeof(T).GetRuntimeProperties()
                .First(info => info.GetCustomAttributes(typeof(MetaNameAttribute), false).Any())
                .GetValue(item);

            return v.ToString();

        }

        public async Task<bool> TrySaveItemAsync(T item)
        {
            ICloudBlockBlob blockBlob = _container.GetBlockBlobReference(GetName(item));
            await blockBlob.UploadFromByteArrayAsync(_getBytes(item), 0, _getBytes(item).Length);
            foreach (var metaItem in GetMetaData(item).Where(pair => !string.IsNullOrWhiteSpace(pair.Value.Value)))
            {
                blockBlob.Metadata[$"meta_{metaItem.Key}"] = metaItem.Value.Value;
            }
            await blockBlob.SetMetadataAsync();

            return true;
        }

        public async Task<T> GetMetaItemAsync(string name)
        {
            var ret = new T();
            var br = _container.GetBlobReference(name);
            await br.FetchAttributesAsync();

            foreach (var keyValuePair in br.Metadata)
            {
                var prop = typeof(T).GetRuntimeProperty(keyValuePair.Key.Remove(0, 5));
                if (prop.PropertyType.GetTypeInfo().IsEnum)
                    prop.SetValue(ret, Enum.Parse(prop.PropertyType, keyValuePair.Value));
                else if (prop.PropertyType == typeof(string))
                    prop.SetValue(ret, keyValuePair.Value);
                else if (prop.PropertyType == typeof(int))
                    prop.SetValue(ret, int.Parse(keyValuePair.Value));
                else if (prop.PropertyType == typeof(DateTime))
                    prop.SetValue(ret, DateTime.Parse(keyValuePair.Value));
                else if (prop.PropertyType == typeof(bool))
                    prop.SetValue(ret, bool.Parse(keyValuePair.Value));
            }
            return ret;
        }

        public async Task<byte[]> GetFileAsync(string name)
        {

            var br = _container.GetBlobReference(name);
            ;
            byte[] arr = new byte[br.StreamMinimumReadSizeInBytes];
            var length = await br.DownloadToByteArrayAsync(arr, 0);
            return arr.Where((b, i) => i < length).ToArray();
        }

        public async Task DeleteFileAsync(string name)
        {
            await _container.GetBlobReference(name).DeleteIfExistsAsync();
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _container.GetBlobReference(name).ExistsAsync();
        }
    }

}

