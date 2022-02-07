using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using commercetools.Base.Client;
using Training.Services;

namespace Training
{
    /// <summary>
    /// PageQueryRequests Optimized, Query all Products in optimized way
    /// https://docs.commercetools.com/http-api#paging
    /// </summary>
    public class Task06B : IExercise
    {
        private readonly IClient _client;
        private readonly SearchService _searchService;
        
        public Task06B(IEnumerable<IClient> clients)
        {
            _client = clients.FirstOrDefault(c => c.Name.Equals("Client"));
            _searchService = new SearchService(_client, Settings.ProjectKey);
        }

        public async Task ExecuteAsync()
        {
            string lastId = null ;int pageSize = 2;int currentPage = 1; bool lastPage = false;

            while (!lastPage)
            {
                var where = lastId != null ? $"id>\"{lastId}\"" : null;
                
                var response = await _searchService.GetPagedResults(where,pageSize);

                Console.WriteLine($"Show Results of Page {currentPage}");
                foreach (var product in response.Results)
                {
                    Console.WriteLine($"{product.MasterData.Current.Name["en"]}");
                }
                Console.WriteLine("///////////////////////");
                currentPage++;
                lastId = response.Results.Last().Id;
                lastPage = response.Results.Count < pageSize;
            }
        }
    }
}