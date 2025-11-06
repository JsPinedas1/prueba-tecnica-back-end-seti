using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Btq.Funds.Api.Models.Entities;
using Btq.Funds.Api.Utils;

namespace Btq.Funds.Api.Services
{
  public class HistoryService
  {
    private readonly IAmazonDynamoDB db;
    private readonly DdbHelper ddb;

    public HistoryService()
    {
      db = new AmazonDynamoDBClient();
      ddb = new DdbHelper(db);
    }

    public HistoryService(IAmazonDynamoDB dynamo)
    {
      db = dynamo;
      ddb = new DdbHelper(db);
    }

    public async Task<List<TransactionModel>> GetAsync(string userId)
    {
      var req = new QueryRequest
      {
        TableName = ddb.TrxTable,
        KeyConditionExpression = "user_id = :uid",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
          [":uid"] = new AttributeValue { S = userId }
        }
      };

      var res = await db.QueryAsync(req, CancellationToken.None);

      var list = new List<TransactionModel>();

      foreach (var it in res.Items)
      {
        var model = new TransactionModel
        {
          UserId = DdbHelper.GetString(it, "user_id"),
          TrxId = DdbHelper.GetString(it, "trx_id"),
          FundId = DdbHelper.GetString(it, "fund_id"),
          FundName = DdbHelper.GetString(it, "fund_name"),
          Type = DdbHelper.GetString(it, "type"),
          Amount = DdbHelper.GetInt(it, "amount", 0),
          CreatedAtIso = DdbHelper.GetString(it, "created_at")
        };

        list.Add(model);
      }
      return list.OrderByDescending(x => x.CreatedAtIso).ToList();
    }
  }
}
