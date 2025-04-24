using CustomerManagement.Application.DTOs;
using CustomerManagement.Application.Interfaces;
using CustomerManagement.Domain.Entities;
using CustomerManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManagement.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;

        public CustomerService(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(CustomerDto dto)
        {
            if (await _repository.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("A Customer with this email already exists.");
            
            var customer = new Customer(dto.Name, dto.Email);
            await _repository.AddAsync(customer);
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await _repository.GetAllAsync();
            return customers.Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email
            });
        }

        public async Task<CustomerDto> GetByIdAsync(Guid id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null) return null;

            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email
            };
        }
    }
}
