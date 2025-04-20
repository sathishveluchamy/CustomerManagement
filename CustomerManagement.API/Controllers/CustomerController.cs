using CustomerManagement.Application.DTOs;
using CustomerManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;
        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var customer = await _service.GetByIdAsync(id);
            if (customer == null) return NotFound();

            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDto dto)
        {
            await _service.AddAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }
    }
}
