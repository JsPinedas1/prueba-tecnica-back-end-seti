using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace Btq.Funds.Api.Services
{
  public class NotificationService
  {
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly string _topicArn;

    public NotificationService()
    {
      _sns = new AmazonSimpleNotificationServiceClient();
      _topicArn = Environment.GetEnvironmentVariable("NOTIFY_TOPIC_ARN") ?? string.Empty;
    }

    public NotificationService(IAmazonSimpleNotificationService sns)
    {
      _sns = sns;
      _topicArn = Environment.GetEnvironmentVariable("NOTIFY_TOPIC_ARN") ?? string.Empty;
    }

    public async Task PublishAsync(string subject, string message)
    {
      if (string.IsNullOrWhiteSpace(_topicArn)) return;

      await _sns.PublishAsync(new PublishRequest
      {
        TopicArn = _topicArn,
        Subject = subject,
        Message = message
      }, CancellationToken.None);
    }
  }
}
