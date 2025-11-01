using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Btq.Funds.Api.Models.Entities;
using Btq.Funds.Api.Models.Requests;
using Btq.Funds.Api.Models.Responses;
using Btq.Funds.Api.Utils;

namespace Btq.Funds.Api.Services
{
  public class CancelService
  {
    private readonly IAmazonDynamoDB _db;
    private readonly DdbHelper _ddb;

    public CancelService()
    {
      _db = new AmazonDynamoDBClient();
      _ddb = new DdbHelper(_db);
    }

    public CancelService(IAmazonDynamoDB db, DdbHelper ddb)
    {
      _db = db;
      _ddb = ddb;
    }

    public async Task<CancelResponse> ProcessAsync(CancelRequest request)
    {
      var sub = await _ddb.GetSubscription(request.UserId, request.FundId);
      if (sub == null || !sub.ContainsKey("status"))
        throw new Exception("No existe suscripción activa");

      var status = sub["status"].S;
      if (!string.Equals(status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
        throw new Exception("Suscripción no está activa");

      var amount = int.Parse(sub["amount"].N);
      var user = await _ddb.GetUser(request.UserId);
      var fund = await _ddb.GetFund(request.FundId);
      var fundName = fund?.Name ?? "Fondo";

      var newBalance = user.Balance + amount;
      var trxId = $"trx#{Guid.NewGuid()}";
      var now = DdbHelper.NowIso();

      var tx = new TransactWriteItemsRequest
      {
        TransactItems = new List<TransactWriteItem>
        {
          new TransactWriteItem
          {
            Update = new Update
            {
              TableName = _ddb.UsersTable,
              Key = new Dictionary<string, AttributeValue> { ["user_id"] = new AttributeValue { S = request.UserId } },
              UpdateExpression = "SET balance = :nb",
              ExpressionAttributeValues = new Dictionary<string, AttributeValue>
              {
                [":nb"] = new AttributeValue { N = newBalance.ToString() }
              }
            }
          },
          new TransactWriteItem
          {
            Update = new Update
            {
              TableName = _ddb.SubsTable,
              Key = new Dictionary<string, AttributeValue>
              {
                ["user_id"] = new AttributeValue { S = request.UserId },
                ["fund_id"] = new AttributeValue { S = request.FundId }
              },
              UpdateExpression = "SET #s = :c",
              ExpressionAttributeNames = new Dictionary<string, string> { ["#s"] = "status" },
              ExpressionAttributeValues = new Dictionary<string, AttributeValue>
              {
                [":c"] = new AttributeValue { S = "CANCELLED" }
              }
            }
          },
          new TransactWriteItem
          {
            Put = new Put
            {
              TableName = _ddb.TrxTable,
              Item = new Dictionary<string, AttributeValue>
              {
                ["user_id"] = new AttributeValue { S = request.UserId },
                ["trx_id"] = new AttributeValue { S = $"{now}#{trxId}" },
                ["fund_id"] = new AttributeValue { S = request.FundId },
                ["fund_name"] = new AttributeValue { S = fundName },
                ["type"] = new AttributeValue { S = "CANCEL" },
                ["amount"] = new AttributeValue { N = amount.ToString() },
                ["created_at"] = new AttributeValue { S = now }
              }
            }
          }
        }
      };

      await _db.TransactWriteItemsAsync(tx, CancellationToken.None);

      return new CancelResponse
      {
        TrxId = trxId,
        NewBalance = newBalance
      };
    }
  }
}
