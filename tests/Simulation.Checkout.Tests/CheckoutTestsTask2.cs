using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class CheckoutTestsTask2
    {
        private readonly Mock<IOrderRepository> _orderRepositorySubstitute;
        private readonly Mock<IInventoryService> _inventoryServiceSubstitute;
        private readonly Mock<INotificationService> _notificationServiceSubstitute;
        private readonly CheckoutService _sut;
        public CheckoutTestsTask2()
        {
            _orderRepositorySubstitute = new Mock<IOrderRepository>();
            _notificationServiceSubstitute = new Mock<INotificationService>();
            _inventoryServiceSubstitute=new Mock<IInventoryService>();
            _sut = new CheckoutService(
         _orderRepositorySubstitute.Object,
         _inventoryServiceSubstitute.Object,
         _notificationServiceSubstitute.Object
                );
        }
        //2.1
        [Fact]
        public async Task CheckoutAsync_ValidOrder_ShouldSaveAndSendConfirmation()
        {
            //arrange
            Order order = new Order
            {
                CustomerName = "mariam",
                CustomerEmail = "MARY@YAHOO.COM"
                ,
                Items = new List<OrderItem>
                {
                    new OrderItem
                        {
                            ProductId=Guid.NewGuid(),
                            ProductName="keyboard",
                            UnitPrice=350m,
                            Quantity=2
                        }
                        ,
                        new OrderItem
                        {
                            ProductId=Guid.NewGuid(),
                            ProductName="mouse",
                            UnitPrice=450m,
                            Quantity=2
                        }
                }
            };

            _inventoryServiceSubstitute.Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            //act

        var receipt =await _sut.CheckoutAsync(order);

            //ASSERT

             _orderRepositorySubstitute.Verify(x => x.SaveAsync(It.IsAny<Order>()
                 , It.IsAny<CancellationToken>())
                 , Times.Once);

            _notificationServiceSubstitute.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>()
                , It.IsAny<CancellationToken>())
            , Times.Once);
        }
        //2.2
        [Fact]
        public async Task CheckoutAsync_OutOfStock_ThrowException()
        {
            Order order = new Order
            {
                CustomerName = "maram",
                CustomerEmail = "MARY@YAHOO.COM"
               ,
                Items = new List<OrderItem>
                {
                    new OrderItem
                        {
                            ProductId=Guid.NewGuid(),
                            ProductName="keyboard",
                            UnitPrice=350m,
                            Quantity=2
                        }
 
                }
            };
            _inventoryServiceSubstitute.Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
             //act
             var act =async ()=>await _sut.CheckoutAsync(order);

            //assert
            await act.Should().ThrowExactlyAsync<InsufficientInventoryException>()
                .WithMessage("*keyboard*");

            _orderRepositorySubstitute.Verify(x => x.SaveAsync(It.IsAny<Order>()
                 , It.IsAny<CancellationToken>())
                 , Times.Never);

            _notificationServiceSubstitute.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>()
                , It.IsAny<CancellationToken>())
            , Times.Never);
        }
        //2.3
        [Fact]
        public async Task CheckoutAsync_InvalidOrder_ThrowException()
        {
            //arrange
            Order order = new Order
            {
                CustomerName = "maram",
                CustomerEmail = "MARY@YAHOO.COM"
               ,
                Items = new List<OrderItem>
                {
                    new OrderItem
                        {
                            ProductId=Guid.NewGuid(),
                            ProductName="keyboard",
                            UnitPrice=350m,
                            Quantity=0
                        }

                }
            };
            
            //act
            var act = async() => await _sut.CheckoutAsync(order);
            //assert
            await act.Should().ThrowExactlyAsync<InvalidQuantityException>()
                .WithMessage("*keyboard*");
            _orderRepositorySubstitute.Verify(x => x.SaveAsync(It.IsAny<Order>()
                 , It.IsAny<CancellationToken>())
                 , Times.Never);

            _notificationServiceSubstitute.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>()
                , It.IsAny<CancellationToken>())
            , Times.Never);
            _inventoryServiceSubstitute.Verify(x => x.IsAvailableAsync(It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
