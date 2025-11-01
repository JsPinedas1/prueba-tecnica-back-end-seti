using System;

namespace Btq.Funds.Api.Models.Entities
{
  public class TransactionModel
  {
    public string UserId { get; set; }
    public string TrxId { get; set; }
    public string FundId { get; set; }
    public string FundName { get; set; }
    public string Type { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
  }
}
