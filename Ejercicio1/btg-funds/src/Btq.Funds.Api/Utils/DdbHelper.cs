using System;
using System.Collections.Generic;
using System.Globalization;
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

    public string UsersTable => Environment.GetEnvironmentVariable("USERS_TABLE")!;
    public string FundsTable => Environment.GetEnvironmentVariable("FUNDS_TABLE")!;
    public string SubsTable => Environment.GetEnvironmentVariable("SUBS_TABLE")!;
    public string TrxTable => Environment.GetEnvironmentVariable("TRX_TABLE")!;

    public static string NowIso() => DateHelper.NowIso();

    public static void AddStringAttr(Dictionary<string, AttributeValue> item, string key, string? value)
    {
      if (!string.IsNullOrWhiteSpace(value))
        item[key] = new AttributeValue { S = value };
    }

    public static void AddNumberAttr(Dictionary<string, AttributeValue> item, string key, decimal? value)
    {
      if (value.HasValue)
        item[key] = new AttributeValue { N = value.Value.ToString(CultureInfo.InvariantCulture) };
    }

    public static void AddNumberAttr(Dictionary<string, AttributeValue> item, string key, int? value)
    {
      if (value.HasValue)
        item[key] = new AttributeValue { N = value.Value.ToString(CultureInfo.InvariantCulture) };
    }

    public static string GetString(Dictionary<string, AttributeValue> item, string key, string defaultValue = "")
      => item.TryGetValue(key, out var v) ? v.S ?? defaultValue : defaultValue;

    public static int GetInt(Dictionary<string, AttributeValue> item, string key, int defaultValue = 0)
      => item.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v.N) && int.TryParse(v.N, NumberStyles.Any, CultureInfo.InvariantCulture, out var n)
        ? n : defaultValue;

    public async Task<UserModel?> GetUser(string userId)
    {
      var res = await db.GetItemAsync(new GetItemRequest
      {
        TableName = UsersTable,
        Key = new Dictionary<string, AttributeValue> { ["user_id"] = new AttributeValue { S = userId } }
      }, CancellationToken.None);

      if (res.Item == null || res.Item.Count == 0) return null;

      return new UserModel
      {
        UserId = GetString(res.Item, "user_id"),
        Name = GetString(res.Item, "name"),
        Email = GetString(res.Item, "email"),
        Phone = GetString(res.Item, "phone"),
        NotifyPref = GetString(res.Item, "notify_pref"),
        Balance = GetInt(res.Item, "balance", 0)
      };
    }

    public async Task<FundModel?> GetFund(string fundId)
    {
      var res = await db.GetItemAsync(new GetItemRequest
      {
        TableName = FundsTable,
        Key = new Dictionary<string, AttributeValue> { ["fund_id"] = new AttributeValue { S = fundId } }
      }, CancellationToken.None);

      if (res.Item == null || res.Item.Count == 0) return null;

      return new FundModel
      {
        FundId = GetString(res.Item, "fund_id"),
        Name = GetString(res.Item, "name"),
        MinAmount = GetInt(res.Item, "min_amount", 0),
        Category = GetString(res.Item, "category")
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

      return res.Item ?? new Dictionary<string, AttributeValue>();
    }

    public Task PutSubscription(string userId, string fundId, string status, string createdAtIso)
    {
      var item = new Dictionary<string, AttributeValue>();
      AddStringAttr(item, "user_id", userId);
      AddStringAttr(item, "fund_id", fundId);
      AddStringAttr(item, "status", status);
      AddStringAttr(item, "created_at", createdAtIso);

      return db.PutItemAsync(new PutItemRequest
      {
        TableName = SubsTable,
        Item = item
      });
    }

    public Task PutTransaction(TransactionModel trx)
    {
      var item = new Dictionary<string, AttributeValue>();
      AddStringAttr(item, "user_id", trx.UserId);
      AddStringAttr(item, "trx_id", trx.TrxId);
      AddStringAttr(item, "fund_id", trx.FundId);
      AddStringAttr(item, "fund_name", trx.FundName);
      AddStringAttr(item, "type", trx.Type);
      AddStringAttr(item, "created_at", trx.CreatedAtIso);
      AddNumberAttr(item, "amount", trx.Amount);

      return db.PutItemAsync(new PutItemRequest
      {
        TableName = TrxTable,
        Item = item
      });
    }
  }
}
