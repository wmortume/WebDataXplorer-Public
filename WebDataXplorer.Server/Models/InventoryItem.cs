#nullable disable
namespace WebDataXplorer.Server.Models;

public partial class InventoryItem
{
    public string InventoryItemId { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public string Url { get; set; }

    public string ItemName { get; set; }

    public string ItemDescription { get; set; }

    public string ItemImageUrl { get; set; }

    public DateTimeOffset? DateAdded { get; set; }

    public string Location { get; set; }

    public int StockQuantity { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; }
}