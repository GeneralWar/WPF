using System.Text;

static public partial class Extension
{
    static public string EscapeToWindowsPresentationFoundationHeader(this string value)
    {
        StringBuilder builder = new StringBuilder();
        foreach (char c in value)
        {
            if ('_' == c)
            {
                builder.Append("__");
            }
            else
            {
                builder.Append(c);
            }
        }
        return builder.ToString();
    }

    static public string UnescapeFromWindowsPresentationFoundationHeader(this string value)
    {
        return value.Replace("__", "_");
    }
}
