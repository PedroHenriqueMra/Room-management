using System.Text.RegularExpressions;

namespace Utils.RegularExpression.User;

public static class UserCheckRegularExpression
{
    private static readonly List<string> AllowDomains = new List<string>()
    {
        "@gmail.com"
    };
    private static readonly string EmailRegex = @"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9\-]+\.[a-zA-Z]{2,})$";
    private static readonly string PasswordRegex = @"^[a-zA-Z0-9]{8,50}[^!@#$%¨&*()\[\]~^´`'?ºª¬¢£³²¹]*$";
    private static readonly string NameRegex = @"^[a-zA-Z0-9]{3,50}$";

    public static bool IsValidEmail(string email)
    {
        if (Regex.IsMatch(email, EmailRegex))
        {
            string domain = email.Substring(email.IndexOf("@"));

            return AllowDomains.Contains(domain);
        }

        return false;
    }

    public static bool IsValidPassword(string password)
    {
        if (Regex.IsMatch(password, PasswordRegex))
        {
            return true;
        }

        return false;
    }

    public static bool IsValidName(string name)
    {
        if (Regex.IsMatch(name, NameRegex))
        {
            return true;
        }

        return false;
    }

    public static bool CheckAll(string password, string email, string name)
    {
        bool passwordCheck = IsValidPassword(password);
        bool emailCheck = IsValidEmail(email);
        bool nameCheck = IsValidName(name);
        
        return passwordCheck && emailCheck && nameCheck;
    }
}
