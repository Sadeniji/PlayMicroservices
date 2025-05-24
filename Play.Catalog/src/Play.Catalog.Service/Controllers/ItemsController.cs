using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Play.Common;
using MassTransit;
using Play.Catalog.Contracts;

namespace Play.Catalog.Service.Controllers
{
    [Route("items")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> _repository;
        private readonly IPublishEndpoint _publishEndPoint;

        public ItemsController(IRepository<Item> repository, IPublishEndpoint publishEndPoint)
        {
            _repository = repository;
            _publishEndPoint = publishEndPoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {
            var items = (await _repository.GetAllAsync()).Select(item => item.AsDto());

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

            await _publishEndPoint.Publish(new CatalogItemCreated(newItem.Id, newItem.Name, newItem.Description));
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

            await _publishEndPoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));
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


            await _publishEndPoint.Publish(new CatalogItemDeleted(id));
            return NoContent();
        }
    }
}
