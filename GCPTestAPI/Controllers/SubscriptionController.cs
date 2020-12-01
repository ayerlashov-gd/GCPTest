using Google.Api.Gax.ResourceNames;
using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCPTestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private const string ProjectId = "gpc-dotnetcore-viability-test";

        [HttpPost]
        public void Post([FromBody] CreateTopicRequest request)
        {
            var subscriber = SubscriberServiceApiClient.Create();
            var topicName = TopicName.FromProjectTopic(ProjectId, request.TopicId);

            var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, request.SubscriptionId);
            subscriber.CreateSubscription(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 60);
        }

        [HttpGet]
        public IActionResult Get()
        {
            var subscriber = SubscriberServiceApiClient.Create();
            var projectName = ProjectName.FromProject(ProjectId);
            var subscriptions = subscriber.ListSubscriptions(projectName);

            return Ok(subscriptions);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var subscriber = SubscriberServiceApiClient.Create();
            var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, id);
            var subscription = subscriber.GetSubscription(subscriptionName);

            return Ok(subscription);
        }

        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            var subscriber = SubscriberServiceApiClient.Create();
            var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, id);
            subscriber.DeleteSubscription(subscriptionName);
        }

        [HttpGet("getMessages")]
        public IEnumerable<DecodedMessage> GetMessages(string subscriptionId)
        {
            var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, subscriptionId);
            var subscriberClient = SubscriberServiceApiClient.Create();

            int messageCount = 0;

            PullResponse response = subscriberClient.Pull(subscriptionName, returnImmediately: false, maxMessages: 20);

            messageCount = response.ReceivedMessages.Count;

            if (messageCount > 0)
            {
                subscriberClient.Acknowledge(subscriptionName, response.ReceivedMessages.Select(msg => msg.AckId));
            }

            return response.ReceivedMessages.Select(Map);
        }

        private DecodedMessage Map(ReceivedMessage rawMessage)
        {
            return new DecodedMessage
            {
                Message = Encoding.UTF8.GetString(rawMessage.Message.Data.ToArray()),
                RawMessage = rawMessage
            };
        }
    }

    public class DecodedMessage
    {
        public ReceivedMessage RawMessage { get; set; }
        
        public string Message { get; set; }
    }

    public class CreateTopicRequest
    {
        public string TopicId { get; set; }

        public string SubscriptionId { get; set; }
    }
}
