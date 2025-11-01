using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace Btq.Funds.Api.Utils
{
  public static class ResponseBuilder
  {
    public static APIGatewayProxyResponse Ok(object body) =>
      new()
      {
        StatusCode = (int)HttpStatusCode.OK,
        Body = JsonSerializer.Serialize(body),
        Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
      };

    public static APIGatewayProxyResponse BadRequest(string message) =>
      new()
      {
        StatusCode = (int)HttpStatusCode.BadRequest,
        Body = JsonSerializer.Serialize(new { message }),
        Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
      };

    public static APIGatewayProxyResponse Error(string message) =>
      new()
      {
        StatusCode = (int)HttpStatusCode.InternalServerError,
        Body = JsonSerializer.Serialize(new { message }),
        Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
      };
  }
}
