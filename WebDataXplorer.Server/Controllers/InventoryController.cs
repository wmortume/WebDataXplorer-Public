using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDataXplorer.Server.Models;
using WebDataXplorer.Server.Services;

namespace WebDataXplorer.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly SqldbWebDataXplorerContext _context;
        private readonly InventoryService _service;
        private readonly IConfiguration _configuration;
        private static List<InventoryItem>? _items;

        public InventoryController(SqldbWebDataXplorerContext context, InventoryService service, IConfiguration configuration)
        {
            _context = context;
            _service = service;
            _configuration = configuration;
            _items ??= configuration.GetSection("SampleData:InventoryItems").Get<List<InventoryItem>>() ?? [];
        }

        [HttpGet("xplorer")]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetInventoryItems()
        {
            bool useSampleData = _configuration.GetValue<bool>("UseSampleData");

            if (useSampleData)
                return _items!.Where(item => item.IsActive).ToList();
            else
                return await _context.InventoryItems.Where(j => j.IsActive).ToListAsync();
        }

        [HttpPatch("{id}/discontinued")]
        public async Task<ActionResult> MarkItemDiscontinued(string id)
        {
            bool useSampleData = _configuration.GetValue<bool>("UseSampleData");

            if (useSampleData)
            {
                var item = _items!.FirstOrDefault(item => item.InventoryItemId == id);
                if (item is null)
                    return NotFound();
                item.IsActive = false;
                return NoContent();
            }
            else
            {
                var inventoryItem = await _context.InventoryItems.FindAsync(id);
                if (inventoryItem is null)
                    return NotFound();
                inventoryItem.IsActive = false;

                try
                {
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest("Concurrency conflict occurred");
                }
            }
        }

        [HttpPost("snapshot")]
        public async Task<ActionResult<string>> TriggerSnapshot([FromBody] InventoryQuery query)
        {
            var result = await _service.CreateAndUploadSnapshotAsync(query);

            if (result == "Missing dataset id, blob container name, or blob storage account name")
            {
                return BadRequest("Missing dataset id, blob container name, or blob storage account name");
            }
            else if (result.StartsWith("Error"))
            {
                // Return a 500 Internal Server Error for other errors
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }
    }
}


