using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Catalog.Api.Controllers
{
  [ApiController]
  // [Route("[controller]")] or following
  [Route("items")]
  public class ItemController : ControllerBase
  {
    private readonly IItemsRepository repository;
    private readonly ILogger<ItemController> logger;
    public ItemController(IItemsRepository repository, ILogger<ItemController> logger)
    {
      this.repository = repository;
      this.logger = logger;
    }

    // /items
    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetItemsAsync()
    {
      var items = (await repository.GetItemsAsync())
        .Select(item => item.AsDto());
      logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: Retrieved {items.Count()} items");
      return items;
    }

    // items/:id
    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetItemAsync(Guid id)
    {
      var item = await repository.GetItemAsync(id);
      if (item is null)
      {
        return NotFound();
      }
      return Ok(item.AsDto());
    }

    // /items
    [HttpPost]
    public async Task<ActionResult<ItemDto>> CreateItemAsync(CreateItemDto newItem)
    {
      Item item = new()
      {
        Id = Guid.NewGuid(),
        Name = newItem.Name,
        Price = newItem.Price,
        CreatedDate = DateTimeOffset.UtcNow
      };
      await repository.CreateItemAsync(item);

      return CreatedAtAction(nameof(GetItemAsync), new { id = item.Id }, item.AsDto());
    }

    // /items/:id
    [HttpPut("{id}")]
    public async Task<ActionResult<ItemDto>> UpdateItemAsync(Guid id, UpdateItemDto updateItem)
    {
      var existingItem = await repository.GetItemAsync(id);
      if (existingItem is null)
      {
        return NotFound();
      }
      Item item = existingItem with
      {
        Name = updateItem.Name,
        Price = updateItem.Price
      };
      await repository.UpdateItemAsync(item);
      return Ok(item.AsDto());
    }


    // items/:id
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteItemAsync(Guid id)
    {
      var item = await repository.GetItemAsync(id);
      if (item is null)
      {
        return NotFound();
      }
      await repository.DeleteItemAsync(id);
      return NoContent();
    }
  }
}