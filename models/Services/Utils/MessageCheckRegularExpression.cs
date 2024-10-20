using System.Text.RegularExpressions;

namespace MinimalApi.Services.Utils.RegularExpression.Message;

public static class MessageCheckRegularExpression
{
    private static readonly string MessageRegex = @"^[a-zA-Z0-9]{1,200}$";

    public static bool MessageIsValid(string message)
    {
        if (String.IsNullOrEmpty(message))
        {
            return false;
        }

        return Regex.IsMatch(message, MessageRegex);
    }
}
