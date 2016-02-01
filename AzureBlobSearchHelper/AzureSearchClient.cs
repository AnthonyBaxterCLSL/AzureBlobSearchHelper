using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace AzureBlobSearchHelper
{
    public class AzureSearchClient
    {
        private static SearchServiceClient _searchClient;
        private static SearchIndexClient _indexClient;

        public AzureSearchClient(string searchServiceName, string apiKey, string indexName)
        {
            _searchClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
            _indexClient = _searchClient.Indexes.GetClient(indexName);
        }

        public DocumentSearchResponse Search(string searchText)
        {
            SearchParameters sp = new SearchParameters() { SearchMode = SearchMode.All };
            return _indexClient.Documents.Search(searchText, sp);
        }

        public Task<AzureOperationResponse> AsyncRerunIndexer(string indexer)
        {
            return _searchClient.Indexers.RunAsync(indexer);
        }

    }
}
