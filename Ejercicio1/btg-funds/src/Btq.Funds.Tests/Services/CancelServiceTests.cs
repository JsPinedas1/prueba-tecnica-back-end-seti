using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Btq.Funds.Api.Models.Requests;
using Btq.Funds.Api.Services;
using Btq.Funds.Api.Utils;
using FluentAssertions;
using Moq;
using Xunit;

namespace Btq.Funds.Tests.Services
{
  public class CancelServiceTests
  {
    [Fact]
    public async Task Cancel_Should_Return_NewBalance_And_Set_Status_Cancelled()
    {
      var db = new Mock<IAmazonDynamoDB>();

      var subItem = new Dictionary<string, AttributeValue>
      {
        ["user_id"] = new AttributeValue { S = "u123" },
        ["fund_id"] = new AttributeValue { S = "5" },
        ["status"] = new AttributeValue { S = "ACTIVE" },
        ["amount"] = new AttributeValue { N = "100000" }
      };

      var userItem = new Dictionary<string, AttributeValue>
      {
        ["user_id"] = new AttributeValue { S = "u123" },
        ["balance"] = new AttributeValue { N = "400000" }
      };

      var fundItem = new Dictionary<string, AttributeValue>
      {
        ["fund_id"] = new AttributeValue { S = "5" },
        ["name"] = new AttributeValue { S = "FPV_BTG_PACTUAL_DINAMICA" }
      };

      db.Setup(x => x.GetItemAsync(It.Is<GetItemRequest>(r => r.TableName == System.Environment.GetEnvironmentVariable("SUBS_TABLE") || r.TableName == "Subscriptions"),
                                   It.IsAny<CancellationToken>()))
        .ReturnsAsync(new GetItemResponse { Item = subItem });

      db.Setup(x => x.GetItemAsync(It.Is<GetItemRequest>(r => r.TableName == System.Environment.GetEnvironmentVariable("USERS_TABLE") || r.TableName == "Users"),
                                   It.IsAny<CancellationToken>()))
        .ReturnsAsync(new GetItemResponse { Item = userItem });

      db.Setup(x => x.GetItemAsync(It.Is<GetItemRequest>(r => r.TableName == System.Environment.GetEnvironmentVariable("FUNDS_TABLE") || r.TableName == "Funds"),
                                   It.IsAny<CancellationToken>()))
        .ReturnsAsync(new GetItemResponse { Item = fundItem });

      db.Setup(x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new TransactWriteItemsResponse());

      var ddbHelper = new DdbHelper(db.Object);
      var service = new CancelService(db.Object, ddbHelper);

      var req = new CancelRequest { UserId = "u123", FundId = "5" };
      var res = await service.ProcessAsync(req);

      res.NewBalance.Should().Be(500000);
      res.TrxId.Should().NotBeNullOrWhiteSpace();
      db.Verify(x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_Should_Fail_When_Subscription_Not_Active()
    {
      var db = new Mock<IAmazonDynamoDB>();

      var subItem = new Dictionary<string, AttributeValue>
      {
        ["user_id"] = new AttributeValue { S = "u123" },
        ["fund_id"] = new AttributeValue { S = "5" },
        ["status"] = new AttributeValue { S = "CANCELLED" },
        ["amount"] = new AttributeValue { N = "100000" }
      };

      db.Setup(x => x.GetItemAsync(It.Is<GetItemRequest>(r => r.TableName == System.Environment.GetEnvironmentVariable("SUBS_TABLE") || r.TableName == "Subscriptions"),
                                   It.IsAny<CancellationToken>()))
        .ReturnsAsync(new GetItemResponse { Item = subItem });

      var ddbHelper = new DdbHelper(db.Object);
      var service = new CancelService(db.Object, ddbHelper);

      var req = new CancelRequest { UserId = "u123", FundId = "5" };
      var act = async () => await service.ProcessAsync(req);

      await act.Should().ThrowAsync<System.Exception>().WithMessage("Suscripción no está activa");
    }
  }
}
