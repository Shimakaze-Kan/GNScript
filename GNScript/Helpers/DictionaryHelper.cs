using System.Text;

namespace GNScript.Helpers;
public static class DictionaryHelper
{
    public static string ToPrintableDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        var sb = new StringBuilder();
        sb.Append("{");

        foreach (var kvp in dictionary)
        {
            sb.AppendFormat("{0}: {1}, ", kvp.Key, kvp.Value);
        }

        if (dictionary.Count > 0)
        {
            sb.Length -= 2;
        }

        sb.Append("}");
        return sb.ToString();
    }
}
