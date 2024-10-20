using System.Text.RegularExpressions;

namespace MinimalApi.Services.Utils.RegularExpression.Room;

public static class RoomCheckRegularExpression
{
    private static readonly string PasswordRegex = @"^[a-zA-Z0-9]{8,50}[^!@#$%¨&*()\[\]~^´`'?ºª¬¢£³²¹]*$";

    public static bool RoomPasswordIsValid(string password)
    {
        return Regex.IsMatch(password, PasswordRegex);
    }
}
