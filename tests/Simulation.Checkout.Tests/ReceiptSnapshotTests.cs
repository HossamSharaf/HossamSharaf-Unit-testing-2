using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class CheckoutTestsTask4
    {
        private readonly Mock<IOrderRepository> _orderRepositorySubstitute;
        private readonly Mock<IInventoryService> _inventoryServiceSubstitute;
        private readonly Mock<INotificationService> _notificationServiceSubstitute;
        private readonly CheckoutService _sut;
       public CheckoutTestsTask4()
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
        [Fact]
        public async Task CheckoutAsync_ExampleC_SnapshotTest()
        {
            // Arrange
            // 1. لازم نستخدم Guid ثابت مش NewGuid عشان الـ Snapshot مايضربش كل مرة
            var fixedProductId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            Order order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed@Gmail.com",
                Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = fixedProductId, // استخدمنا الـ Guid الثابت
                ProductName = "Product 3",
                UnitPrice = 500m,
                Quantity = 3 // ده Example C (3 * 500)
            }
        }
            };

            _inventoryServiceSubstitute
                .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var receipt = await _sut.CheckoutAsync(order);

            // Assert
            // سطر واحد بس بيقوم بكل الشغل!
            await Verify(receipt);
        }
    }
}
