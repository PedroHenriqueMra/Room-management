using Microsoft.DotNet.Scaffolding.Shared.Messaging;

public class GetPropertyAnonymous : IGetPropertyAnonymous
{
    public string GetMessage(object anonymousObject, string propertyName, char? delimiterInit, char? delimiterEnd)
    {
        System.Type type = anonymousObject.GetType();
        var propertyValue = type?.GetProperty(propertyName)?.GetValue(anonymousObject, null)?.ToString();

        if (!String.IsNullOrEmpty(propertyValue))
        {
            string message = propertyValue;
            if (delimiterInit != null && delimiterEnd != null)
            {
                if (propertyValue.Contains((char)delimiterInit))
                {
                    message = propertyValue.Substring(propertyValue.IndexOf((char)delimiterInit)+1);
                }
                    
                if (message.Contains((char)delimiterEnd))
                {
                    int indexEndChar = message.LastIndexOf((char)delimiterEnd);
                    message = message.Remove(indexEndChar, 1);
                }
            }

            return message.Trim();
        }

        return "An error ocurred!";
    }
}
