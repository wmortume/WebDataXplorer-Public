using WebDataXplorer.Server.Interfaces;
using WebDataXplorer.Server.Models;

namespace WebDataXplorer.Server.Services
{
    public class InventoryService(IInventoryRepository repository)
    {
        private readonly IInventoryRepository _repository = repository;

        public async Task<string> CreateAndUploadSnapshotAsync(InventoryQuery query)
        {
            return await _repository.CreateAndUploadInventorySnapshotAsync(query);
        }
    }
}
