using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GCPTestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private const string ProjectId = "gpc-dotnetcore-viability-test";

        static StorageController()
        {
            Client = StorageClient.Create();

            Client.Service.HttpClient.Timeout = TimeSpan.FromMinutes(1);
        }

        private static StorageClient Client { get; } 

        [HttpGet("bucket")]
        public async Task<IActionResult> Get() =>
            Ok(await Client.ListBucketsAsync(ProjectId).ToListAsync());

        [HttpGet("bucket/{id}")]
        public async Task<IActionResult> Get(string id) =>
            Ok(await Client.GetBucketAsync(id));

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Post([FromBody] string bucketId) =>
            Ok(await Client.CreateBucketAsync(ProjectId, bucketId));

        [HttpDelete("bucket/{id}")]
        public async Task<IActionResult> DeleteBucket(string id)
        {
            await Client.DeleteBucketAsync(id);
            
            return Ok();
        }

        [HttpGet("bucket/{id}/object")]
        public async Task<IActionResult> GetObjects(string id) =>
            Ok(await Client.ListObjectsAsync(id).ToListAsync());

        [HttpGet("bucket/{id}/object/{objectName}")]
        public IActionResult Download(string id, string objectName) =>
            new FileCallbackResult(
                new MediaTypeHeaderValue("application/octet-stream"),
                async (stream, context) => await Client.DownloadObjectAsync(id, objectName, stream))
            {
                FileDownloadName = objectName
            };

        [HttpGet("bucket/{id}/object/{objectName}/metaData")]
        public async Task<IActionResult> Get(string id, string objectName) =>
            Ok(await Client.GetObjectAsync(id, objectName));

        [HttpPost("bucket/{id}/object")]
        public async Task<IActionResult> Upload(string id, IFormFile file)
        {
            var uploader = Client.CreateObjectUploader(id, file.FileName, Request.ContentType, file.OpenReadStream());

            var upload = uploader.UploadAsync();

            while (upload.Status != TaskStatus.RanToCompletion)
            {
                try
                {
                    await upload;
                }
                catch (TaskCanceledException)
                {
                    upload = uploader.ResumeAsync();
                }
            }            

            return Ok(uploader.ResponseBody);
        }

        [HttpDelete("bucket/{id}/object/{objectName}")]
        public async Task<IActionResult> Delete(string id, string objectName)
        {
            await Client.DeleteObjectAsync(id, objectName);

            return Ok();
        }
    }
}
