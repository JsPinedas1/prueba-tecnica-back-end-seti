using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Btq.Funds.Api.Models.Requests;
using Btq.Funds.Api.Services;
using Btq.Funds.Api.Utils;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Btq.Funds.Api.Functions
{
  public class SubscribeFunction
  {
    private readonly SubscribeService _service;

    public SubscribeFunction()
    {
      _service = new SubscribeService();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(
      APIGatewayProxyRequest request,
      ILambdaContext context
    )
    {
      try
      {
        var payload = JsonSerializer.Deserialize<SubscribeRequest>(request.Body);
        var result = await _service.ProcessAsync(payload);
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
