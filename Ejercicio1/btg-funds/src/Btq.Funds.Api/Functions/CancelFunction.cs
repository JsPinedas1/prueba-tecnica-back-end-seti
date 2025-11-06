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

namespace Btq.Funds.Api.Functions
{
  public class CancelFunction
  {
    private readonly CancelService _service;

    public CancelFunction()
    {
      _service = new CancelService();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(
      APIGatewayProxyRequest request,
      ILambdaContext context
    )
    {
      try
      {
        if (string.IsNullOrWhiteSpace(request.Body))
          return ResponseBuilder.BadRequest("invalid body");

        var payload = JsonSerializer.Deserialize<CancelRequest>(request.Body);
        if (payload == null)
          return ResponseBuilder.BadRequest("invalid body");

        var result = await _service.ProcessAsync(payload);
        return ResponseBuilder.Ok(result);
      }
      catch (Exception ex)
      {
        return ResponseBuilder.Error(ex.Message);
      }
    }
  }
}
