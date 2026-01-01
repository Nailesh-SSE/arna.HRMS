namespace arna.HRMS.ClientServices.Auth;

public interface ITokenProvider
{
    string? Token { get; set; }
}

public class TokenProvider : ITokenProvider 
{
    public string? Token { get; set; }
}
