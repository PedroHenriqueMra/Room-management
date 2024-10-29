using Microsoft.DotNet.Scaffolding.Shared.Messaging;

public class GetMessageError : IGetMessageError
{
    public string GetMessage(object anonymousObject, string propertyName, char delimiterInit, char delimiterEnd)
    {
        System.Type type = anonymousObject.GetType();
        var propertyValue = type?.GetProperty(propertyName)?.GetValue(anonymousObject, null)?.ToString();

        if (!String.IsNullOrEmpty(propertyValue))
        {
            string message = propertyValue;
            if (propertyValue.Contains(delimiterInit))
            {
                message = propertyValue.Substring(propertyValue.IndexOf(delimiterInit)+1);
            }
            
            if (message.Contains(delimiterEnd))
            {
                int indexEndChar = message.LastIndexOf(delimiterEnd);
                message = message.Remove(indexEndChar, 1);
            }

            return message.Trim();
        }

        return "An error ocurred!";
    }
}
