namespace Btq.Funds.Api.Models.Requests
{
  public class SubscribeRequest : BaseRequest
  {
    public int Amount { get; set; }
    public string ClientToken { get; set; }
  }
}
