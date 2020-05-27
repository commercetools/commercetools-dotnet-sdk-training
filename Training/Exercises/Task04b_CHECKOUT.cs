using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using commercetools.Sdk.Client;
using commercetools.Sdk.Domain;
using commercetools.Sdk.Domain.Carts;
using commercetools.Sdk.Domain.Carts.UpdateActions;
using commercetools.Sdk.Domain.Channels;
using commercetools.Sdk.Domain.Common;
using commercetools.Sdk.Domain.Customers;
using commercetools.Sdk.Domain.Orders;
using commercetools.Sdk.Domain.Orders.UpdateActions;
using commercetools.Sdk.Domain.States;
using commercetools.Sdk.HttpApi.CommandBuilders;
using Training.Extensions;

namespace Training
{
    /// <summary>
    /// Create a cart for a customer, add a product to it, create an order from the cart and change the order state.
    /// </summary>
    public class Task04B : IExercise
    {
        private readonly IClient _client;

        public Task04B(IClient commercetoolsClient)
        {
            this._client =
                commercetoolsClient ?? throw new ArgumentNullException(nameof(commercetoolsClient));
        }

        public async Task ExecuteAsync()
        {
            //Fetch a channel if your inventory mode will not be NONE
            //Get Channel By Key (not supported yet)
            var channelResult = await _client.Builder()
                .Channels()
                .Query()
                .Where(ch => ch.Key == "berlin-warehouse")
                .ExecuteAsync();

            //check the result
            var channel = channelResult.Results.FirstOrDefault();
                                        

            //Fetch a state if you have a defined custom workflow
            var orderPacked = await _client.Builder().States().GetByKey("OrderPacked").ExecuteAsync();

            // TODO: Perform cart operations:
            //      Get Customer, create cart, add products, add inventory mode
            //      add discount codes, perform a recalculation
            // TODO: Convert cart into an order, set order status, set state in custom work flow


            var customer = await _client.Builder()
                .Customers().GetByKey("customer-michael1").ExecuteAsync();

            //create a cart for a customer
            var cart = await CreateCart(customer);
            
            Console.WriteLine($"Cart {cart.Id} for customer: {cart.CustomerId}");

            cart = await
                AddProductToCartBySkusAndChannel(cart, channel, "9812", "9812", "9812");
            
            cart = await AddDiscountToCart(cart, "SUMMER");
            cart = await Recalculate(cart);
            cart = await SetShipping(cart);
            
            //TODO: need to create payment and add to cart

            var order = await CreateOrder(cart);
            order = await ChangeOrderState(order, OrderState.Complete);
            order = await ChangeWorkflowState(order,orderPacked.ToReference());
        }


        #region HelperFunctions
        
        private async Task<Cart> CreateCart(Customer customer)
        {
            var defaultShippingAddress = customer.GetDefaultShippingAddress();
            var cartDraft = new CartDraft
            {
                CustomerId = customer.Id,
                CustomerEmail = customer.Email,
                Currency = "EUR",
                Country = defaultShippingAddress.Country,
                ShippingAddress = defaultShippingAddress,
                DeleteDaysAfterLastModification = 90,
                InventoryMode = InventoryMode.ReserveOnOrder
            };
            return await _client.ExecuteAsync(new CreateCommand<Cart>(cartDraft));
        }

        private async Task<Cart> AddProductToCartBySkusAndChannel(Cart cart, Channel channel, params string[] skus)
        {
            var lineItemsToAddActions = new List<UpdateAction<Cart>>();
            foreach (var sku in skus)
            {
                lineItemsToAddActions.AddUpdate(new AddLineItemUpdateAction
                {
                    Sku = sku,
                    Quantity = 1,
                    SupplyChannel = new Reference<Channel> {Id = channel.Id}
                });
            }

            return await _client.ExecuteAsync(
                cart.UpdateById(actions => lineItemsToAddActions));
        }

        private async Task<Cart> AddDiscountToCart(Cart cart, string code)
        {
            var action = new AddDiscountCodeUpdateAction
            {
                Code = code
            };
            return await _client.ExecuteAsync(
                cart.UpdateById(actions =>
                    actions.AddUpdate(action)));
        }

        private async Task<Cart> Recalculate(Cart cart)
        {
            var action = new RecalculateUpdateAction();

            return await _client.ExecuteAsync(
                cart.UpdateById(actions =>
                    actions.AddUpdate(action)));
        }

        private async Task<Cart> SetShipping(Cart cart)
        {
            var shippingMethodsResult = await _client
                .ExecuteAsync(new GetShippingMethodsForCartCommand(cart.Id));

            var shippingMethod = shippingMethodsResult.Results.FirstOrDefault();
            var action = new SetShippingMethodUpdateAction
            {
                ShippingMethod = shippingMethod?.ToReference()
            };

            return await _client.ExecuteAsync(
                cart.UpdateById(actions =>
                    actions.AddUpdate(action)));
        }

        private async Task<Order> CreateOrder(Cart cart)
        {
            var orderFromCartDraft = new OrderFromCartDraft
            {
                Id = cart.Id,
                Version = cart.Version,
            };
            return await _client
                .Builder()
                .Orders()
                .Create(orderFromCartDraft).ExecuteAsync();
        }
        
        
        private async Task<Order> ChangeOrderState(Order order, OrderState state)
        {
            var action = new ChangeOrderStateUpdateAction
            {
                OrderState = state
            };
            return await _client.ExecuteAsync(
                order.UpdateById(actions =>
                    actions.AddUpdate(action)));
        }
        
        private async Task<Order> ChangeWorkflowState(Order order, IReference<State> state)
        {
            var action = new TransitionStateUpdateAction
            {
                State = state
            };
            return await _client.ExecuteAsync(
                order.UpdateById(actions =>
                    actions.AddUpdate(action)));
        }
        
        #endregion
    }
}