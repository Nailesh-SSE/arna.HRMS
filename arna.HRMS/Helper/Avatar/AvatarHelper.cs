namespace arna.HRMS.Helper.Avatar;

public static class AvatarHelper
{
    public static string GetInitials(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "EM";
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}";
        return fullName.Length >= 2 ? fullName[..2].ToUpper() : fullName.ToUpper();
    }
}
