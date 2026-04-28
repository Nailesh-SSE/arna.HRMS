namespace arna.HRMS.Models.ViewModels.Auth;

public class RefreshTokenDTO
{
    public int UserId { get; set; }

    // ✅ FIX: Renamed from "Token" to "RefreshToken" to match backend DTO (RefreshTokenDto.RefreshToken)
    // Old: public string Token { get; set; }  ← frontend sent { "Token": "..." }
    // Backend expects: { "RefreshToken": "..." } (RefreshTokenDto.cs in Core)
    // With PropertyNameCaseInsensitive=true, "Token" still won't bind to "RefreshToken"
    // because they are completely different property names — causing refresh to always fail
    // and users being logged out after every access token expiry.
    public string RefreshToken { get; set; } = string.Empty;
}