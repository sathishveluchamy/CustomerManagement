using Xunit;
using Moq;
using CustomerManagement.API.Controllers;
using CustomerManagement.Application.DTOs;
using CustomerManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace CustomerManagement.Tests
{
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerService> _mockService;
        private readonly CustomerController _controller;

        public CustomerControllerTests()
        {
            _mockService = new Mock<ICustomerService>();
            _controller = new CustomerController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsListOfCustomers()
        {
            // Arrange
            var customers = new List<CustomerDto>
            {
                new CustomerDto { Name = "Alice", Email = "alice@example.com" },
                new CustomerDto { Name = "Bob", Email = "bob@example.com" }
            };

            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(customers);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCustomers = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(okResult.Value);
            Assert.Equal(2, ((List<CustomerDto>)returnedCustomers).Count);
        }

        [Fact]
        public async Task GetAll_ReturnsNoContent_WhenNoCustomerExist()
        {
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<CustomerDto>());

            var result = await _controller.GetAll();

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Get_ReturnsCustomer_WhenCustomerExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new CustomerDto
            {
                Id = customerId,
                Name = "Jane",
                Email = "jane@example.com"
            };

            _mockService.Setup(s => s.GetByIdAsync(customerId)).ReturnsAsync(customer);

            // Act
            var result = await _controller.Get(customerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<CustomerDto>(okResult.Value);
            Assert.Equal(customerId, returned.Id);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _mockService.Setup(s => s.GetByIdAsync(nonExistentId)).ReturnsAsync((CustomerDto)null);

            // Act
            var result = await _controller.Get(nonExistentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Post_ReturnsCreatedAtAction_WhenCustomerIsCreated()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id = Guid.NewGuid(),
                Name = "Alice",
                Email = "alice@example.com"
            };

            _mockService.Setup(s => s.AddAsync(customerDto)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(customerDto);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.Get), createdAt.ActionName);

            var returned = Assert.IsType<CustomerDto>(createdAt.Value);
            Assert.Equal(customerDto.Id, returned.Id);
            Assert.Equal(customerDto.Name, returned.Name);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WithCorrectErrorMessage_WhenModelStateIsInvalid()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                Id= Guid.NewGuid(),
                Name = null,
                Email = "alice@example.com"
            }; // Missing required "Name"
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.Post(customerDto);

            
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsType<SerializableError>(badRequest.Value);

            Assert.True(errors.ContainsKey("Name"));
            var NameErrors = errors["Name"] as string[];
            Assert.Contains("Name is required", NameErrors);

        }

        [Fact]
        public async Task Post_ReturnsConflict_WhenDuplicateEmail()
        {
            var customerDto = new CustomerDto
            {
                Id= Guid.NewGuid(),
                Name = "Alice",
                Email = "alice@example.com"
            };

            _mockService.Setup(s => s.AddAsync(customerDto)).ThrowsAsync(new InvalidOperationException("Duplicate email"));

            var result = await _controller.Post(customerDto);

            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("Duplicate", conflict.Value.ToString());

        }
    }
}
