using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Btq.Funds.Api.Services;
using Btq.Funds.Api.Utils;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Btq.Funds.Api.Functions
{
  public class HistoryFunction
  {
    private readonly HistoryService _service;

    public HistoryFunction()
    {
      _service = new HistoryService();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(
      APIGatewayProxyRequest request,
      ILambdaContext context
    )
    {
      try
      {
        if (!request.PathParameters?.TryGetValue("user_id", out var userId) ?? true || string.IsNullOrWhiteSpace(userId))
          return ResponseBuilder.BadRequest("user_id is required");

        var result = await _service.GetAsync(userId);
        return ResponseBuilder.Ok(result);
      }
      catch (Exception ex)
      {
        return new APIGatewayProxyResponse
        {
          StatusCode = (int)HttpStatusCode.InternalServerError,
          Body = JsonSerializer.Serialize(new { message = ex.Message }),
          Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        };
      }
    }
  }
}
