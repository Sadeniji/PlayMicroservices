using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Play.Common;

namespace Play.Catalog.Service.Controllers
{
    [Route("items")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> _repository;
        private static int requestCounter = 0;

        public ItemsController(IRepository<Item> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {
            requestCounter++;
            Console.WriteLine($"Request {requestCounter}: Starting...");

            if (requestCounter <= 2)
            {
                Console.WriteLine($"Request {requestCounter}: Delaying...");
                await Task.Delay(TimeSpan.FromSeconds(10));
            }

            if (requestCounter <= 4)
            {
                Console.WriteLine($"Request {requestCounter}: 500 (Internal Server Error)...");
                return StatusCode(500);
            }

            var items = (await _repository.GetAllAsync()).Select(item => item.AsDto());

            Console.WriteLine($"Request {requestCounter}: 200 (Ok).");
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = await _repository.GetAsync(id);

            return item == null ? NotFound() : item.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> Post(CreateItemDto createItemDto)
        {
            var newItem = createItemDto.AsItem();
            await _repository.CreateAsync(newItem);

            return CreatedAtAction(nameof(GetByIdAsync), new { Id = newItem.Id }, newItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await _repository.GetAsync(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;

            await _repository.UpdateAsync(existingItem);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var itemToDelete = await _repository.GetAsync(id);

            if (itemToDelete == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(id);

            return NoContent();
        }
    }
}
