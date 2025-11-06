using System.Text.Json.Serialization;

namespace Btq.Funds.Api.Models.Requests
{
  public class BaseRequest
  {
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = default!;

    [JsonPropertyName("fundId")]
    public string FundId { get; set; } = default!;
  }
}
