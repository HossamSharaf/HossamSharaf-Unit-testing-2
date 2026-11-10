using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class CheckoutServiceTestsTask1
    {
        private readonly IOrderRepository _orderRepositorySubstitute;
        private readonly IInventoryService _inventoryServiceSubstitute;
        private readonly INotificationService _notificationServiceSubstitute;

        private readonly CheckoutService _sut;
        public static TheoryData<decimal, decimal> _amounts =>
           new()
           {
               {999.99m,0m},
               {1000m,100m},
               {1500m,100m}
           };
       
        public CheckoutServiceTestsTask1()
        {
            _inventoryServiceSubstitute = Substitute.For<IInventoryService>();
             _notificationServiceSubstitute = Substitute.For<INotificationService>();
             _orderRepositorySubstitute = Substitute.For<IOrderRepository>();

            _sut=new CheckoutService (
                _orderRepositorySubstitute,
                _inventoryServiceSubstitute, 
                _notificationServiceSubstitute);
        }
        //1.1
        [Fact]
        public async Task CheckoutAsync_OrderIsEmpty_ThrowEmptyOrderException()
        {
            //arrange
            Order order =new Order 
            {
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem>()
            };
            //act

            Func<Task> act = async () => await _sut.CheckoutAsync(order);

            //assert

           await act.Should().ThrowExactlyAsync<EmptyOrderException>();

           await _orderRepositorySubstitute.
                DidNotReceive().
                SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

            await _notificationServiceSubstitute
                .DidNotReceive()
                .SendOrderConfirmationAsync(Arg.Any<CheckoutReceipt>(), Arg.Any<CancellationToken>());

        }
        //1.2
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CheckoutAsync_ZeroAndNegativeQuantity_ThrowInvalidQuantityException(int  quantity)
        {
            //arrange
            Order order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed@Gmail.com",
                Items = new List<OrderItem>
                {
                    new OrderItem 
                    {
                    ProductId = Guid.NewGuid(),
                    ProductName = "MacBook Pro",
                    UnitPrice = 50000m,
                    Quantity = quantity 
                    }
                }
            }; 
           //act

            var act = async () => await _sut.CheckoutAsync(order);
            //assert

            await act.Should().ThrowExactlyAsync<InvalidQuantityException>()
                .WithMessage("*MacBook Pro*");

            await _orderRepositorySubstitute
                .DidNotReceive()
                .SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

            await _notificationServiceSubstitute
                .DidNotReceive()
                .SendOrderConfirmationAsync(Arg.Any<CheckoutReceipt>(), Arg.Any<CancellationToken>());
        }
        //1.3
        [Fact]
        public async Task CheckoutAsync_NegativeUnitPrice_ThrowInvalidPriceException()
        {
            //arrange
            Order order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed@Gmail.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                    ProductId = Guid.NewGuid(),
                    ProductName = "MacBook Pro",
                    UnitPrice = -0.01m,
                    Quantity = 5
                    }
                }
            };
            //act

            var act = async () => await _sut.CheckoutAsync(order);
            //assert

            await act.Should().ThrowExactlyAsync<InvalidPriceException>()
                .WithMessage("*MacBook Pro*");
                

            await _orderRepositorySubstitute
                            .DidNotReceive()
                            .SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

            await _notificationServiceSubstitute
                .DidNotReceive()
                .SendOrderConfirmationAsync(Arg.Any<CheckoutReceipt>(), Arg.Any<CancellationToken>());

        }
        //1.4
        [Fact]
        public async Task CheckoutAsync_ZeroUnitPrice_CheckoutSucceeds()
        {
            //arrange
            Order order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed@Gmail.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                    ProductId = Guid.NewGuid(),
                    ProductName = "MacBook Pro",
                    UnitPrice = 0m,
                    Quantity = 3
                    }
                }
            };
            _inventoryServiceSubstitute
                .IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(true);
             //act

            var receipt=  await _sut.CheckoutAsync(order);
            //assert

            receipt.Should().NotBeNull();
            receipt.Subtotal.Should().Be(0m);
            receipt.Total.Should().Be(0m);

            await _orderRepositorySubstitute
                            .Received(1)
                            .SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

            await _notificationServiceSubstitute
                .Received(1)
                .SendOrderConfirmationAsync(Arg.Any<CheckoutReceipt>(), Arg.Any<CancellationToken>());
        }
        //1.5
        [Theory]
        [MemberData(nameof(_amounts))]
        public async Task CheckoutAsync_DiscountTest_MaximumDiscountIs100(decimal price ,decimal expectedDiscount)
        {
            //arrange
            Order order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed@Gmail.com",
                Items = new List<OrderItem>
                 {
                     new OrderItem
                     {
                     ProductId = Guid.NewGuid(),
                     ProductName = "MacBook Pro",
                     UnitPrice = price,
                     Quantity = 1
                     }
                 }
            };

            _inventoryServiceSubstitute
               .IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
               .Returns(true);
            //act

            var receipt=await _sut.CheckoutAsync(order);
            //assert

            receipt.Should().NotBeNull();
            receipt.Subtotal.Should().Be(price);
            receipt.Discount.Should().Be(expectedDiscount);
            await _orderRepositorySubstitute
                           .Received(1)
                           .SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

            await _notificationServiceSubstitute
                .Received(1)
                .SendOrderConfirmationAsync(Arg.Any<CheckoutReceipt>(), Arg.Any<CancellationToken>());
        

        }
        //1.6
        [Fact]
        public async Task CheckoutAsync_ValidTest1_CorrectSubtotalDiscountAndTotal()
        {
            //arrange
            Order order =
                new Order
                {
                    CustomerName = "HANA",
                    CustomerEmail = "HANA@gmail.com",
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId=Guid.NewGuid(),
                            ProductName="keyboard",
                            UnitPrice=150m,
                            Quantity=2
                        }
                        ,
                        new OrderItem
                        {
                            ProductId=Guid.NewGuid(),
                            ProductName="mouse",
                            UnitPrice=400m,
                            Quantity=1
                        }
                    }
                };
            _inventoryServiceSubstitute
               .IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
               .Returns(true);
            //act

            var receipt =await _sut.CheckoutAsync(order);
            //assert

            receipt.Should().NotBeNull();
            receipt.Subtotal.Should().Be(700m);
            receipt.Total.Should().Be(700m);
            receipt.Discount.Should().Be(0m);

            await _orderRepositorySubstitute
                          .Received(1)
                          .SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

            await _notificationServiceSubstitute
                .Received(1)
                .SendOrderConfirmationAsync(Arg.Any<CheckoutReceipt>(), Arg.Any<CancellationToken>());
        }
        [Fact]
        public async Task CheckoutAsync_ValidTest2_CorrectSubtotalDiscountAndTotal()
        {
            //arrange
            Order order =
               new Order
               {
                   CustomerName = "HANAN",
                   CustomerEmail = "HANAN@gmail.com",
                   Items = new List<OrderItem>
                    {
                         new OrderItem
                        {
                            ProductId=Guid.NewGuid(),
                            ProductName="Air Buds",
                            UnitPrice=500m,
                            Quantity=3
                        }
                    }
               };

            _inventoryServiceSubstitute
               .IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
               .Returns(true);
            //act

            var receipt = await _sut.CheckoutAsync(order);
            //assert

            receipt.Should().NotBeNull();
            receipt.Subtotal.Should().Be(1500m);
            receipt.Total.Should().Be(1400m);
            receipt.Discount.Should().Be(100m);

            await _orderRepositorySubstitute
                          .Received(1)
                          .SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

            await _notificationServiceSubstitute
                .Received(1)
                .SendOrderConfirmationAsync(Arg.Any<CheckoutReceipt>(), Arg.Any<CancellationToken>());
        }
      
    }
    }

