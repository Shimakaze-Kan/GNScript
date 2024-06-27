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

    public static void CombineDictionaries<K, V>(this IDictionary<K, V> dict1, IDictionary<K, V> dict2)
    {
        foreach (var (key, value) in dict2)
        {
            dict1.TryAdd(key, value);
        }
    }
}
