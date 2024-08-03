using System.Text;

namespace GNScript.Helpers;
public static class DictionaryHelper
{
    public static void CombineDictionaries<K, V>(this IDictionary<K, V> dict1, IDictionary<K, V> dict2)
    {
        foreach (var (key, value) in dict2)
        {
            dict1.TryAdd(key, value);
        }
    }
}
