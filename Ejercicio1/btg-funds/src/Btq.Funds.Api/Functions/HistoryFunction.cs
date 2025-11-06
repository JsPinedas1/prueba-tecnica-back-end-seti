using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Btq.Funds.Api.Services;
using Btq.Funds.Api.Utils;

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
        var pathParams = request.PathParameters ?? new Dictionary<string, string>();
        pathParams.TryGetValue("user_id", out var userId);

        if (string.IsNullOrWhiteSpace(userId))
          return ResponseBuilder.BadRequest("user_id is required");

        var result = await _service.GetAsync(userId);
        return ResponseBuilder.Ok(result);
      }
      catch (Exception ex)
      {
        return ResponseBuilder.Error(ex.Message);
      }
    }
  }
}
