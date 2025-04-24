using CustomerManagement.Application.DTOs;
using CustomerManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagement.API.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing customer data.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerController"/> class.
        /// </summary>
        /// <param name="service">The customer service instance.</param>
        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets all customers.
        /// </summary>
        /// <remarks>
        /// Returns 200 with a list of customers if any exist, or 204 if there are none.
        /// </remarks>
        /// <returns>A list of customers, or no content if none exist.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CustomerDto>), 200)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _service.GetAllAsync();
            if (customers == null || !customers.Any())
                return NoContent();

            return Ok(customers);
        }

        /// <summary>
        /// Gets a customer by ID.
        /// </summary>
        /// <param name="id">The unique identifier of the customer to retrieve.</param>
        /// <returns>
        /// 200 with the customer if found; 404 if not found.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(Guid id)
        {
            var customer = await _service.GetByIdAsync(id);
            if (customer == null) return NotFound();

            return Ok(customer);
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="dto">The customer data transfer object.</param>
        /// <returns>
        /// 201 with the created customer if successful, 400 if the model is invalid, or 409 if the email already exists.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(CustomerDto), 201)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Post([FromBody] CustomerDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _service.AddAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
