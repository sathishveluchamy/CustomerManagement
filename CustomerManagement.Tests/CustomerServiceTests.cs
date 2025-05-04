using Castle.Core.Resource;
using CustomerManagement.Application.DTOs;
using CustomerManagement.Application.Services;
using CustomerManagement.Domain.Entities;
using CustomerManagement.Domain.Interfaces;
using Moq;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManagement.Tests
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _mockRepo;
        private readonly CustomerService _service;

        public CustomerServiceTests()
        {
            _mockRepo = new Mock<ICustomerRepository>();
            _service = new CustomerService(_mockRepo.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCustomer_WhenEmailIsUnique()
        {
            var customerDto = new CustomerDto { Name = "Bob", Email = "bob@example.com" };
            _mockRepo.Setup(r => r.EmailExistsAsync(customerDto.Email)).ReturnsAsync(false);
            
            await _service.AddAsync(customerDto);

            _mockRepo.Verify(r => r.AddAsync(It.Is<Customer>(
                c =>  c.Name == customerDto.Name && c.Email == customerDto.Email
                )), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            var customerDto = new CustomerDto { Name = "Bob", Email = "bob@example.com" };
            _mockRepo.Setup(r => r.EmailExistsAsync(customerDto.Email)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(customerDto));
            Assert.Equal("A Customer with this email already exists.", ex.Message);

            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedDtos()
        {
            var customer = new List<Customer>
            {
                new Customer("John", "john@example.com"),
                new Customer("Jane", "jane@example.com")
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(customer);


            var result = await _service.GetAllAsync();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, c => c.Name == "John" && c.Email == "john@example.com");
            Assert.Contains(result, c => c.Name == "Jane" && c.Email == "jane@example.com");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedDto_WhenCustomerExist()
        {
            var customer = new Customer("Bob", "bob@example.com");
            _mockRepo.Setup(r => r.GetByIdAsync(customer.Id)).ReturnsAsync(customer);

            var result = await _service.GetByIdAsync(customer.Id);

            Assert.NotNull(result);
            Assert.IsType<CustomerDto>(result);
            Assert.Equal("Bob", result.Name);
            Assert.Equal("bob@example.com", result.Email);
            Assert.Equal(customer.Id, result.Id);            
        }

        [Fact]
        public async Task GetCustomerById_ShouldreturnNull_WhenCustomerNotFound()
        {
            var customerId = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync((Customer)null);

            var result = await _service.GetByIdAsync(customerId);

            Assert.Null(result);
        }



    }
}
