using FluentAssertions;
using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class ArchitectureTests
    {
        [Fact]
        public void Domain_ShouldNotDependOn_ApplicationOrInfrastructure()
        {
            // Arrange
            var domainAssembly = typeof(Order).Assembly;

            // Act
            var result = Types.InAssembly(domainAssembly)
                .ShouldNot()
                .HaveDependencyOn("Simulation.Checkout.Application")
                .And()
                .NotHaveDependencyOn("Simulation.Checkout.Infrastructure")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        // 5.2 Application does not depend on Infrastructure
        [Fact]
        public void Application_ShouldNotDependOn_Infrastructure()
        {
            // Arrange
            var applicationAssembly = typeof(CheckoutService).Assembly;

            // Act
            var result = Types.InAssembly(applicationAssembly)
                .ShouldNot()
                .HaveDependencyOn("Simulation.Checkout.Infrastructure")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        // 5.3 All interfaces start with I
        [Fact]
        public void Interfaces_ShouldStartWith_I()
        {
            // Arrange
            var applicationAssembly = typeof(IOrderRepository).Assembly;

            // Act
            var result = Types.InAssembly(applicationAssembly)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        // 5.4 Classes that implement IOrderRepository end with Repository
        [Fact]
        public void ClassesImplementingIOrderRepository_ShouldEndWith_Repository()
        {
            // Arrange 
            // بنستخدم كلاس من الـ Infrastructure عشان نجيب الـ Assembly بتاعه
            var infrastructureAssembly = typeof(InMemoryOrderRepository).Assembly;

            // Act
            var result = Types.InAssembly(infrastructureAssembly)
                .That()
                .ImplementInterface(typeof(IOrderRepository))
                .Should()
                .HaveNameEndingWith("Repository")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
    }
}
