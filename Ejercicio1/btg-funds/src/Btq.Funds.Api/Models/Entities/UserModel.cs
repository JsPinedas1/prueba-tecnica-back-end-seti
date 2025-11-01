namespace Btq.Funds.Api.Models.Entities
{
  public class UserModel
  {
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string NotifyPref { get; set; }
    public int Balance { get; set; }
  }
}
