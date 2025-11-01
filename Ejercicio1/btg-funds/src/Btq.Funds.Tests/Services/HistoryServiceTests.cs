using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Btq.Funds.Api.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Btq.Funds.Tests.Services
{
  public class HistoryServiceTests
  {
    [Fact]
    public async Task History_Should_Return_Transactions_For_User()
    {
      var db = new Mock<IAmazonDynamoDB>();

      var items = new List<Dictionary<string, AttributeValue>> {
        new Dictionary<string, AttributeValue> {
          ["user_id"] = new AttributeValue { S = "u123" },
          ["trx_id"] = new AttributeValue { S = "2025-10-30T10:00:00Z#trx#1" },
          ["type"] = new AttributeValue { S = "SUBSCRIBE" },
          ["fund_id"] = new AttributeValue { S = "5" },
          ["fund_name"] = new AttributeValue { S = "FPV_BTG_PACTUAL_DINAMICA" },
          ["amount"] = new AttributeValue { N = "100000" },
          ["created_at"] = new AttributeValue { S = "2025-10-30T10:00:00Z" }
        }
      };

      db.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new QueryResponse { Items = items });

      var service = new HistoryService(db.Object);
      var result = await service.GetAsync("u123");

      result.Should().NotBeNull();
      result.Count.Should().Be(1);
      result[0].Type.Should().Be("SUBSCRIBE");
      result[0].FundId.Should().Be("5");
      result[0].Amount.Should().Be(100000);
    }
  }
}
