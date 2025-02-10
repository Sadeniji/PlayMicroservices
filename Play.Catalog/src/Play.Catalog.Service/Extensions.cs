using System;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service
{
    public static class Extensions
    {
        public static ItemDto AsDto(this Item item)
        {
            return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
        }

        public static Item AsItem(this CreateItemDto newItem)
        {
            return new Item
            {
                Id = Guid.NewGuid(),
                Name = newItem.Name,
                Description = newItem.Description,
                CreatedDate = DateTimeOffset.UtcNow,
                Price = newItem.Price
            };
        }
    }
}