using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCPTestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotSoSecretController : ControllerBase
    {
        private const string ProjectId = "gpc-dotnetcore-viability-test";

        private static ProjectName ProjectName => ProjectName.FromProject(ProjectId);

        [HttpGet]
        public IActionResult Get() => 
            Ok(SecretManagerServiceClient.Create().ListSecrets(ProjectName));

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var client = SecretManagerServiceClient.Create();

            var secretName = new SecretName(ProjectId, id);

            return Ok(client.GetSecret(secretName));
        }

        [HttpPost]
        public void Post([FromBody] string secretId)
        {
            var client = SecretManagerServiceClient.Create();

            var projectName = new ProjectName(ProjectId);

            var secret = new Secret
            {
                Replication = new Replication
                {
                    Automatic = new Replication.Types.Automatic(),
                },
            };

            client.CreateSecret(projectName, secretId, secret);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] Dictionary<string, string> labels)
        {
            var client = SecretManagerServiceClient.Create();

            var secret = new Secret
            {
                SecretName = new SecretName(ProjectId, id),
            };

            foreach (var label in labels)
            {
                secret.Labels[label.Key] = label.Value;
            }

            var fieldMask = FieldMask.FromString("labels");

            var updatedSecret = client.UpdateSecret(secret, fieldMask);

            return Ok(updatedSecret);
        }

        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            var client = SecretManagerServiceClient.Create();

            var secretName = new SecretName(ProjectId, id);

            client.DeleteSecret(secretName);
        }

        [HttpGet("{id}/version")]
        public IActionResult GetVersion(string id)
        {
            var client = SecretManagerServiceClient.Create();

            var secretName = new SecretName(ProjectId, id);

            return Ok(client.ListSecretVersions(secretName));
        }

        [HttpGet("{id}/version/{versionId}")]
        public IActionResult GetVersion(string id, string versionId)
        {
            var client = SecretManagerServiceClient.Create();

            var secretVersionName = new SecretVersionName(ProjectId, id, versionId);

            return Ok(client.GetSecretVersion(secretVersionName));
        }

        [HttpGet("{id}/version/access")]
        public IActionResult AccessVersion(string id)
        {
            var client = SecretManagerServiceClient.Create();

            var secretName = new SecretName(ProjectId, id);

            var result = client.ListSecretVersions(secretName)
                .Select(v => new 
                {
                    RawVersion = v,
                    Payload = v.State == SecretVersion.Types.State.Enabled
                    ? client.AccessSecretVersion(v.Name).Payload.Data.ToStringUtf8() 
                    : null
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id}/version/access/{versionId}")]
        public IActionResult AccessVersion(string id, string versionId)
        {
            var client = SecretManagerServiceClient.Create();

            var secretVersionName = new SecretVersionName(ProjectId, id, versionId);

            return Ok(client.AccessSecretVersion(secretVersionName).Payload.Data.ToStringUtf8());
        }

        [HttpPost("{id}/version")]
        public IActionResult PostVersion(string id, [FromBody] string data)
        {
            var client = SecretManagerServiceClient.Create();

            var secretName = new SecretName(ProjectId, id);

            var payload = new SecretPayload
            {
                Data = ByteString.CopyFrom(data, Encoding.UTF8),
            };

            return Ok(client.AddSecretVersion(secretName, payload));
        }

        [HttpDelete("{id}/version/{versionId}")]
        public void DeleteVersion(string id, string versionId)
        {
            var client = SecretManagerServiceClient.Create();

            var secretVersionName = new SecretVersionName(ProjectId, id, versionId);

            client.DestroySecretVersion(secretVersionName);
        }
    }
}
