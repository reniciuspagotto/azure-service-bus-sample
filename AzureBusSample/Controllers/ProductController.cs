using AzureBusSample.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace AzureBusSample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Save(IFormFile file, [FromForm] Product product)
        {
            product.ImageUrl = await Upload(file);
            await SendMessage(product);

            return Ok(true);
        }

        private async Task<string> Upload(IFormFile file)
        {
            var accountName = _configuration["StorageConfiguration:AccountName"];
            var accountKey = _configuration["StorageConfiguration:AccountKey"];
            var containerName = _configuration["StorageConfiguration:ContainerName"];

            var storageCredentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(storageCredentials, true);
            var blobAzure = storageAccount.CreateCloudBlobClient();
            var container = blobAzure.GetContainerReference(containerName);

            var blob = container.GetBlockBlobReference(file.FileName);
            await blob.UploadFromStreamAsync(file.OpenReadStream());

            return blob.SnapshotQualifiedStorageUri.PrimaryUri.ToString();
        }

        private async Task SendMessage(Product product)
        {
            var serviceBusConnectionString = "Endpoint=sb://pagottomanzan.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=u+rnqaa+MyOmLSXocFvKyPTAmQ26Mdrj4HcSdSe6Ff4=";
            var queueName = "product";

            var client = new QueueClient(serviceBusConnectionString, queueName, ReceiveMode.ReceiveAndDelete);

            string messageBody = JsonConvert.SerializeObject(product);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            await client.SendAsync(message);
            await client.CloseAsync();
        }
    }
}
