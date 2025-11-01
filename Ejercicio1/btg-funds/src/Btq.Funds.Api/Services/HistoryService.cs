using System.Collections.Generic;
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
    private readonly IAmazonDynamoDB _db;
    private readonly DdbHelper _ddb;

    public HistoryService()
    {
      _db = new AmazonDynamoDBClient();
      _ddb = new DdbHelper(_db);
    }

    public HistoryService(IAmazonDynamoDB db)
    {
      _db = db;
      _ddb = new DdbHelper(_db);
    }

    public async Task<List<TransactionModel>> GetAsync(string userId)
    {
      var q = new QueryRequest
      {
        TableName = _ddb.TrxTable,
        KeyConditionExpression = "user_id = :uid",
        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
          [":uid"] = new AttributeValue { S = userId }
        },
        ScanIndexForward = false
      };

      var res = await _db.QueryAsync(q, CancellationToken.None);
      var list = new List<TransactionModel>();

      foreach (var item in res.Items)
      {
        list.Add(new TransactionModel
        {
          UserId = item["user_id"].S,
          TrxId = item["trx_id"].S,
          FundId = item["fund_id"].S,
          FundName = item.ContainsKey("fund_name") ? item["fund_name"].S : "",
          Type = item["type"].S,
          Amount = int.Parse(item["amount"].N),
          CreatedAt = System.DateTime.Parse(item["created_at"].S)
        });
      }

      return list;
    }
  }
}
