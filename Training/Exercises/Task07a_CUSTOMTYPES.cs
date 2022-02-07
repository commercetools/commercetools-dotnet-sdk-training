using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using commercetools.Api.Models.Common;
using commercetools.Api.Models.Types;
using commercetools.Base.Client;
using Training.Services;

namespace Training
{
    /// <summary>
    /// 1- Create TypeDraft with Custom fields
    /// 2- Create The Type and assign it to customers (as Resources you want to extend)
    public class Task07A : IExercise
    {
        private readonly IClient _client;
        private readonly TypeService _typeService;

        public Task07A(IEnumerable<IClient> clients)
        {
            _client = clients.FirstOrDefault(c => c.Name.Equals("Client"));
            _typeService = new TypeService(_client, Settings.ProjectKey);
        }

        public async Task ExecuteAsync()
        {

            var typeDraft = new TypeDraft
            {
                Key = "allowed-to-place-orders",
                Name = new LocalizedString {{"de", "allowed-to-place-orders"},{"en", "allowed-to-place-orders"}},
                ResourceTypeIds = new List<IResourceTypeId> {IResourceTypeId.Customer},
                FieldDefinitions = new List<IFieldDefinition>
                {
                    this.CreateTypeFieldDefinition()
                }
            };

            var createdType = await _typeService.CreateCustomType("allowed-to-place-orders", 
                new LocalizedString {{"de", "allowed-to-place-orders"},{"en", "allowed-to-place-orders"}},
                new List<IResourceTypeId> {IResourceTypeId.Customer},
                new List<IFieldDefinition>
                {
                    this.CreateTypeFieldDefinition()
                }
            );
            Console.WriteLine($"New custom type has been created with Id: {createdType.Id}");
        }

        private FieldDefinition CreateTypeFieldDefinition()
        {
            var fieldDefinition = new FieldDefinition
            {
                Name = "allowed-to-place-orders",
                Required = false,
                Label = new LocalizedString {{"de", "Allowed to place orders"},{"en", "Allowed to place orders"}},
                Type = new CustomFieldBooleanType()
            };
            return fieldDefinition;
        }
    }
}