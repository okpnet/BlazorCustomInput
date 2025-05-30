namespace BlazorCustomInput.Components
{
    public enum TextEditType
    {
        Text,
        Email,
        Tel,
        Url,
        Number,
        Password,
    }

    public static class TextEditTypeHelper
    {
        public static string GetTypeString(this TextEditType textEditType)
        {
            return textEditType switch
            {
                TextEditType.Email => "email",
                TextEditType.Url => "url",
                TextEditType.Number => "number",
                TextEditType.Password => "password",
                _ => "text"
            };
    }
    }
}
