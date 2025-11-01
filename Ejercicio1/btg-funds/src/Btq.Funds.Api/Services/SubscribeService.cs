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
  public class SubscribeService
  {
    private readonly IAmazonDynamoDB _db;
    private readonly NotificationService _notifier;
    private readonly DdbHelper _ddb;

    public SubscribeService()
    {
      _db = new AmazonDynamoDBClient();
      _notifier = new NotificationService();
      _ddb = new DdbHelper(_db);
    }

    public SubscribeService(IAmazonDynamoDB db, NotificationService notifier, DdbHelper ddb)
    {
      _db = db;
      _notifier = notifier;
      _ddb = ddb;
    }

    public async Task<SubscribeResponse> ProcessAsync(SubscribeRequest request)
    {
      var user = await _ddb.GetUser(request.UserId);
      var fund = await _ddb.GetFund(request.FundId);

      if (fund == null)
        throw new Exception("Fondo no encontrado");

      if (request.Amount < fund.MinAmount)
        throw new Exception("Monto inferior al mínimo de vinculación");

      if (user.Balance < request.Amount)
        throw new Exception($"No tiene saldo disponible para vincularse al fondo {fund.Name}");

      var trxId = $"trx#{Guid.NewGuid()}";
      var newBalance = user.Balance - request.Amount;
      var now = DdbHelper.NowIso();

      var transact = new TransactWriteItemsRequest
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
              ConditionExpression = "balance >= :amt",
              ExpressionAttributeValues = new Dictionary<string, AttributeValue>
              {
                [":nb"] = new AttributeValue { N = newBalance.ToString() },
                [":amt"] = new AttributeValue { N = request.Amount.ToString() }
              }
            }
          },
          new TransactWriteItem
          {
            Put = new Put
            {
              TableName = _ddb.SubsTable,
              Item = new Dictionary<string, AttributeValue>
              {
                ["user_id"] = new AttributeValue { S = request.UserId },
                ["fund_id"] = new AttributeValue { S = request.FundId },
                ["status"] = new AttributeValue { S = "ACTIVE" },
                ["amount"] = new AttributeValue { N = request.Amount.ToString() },
                ["subscribed_at"] = new AttributeValue { S = now }
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
                ["fund_name"] = new AttributeValue { S = fund.Name },
                ["type"] = new AttributeValue { S = "SUBSCRIBE" },
                ["amount"] = new AttributeValue { N = request.Amount.ToString() },
                ["created_at"] = new AttributeValue { S = now }
              }
            }
          }
        }
      };

      await _db.TransactWriteItemsAsync(transact, CancellationToken.None);

      await _notifier.PublishAsync(
        "Notificación de Suscripción",
        $"Suscripción exitosa al fondo {fund.Name} por {request.Amount} COP"
      );

      return new SubscribeResponse
      {
        TrxId = trxId,
        NewBalance = newBalance
      };
    }
  }
}
