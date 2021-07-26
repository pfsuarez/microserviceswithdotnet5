using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;

namespace Play.Catalog.Service.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ItemsComtroller : ControllerBase
  {
    private static readonly List<ItemDto> items = new()
    {
      new ItemDto(Guid.NewGuid(), "Potion", "Restores a small amount of HP", 5, DateTimeOffset.UtcNow),
      new ItemDto(Guid.NewGuid(), "Antidote", "Cures Poison", 7, DateTimeOffset.UtcNow),
      new ItemDto(Guid.NewGuid(), "Bronze sword", "Deals a small amount of damage", 20, DateTimeOffset.UtcNow),
    };

    [HttpGet]
    public IEnumerable<ItemDto> Get()
    {
      return items;
    }

    [HttpGet("{id}")]
    public ItemDto GetById(Guid id)
    {
      var item = items.Where(item => item.Id == id).SingleOrDefault();
      return item;
    }

    [HttpPost]
    public ActionResult<ItemDto> Post(CreateItemDto createItemDto)
    {
      var item = new ItemDto(Guid.NewGuid(),
                             createItemDto.Name,
                             createItemDto.Description,
                             createItemDto.Price,
                             DateTimeOffset.UtcNow);

      items.Add(item);

      return CreatedAtAction(nameof(GetById),
                             new { id = item.Id },
                             item);
    }

    [HttpPut("{id}")]
    public IActionResult Update(Guid id, UpdateItemDto updateItemDto)
    {
      var existingItem = items.Where(item => item.Id == id).SingleOrDefault();

      var updatedItem = existingItem with
      {
        Name = updateItemDto.Name,
        Description = updateItemDto.Description,
        Price = updateItemDto.Price
      };

      var index = items.FindIndex(existingItem => existingItem.Id == id);
      items[index] = updatedItem;

      return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
      var index = items.FindIndex(existingItem => existingItem.Id == id);
      items.RemoveAt(index);
      
      return NoContent();
    }
  }
}