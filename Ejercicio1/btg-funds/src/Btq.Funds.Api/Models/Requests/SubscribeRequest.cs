using System.Text.Json.Serialization;

namespace Btq.Funds.Api.Models.Requests
{
  public class SubscribeRequest : BaseRequest
  {
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
  }
}
