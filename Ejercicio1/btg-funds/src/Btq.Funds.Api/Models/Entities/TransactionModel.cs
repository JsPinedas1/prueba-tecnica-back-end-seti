namespace Btq.Funds.Api.Models.Entities
{
  public class TransactionModel
  {
    public string UserId { get; set; } = default!;
    public string TrxId { get; set; } = default!;
    public string FundId { get; set; } = default!;
    public string FundName { get; set; } = default!;
    public string Type { get; set; } = default!;
    public int Amount { get; set; }
    public string CreatedAtIso { get; set; } = default!;
  }
}
