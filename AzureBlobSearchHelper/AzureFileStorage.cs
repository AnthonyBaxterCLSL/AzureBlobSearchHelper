
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
            public object OriginalValue { get; set; }

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
            var ret =  typeof(T).GetRuntimeProperties()
                .Where(info => info.GetCustomAttributes(typeof(MetaDataAttribute), false).Any())
                .ToDictionary(info => info.Name, info => new PropMap() { Type = info.PropertyType,  OriginalValue = info.GetValue(item) });

            foreach (var keyValuePair in ret)
            {
                if (keyValuePair.Value.Type == typeof (DateTime) || keyValuePair.Value.Type==typeof(DateTime?))
                    keyValuePair.Value.Value =
                        ((DateTime) keyValuePair.Value.OriginalValue).ToUniversalTime().ToFileTime().ToString();
                else
                    keyValuePair.Value.Value = (keyValuePair.Value.OriginalValue ?? "").ToString();
            }

            return ret;
        }

        private string GetName(T item)
        {
            var v = typeof(T).GetRuntimeProperties()
                .First(info => info.GetCustomAttributes(typeof(MetaNameAttribute), false).Any())
                .GetValue(item);

            return v.ToString();

        }

        private void SetName(T item, string name)
        {
            var propInfo = typeof(T).GetRuntimeProperties()
                .First(info => info.GetCustomAttributes(typeof(MetaNameAttribute), false).Any());

            if (propInfo.PropertyType == typeof (string))
                propInfo.SetValue(item, name);
            else if (propInfo.PropertyType == typeof (Guid))
                propInfo.SetValue(item, Guid.Parse(name));
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
           

            var br = _container.GetBlobReference(name);


            if (!await br.ExistsAsync())
                return default(T);

            var ret = new T();
            await br.FetchAttributesAsync();
            SetName(ret, name);

            foreach (var keyValuePair in br.Metadata)
            {
                var prop = typeof(T).GetRuntimeProperty(keyValuePair.Key.Remove(0, 5));
                if (prop.PropertyType.GetTypeInfo().IsEnum)
                    prop.SetValue(ret, Enum.Parse(prop.PropertyType, keyValuePair.Value));
                else if (prop.PropertyType == typeof(string))
                    prop.SetValue(ret, keyValuePair.Value);
                else if (prop.PropertyType == typeof(int))
                    prop.SetValue(ret, int.Parse(keyValuePair.Value));
                else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                    prop.SetValue(ret, DateTime.FromFileTime(long.Parse(keyValuePair.Value)));
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

