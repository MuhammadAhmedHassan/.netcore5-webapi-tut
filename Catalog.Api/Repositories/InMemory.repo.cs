using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Api.Entities;

namespace Catalog.Api.Repositories
{
  public class InMemoryRepository : IItemsRepository
  {
    private readonly List<Item> items = new() {
      new Item{ Id = Guid.NewGuid(), Name = "Potion", Price = 9, CreatedDate = DateTimeOffset.UtcNow },
      new Item{ Id = Guid.NewGuid(), Name = "Iron Sword", Price = 10, CreatedDate = DateTimeOffset.UtcNow },
      new Item{ Id = Guid.NewGuid(), Name = "L", Price = 30, CreatedDate = DateTimeOffset.UtcNow },
    };

    public async Task CreateItemAsync(Item item)
    {
      items.Add(item);
      await Task.CompletedTask;
    }

    public async Task DeleteItemAsync(Guid id)
    {
      var index = items.FindIndex(item => item.Id == id);
      items.RemoveAt(index);
      await Task.CompletedTask;
    }

    public Task<Item> GetItemAsync(Guid id)
    {
      var item = items.Where(item => item.Id == id).SingleOrDefault();
      return Task.FromResult(item);
    }

    public async Task<IEnumerable<Item>> GetItemsAsync()
    {
      return await Task.FromResult(items);
    }

    public async Task UpdateItemAsync(Item item)
    {
      var index = items.FindIndex(itm => itm.Id == item.Id);
      items[index] = item;
      await Task.CompletedTask;
    }

    Task<Item> IItemsRepository.GetItemAsync(Guid id)
    {
      throw new NotImplementedException();
    }
  }
}