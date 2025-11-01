using System;

namespace Btq.Funds.Api.Utils
{
  public static class DateHelper
  {
    public static string NowIso() => DateTimeOffset.UtcNow.ToString("o");
  }
}
