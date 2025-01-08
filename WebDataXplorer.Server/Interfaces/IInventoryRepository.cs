using WebDataXplorer.Server.Models;

namespace WebDataXplorer.Server.Interfaces
{
    public interface IInventoryRepository
    {
        Task<string> CreateAndUploadInventorySnapshotAsync(InventoryQuery query);
    }
}
