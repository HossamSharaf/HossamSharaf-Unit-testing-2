using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class CheckoutTestTask3
    {
        private readonly Mock<IOrderRepository> _orderRepositorySubstitute;
        private readonly Mock<IInventoryService> _inventoryServiceSubstitute;
        private readonly Mock<INotificationService> _notificationServiceSubstitute;
        private readonly CheckoutService _sut;
        public Fixture fixture = new Fixture();
        public CheckoutTestTask3()
        {
            _orderRepositorySubstitute = new Mock<IOrderRepository>();
            _notificationServiceSubstitute = new Mock<INotificationService>();
            _inventoryServiceSubstitute = new Mock<IInventoryService>();
            _sut = new CheckoutService(
         _orderRepositorySubstitute.Object,
         _inventoryServiceSubstitute.Object,
         _notificationServiceSubstitute.Object
                );
        }
        //3.1
        [Fact]
        public async Task CheckoutAsync_ValidOrder_CheckoutSucceeds()
        {
            //arrange
            var item = fixture.Build<OrderItem>()
                .With(x => x.Quantity, 5)
                .With(x=>x.UnitPrice,100m);
            Order order = fixture.Create<Order>();
            _inventoryServiceSubstitute.Setup(x=>x.IsAvailableAsync(It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            //act
           var receipt =await _sut.CheckoutAsync(order);
            //assert
            receipt.Should().NotBeNull();
            _orderRepositorySubstitute.Verify(x => x.SaveAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
                Times.Once);

            _notificationServiceSubstitute.Verify(x => x.SendOrderConfirmationAsync(
                It.IsAny<CheckoutReceipt>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
        //3.2
       
        [Fact]
        public async Task CheckoutAsync_BogusOrder_ReceiptHasSameCustomerData()
        {
            //arrange
            var faker = new Faker<Order>()
        .UseSeed(2026) 
        .RuleFor(o => o.CustomerName, f => f.Person.FullName) 
        .RuleFor(o => o.CustomerEmail, f => f.Internet.Email()) 
                                                               
        .RuleFor(o => o.Items, f => new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = f.Commerce.ProductName(),
                UnitPrice = 150m,
                Quantity = 2
            }
        });
            Order order = faker.Generate();
            _inventoryServiceSubstitute
        .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);
            //act
            var receipt = await _sut.CheckoutAsync(order);
            //assert
            receipt.Should().NotBeNull();

            receipt.CustomerName.Should().Be(order.CustomerName);

            receipt.CustomerEmail.Should().Be(order.CustomerEmail);

        }



    }
}
