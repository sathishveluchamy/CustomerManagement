using CustomerManagement.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManagement.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto> GetByIdAsync(Guid id);
        Task AddAsync(CustomerDto dto);

    }
}
