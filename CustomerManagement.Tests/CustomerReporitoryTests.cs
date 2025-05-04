using CustomerManagement.Domain.Entities;
using CustomerManagement.Domain.Interfaces;
using CustomerManagement.Infrastructure.Data;
using CustomerManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManagement.Tests
{
    public class CustomerReporitoryTests
    {
        private readonly AppDbContext _context;
        private readonly CustomerRepository _repository;

        public CustomerReporitoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new CustomerRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectCustomer()
        {
            var customer = new Customer("Bob", "bob@example.com");
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(customer.Id);

            Assert.NotNull(result);
            Assert.IsType<Customer>(result);
            Assert.Equal(customer.Id, result.Id);
            Assert.Equal(customer.Name, result.Name);
            Assert.Equal(customer.Email, result.Email);
        }

        [Fact]
        public async Task EmailExistsAsync_ShouldReturnTrue_WhenEmailAlreadyExists()
        {
            var customer = new Customer("Bob", "bob@example.com");
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var exists = await ((ICustomerRepository)_repository).EmailExistsAsync("bob@example.com");

            Assert.True(exists);
        }

        [Fact]
        public async Task EmailExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
        {
            var exists = await ((ICustomerRepository)_repository).EmailExistsAsync("notfound@example.com");

            Assert.False(exists);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCustomers()
        {
            _context.Customers.AddRange(
                new Customer("Tom", "tom@example.com"),
                new Customer("Jerry", "jerry@example.com")
            );
            await _context.SaveChangesAsync();

            var all = await _repository.GetAllAsync();

            Assert.Equal(2, all.Count());
        }

    }
}
