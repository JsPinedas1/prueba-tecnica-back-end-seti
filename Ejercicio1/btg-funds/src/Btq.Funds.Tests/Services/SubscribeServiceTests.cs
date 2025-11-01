using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.SimpleNotificationService;
using Btq.Funds.Api.Models.Requests;
using Btq.Funds.Api.Services;
using Btq.Funds.Api.Utils;
using FluentAssertions;
using Moq;
using Xunit;

namespace Btq.Funds.Tests.Services
{
  public class SubscribeServiceTests
  {
    [Fact]
    public async Task Subscribe_Should_Return_NewBalance_When_Valid()
    {
      var db = new Mock<IAmazonDynamoDB>();
      var sns = new Mock<IAmazonSimpleNotificationService>();

      var userItem = new Dictionary<string, AttributeValue>
      {
        ["user_id"] = new AttributeValue { S = "u123" },
        ["balance"] = new AttributeValue { N = "500000" },
        ["name"] = new AttributeValue { S = "Demo" },
        ["email"] = new AttributeValue { S = "demo@x.com" },
        ["notify_pref"] = new AttributeValue { S = "email" }
      };

      var fundItem = new Dictionary<string, AttributeValue>
      {
        ["fund_id"] = new AttributeValue { S = "5" },
        ["name"] = new AttributeValue { S = "FPV_BTG_PACTUAL_DINAMICA" },
        ["min_amount"] = new AttributeValue { N = "100000" },
        ["category"] = new AttributeValue { S = "FPV" }
      };

      db.Setup(x => x.GetItemAsync(It.Is<GetItemRequest>(r => r.TableName == System.Environment.GetEnvironmentVariable("USERS_TABLE") || r.TableName == "Users"),
                                   It.IsAny<CancellationToken>()))
        .ReturnsAsync(new GetItemResponse { Item = userItem });

      db.Setup(x => x.GetItemAsync(It.Is<GetItemRequest>(r => r.TableName == System.Environment.GetEnvironmentVariable("FUNDS_TABLE") || r.TableName == "Funds"),
                                   It.IsAny<CancellationToken>()))
        .ReturnsAsync(new GetItemResponse { Item = fundItem });

      db.Setup(x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new TransactWriteItemsResponse());

      var ddbHelper = new DdbHelper(db.Object);
      var notifier = new NotificationService(sns.Object);
      var service = new SubscribeService(db.Object, notifier, ddbHelper);

      var req = new SubscribeRequest { UserId = "u123", FundId = "5", Amount = 100000 };

      var res = await service.ProcessAsync(req);

      res.NewBalance.Should().Be(400000);
      res.TrxId.Should().NotBeNullOrWhiteSpace();
      db.Verify(x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Subscribe_Should_Throw_When_Insufficient_Balance()
    {
      var db = new Mock<IAmazonDynamoDB>();
      var sns = new Mock<IAmazonSimpleNotificationService>();

      var userItem = new Dictionary<string, AttributeValue>
      {
        ["user_id"] = new AttributeValue { S = "u123" },
        ["balance"] = new AttributeValue { N = "50000" },
        ["name"] = new AttributeValue { S = "Demo" }
      };

      var fundItem = new Dictionary<string, AttributeValue>
      {
        ["fund_id"] = new AttributeValue { S = "5" },
        ["name"] = new AttributeValue { S = "FPV_BTG_PACTUAL_DINAMICA" },
        ["min_amount"] = new AttributeValue { N = "100000" }
      };

      db.Setup(x => x.GetItemAsync(It.Is<GetItemRequest>(r => r.TableName == System.Environment.GetEnvironmentVariable("USERS_TABLE") || r.TableName == "Users"),
                                   It.IsAny<CancellationToken>()))
        .ReturnsAsync(new GetItemResponse { Item = userItem });

      db.Setup(x => x.GetItemAsync(It.Is<GetItemRequest>(r => r.TableName == System.Environment.GetEnvironmentVariable("FUNDS_TABLE") || r.TableName == "Funds"),
                                   It.IsAny<CancellationToken>()))
        .ReturnsAsync(new GetItemResponse { Item = fundItem });

      var ddbHelper = new DdbHelper(db.Object);
      var notifier = new NotificationService(sns.Object);
      var service = new SubscribeService(db.Object, notifier, ddbHelper);

      var req = new SubscribeRequest { UserId = "u123", FundId = "5", Amount = 100000 };

      var act = async () => await service.ProcessAsync(req);

      await act.Should().ThrowAsync<System.Exception>()
        .WithMessage("No tiene saldo disponible para vincularse al fondo FPV_BTG_PACTUAL_DINAMICA");
    }
  }
}
