using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebDataXplorer.Server.Models;

namespace WebDataXplorer.Server.Repositories
{
    public class InventoryRepository : Interfaces.IInventoryRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _datasetId;
        private readonly string _blobContainerName;
        private readonly string _blobStorageAccountName;
        private string _bearerToken = string.Empty;
        private string _blobAccessKey = string.Empty;

        public InventoryRepository(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _datasetId = configuration["BrightData:DatasetId"] ?? string.Empty;
            _blobContainerName = configuration["Azure:ContainerName"] ?? string.Empty;
            _blobStorageAccountName = configuration["Azure:StorageAccountName"] ?? string.Empty;
            bool useSampleData = configuration.GetValue<bool>("UseSampleData");
            if (!useSampleData)
                LoadKeyVaultSecrets(configuration);
        }

        private void LoadKeyVaultSecrets(IConfiguration configuration)
        {
            var keyVaultUrl = configuration["Azure:KeyVaultUrl"];

            if (!Uri.TryCreate(keyVaultUrl, UriKind.Absolute, out var keyVaultUri))
            {
                Console.Error.WriteLine("Invalid or missing Key Vault URL.");
                return;
            }

            try
            {
                // Allows configuration manager to access values from azure key vault by variables
                var client = new SecretClient(keyVaultUri, new DefaultAzureCredential()); // Uses Local VS or Cloud Hosted credentials
                _bearerToken = client.GetSecret("BrightDataBearerToken")?.Value?.Value ?? string.Empty;
                _blobAccessKey = client.GetSecret("StorageAccessKey")?.Value?.Value ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error accessing Key Vault: {ex.Message}");
                _bearerToken = string.Empty;
                _blobAccessKey = string.Empty;
            }
        }

        /// <summary>
        /// Creates and uploads an inventory snapshot to Azure Blob Storage
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<string> CreateAndUploadInventorySnapshotAsync(InventoryQuery query)
        {
            if (!string.IsNullOrEmpty(_datasetId) && !string.IsNullOrEmpty(_blobContainerName) && !string.IsNullOrEmpty(_blobStorageAccountName))
            {
                var payload = new // Using anonymous type instead of nested classes for simplicity and single-use
                {
                    deliver = new
                    {
                        type = "azure",
                        filename = new
                        {
                            template = "{[snapshot_id]}_{[snapshot_datetime]}",
                            extension = "json"
                        },
                        credentials = new
                        {
                            account = _blobStorageAccountName,
                            key = _blobAccessKey
                        },
                        container = _blobContainerName
                    },
                    input = new[] { query }
                };

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
                var requestContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"trigger?dataset_id={_datasetId}&type=discover_new&discover_by=keyword", requestContent);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return "Missing dataset id, blob container name, or blob storage account name";
            }
        }
    }
}
