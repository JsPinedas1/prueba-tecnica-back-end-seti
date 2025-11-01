using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Btq.Funds.Api.Models.Entities;

namespace Btq.Funds.Api.Utils
{
  public class DdbHelper
  {
    private readonly IAmazonDynamoDB db;

    public DdbHelper(IAmazonDynamoDB db)
    {
      this.db = db;
    }

    public string UsersTable => Environment.GetEnvironmentVariable("USERS_TABLE");
    public string FundsTable => Environment.GetEnvironmentVariable("FUNDS_TABLE");
    public string SubsTable => Environment.GetEnvironmentVariable("SUBS_TABLE");
    public string TrxTable => Environment.GetEnvironmentVariable("TRX_TABLE");

    public static string NowIso() => DateHelper.NowIso();

    public async Task<UserModel> GetUser(string userId)
    {
      var res = await db.GetItemAsync(new GetItemRequest
      {
        TableName = UsersTable,
        Key = new Dictionary<string, AttributeValue> { ["user_id"] = new AttributeValue { S = userId } }
      }, CancellationToken.None);

      if (res.Item == null || res.Item.Count == 0) return null;

      return new UserModel
      {
        UserId = res.Item.TryGetValue("user_id", out var v1) ? v1.S : null,
        Name = res.Item.TryGetValue("name", out var v2) ? v2.S : null,
        Email = res.Item.TryGetValue("email", out var v3) ? v3.S : null,
        Phone = res.Item.TryGetValue("phone", out var v4) ? v4.S : null,
        NotifyPref = res.Item.TryGetValue("notify_pref", out var v5) ? v5.S : null,
        Balance = res.Item.TryGetValue("balance", out var v6) ? int.Parse(v6.N) : 0
      };
    }

    public async Task<FundModel> GetFund(string fundId)
    {
      var res = await db.GetItemAsync(new GetItemRequest
      {
        TableName = FundsTable,
        Key = new Dictionary<string, AttributeValue> { ["fund_id"] = new AttributeValue { S = fundId } }
      }, CancellationToken.None);

      if (res.Item == null || res.Item.Count == 0) return null;

      return new FundModel
      {
        FundId = res.Item.TryGetValue("fund_id", out var v1) ? v1.S : null,
        Name = res.Item.TryGetValue("name", out var v2) ? v2.S : null,
        MinAmount = res.Item.TryGetValue("min_amount", out var v3) ? int.Parse(v3.N) : 0,
        Category = res.Item.TryGetValue("category", out var v4) ? v4.S : null
      };
    }

    public async Task<Dictionary<string, AttributeValue>> GetSubscription(string userId, string fundId)
    {
      var res = await db.GetItemAsync(new GetItemRequest
      {
        TableName = SubsTable,
        Key = new Dictionary<string, AttributeValue>
        {
          ["user_id"] = new AttributeValue { S = userId },
          ["fund_id"] = new AttributeValue { S = fundId }
        }
      }, CancellationToken.None);

      return res.Item;
    }
  }
}
