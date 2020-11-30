using Google.Api.Gax.ResourceNames;
using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace GCPTestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private const string ProjectId = "gpc-dotnetcore-viability-test";

        [HttpGet]
        public IEnumerable<Topic> Get()
        {
            var publisher = PublisherServiceApiClient.Create();

            var projectName = ProjectName.FromProject(ProjectId);

            var topics = publisher.ListTopics(projectName);

            return topics.ToList();
        }

        [HttpGet("{id}")]
        public Topic Get(string id)
        {
            var publisher = PublisherServiceApiClient.Create();

            var topicName = TopicName.FromProjectTopic(ProjectId, id);

            var topic = publisher.GetTopic(topicName);

            return topic;
        }

        [HttpPost]
        public void Post([FromBody] string id)
        {
            var publisher = PublisherServiceApiClient.Create();

            var topicName = TopicName.FromProjectTopic(ProjectId, id);

            publisher.CreateTopic(topicName);
        }

        [HttpDelete]
        public void Delete([FromQuery] string topicId)
        {
            var publisher = PublisherServiceApiClient.Create();

            var topicName = TopicName.FromProjectTopic(ProjectId, topicId);

            publisher.DeleteTopic(topicName);
        }

        [HttpPost("publishMessage")]
        public void PublishMessage([FromBody] Message message)
        {
            var topicName = TopicName.FromProjectTopic(ProjectId, message.TopicId);
            var publisher = PublisherClient.CreateAsync(topicName).Result;

            publisher.PublishAsync(message.Text);
        }
    }

    public class Message
    {
        public string TopicId { get; set; }

        public string Text { get; set; }
    }
}