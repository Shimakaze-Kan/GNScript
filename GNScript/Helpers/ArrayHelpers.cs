using GNScript.Models;
using System.Collections;

namespace GNScript.Helpers;
public static class ArrayHelpers
{
    private static string ConvertListToString(object? value)
    {
        if (value == null) 
            return string.Empty;

        var result = "[";
        var list = value as IList;
        for(int i=0; i< list.Count; i++)
        {
            var item = list[i];
            if (item is IList)
            {
                result += ConvertListToString(item);
            }
            else if (item is null)
            {
                result += "(void)";
            }
            else
            {
                result += item;
            }
            result += i == list.Count - 1 ? "" : ", ";
        }
        result += "]";

        return result;
    }

    public static void ReplaceGNArray(this List<object> list, object[] indicesAndValue)
    {
        if (indicesAndValue.Length < 2)
        {
            throw new ArgumentException("Must provide at least one index and one value.");
        }

        int[] indices = new int[indicesAndValue.Length - 1];
        for (int i = 0; i < indicesAndValue.Length - 1; i++)
        {
            indices[i] = Convert.ToInt32(indicesAndValue[i]);
        }
        object newValue = indicesAndValue[^1];

        ReplaceRecursive(list, newValue, indices, 0);
    }

    private static void ReplaceRecursive(List<object> list, object newValue, int[] indices, int depth)
    {
        if (depth == indices.Length - 1)
        {
            list[indices[depth]] = newValue;
        }
        else
        {
            ReplaceRecursive((List<object>)list[indices[depth]], newValue, indices, depth + 1);
        }
    }

    public static List<object> DeepCopy(this List<object> list)
    {
        var newList = new List<object>();
        foreach (var item in list)
        {
            if (item is IList)
            {
                newList.Add(DeepCopy(item as List<object>));
            }
            else
            {
                newList.Add(item);
            }
        }
        return newList;
    }

    public static List<object> RepeatList(this List<object> list, int count)
    {
        List<object> result = [];

        for (int i = 0; i < count; i++)
        {
            result.AddRange(list);
        }

        return result;
    }

    public static List<List<object>> Chunk(List<object> source, int n)
    {
        if (n <= 0)
        {
            throw new ArgumentException("Number of parts must be greater than zero.", nameof(n));
        }

        List<List<object>> result = [];
        int totalCount = source.Count;
        int size = (int)Math.Ceiling((double)totalCount / n);

        for (int i = 0; i < n; i++)
        {
            result.Add(source.Skip(i * size).Take(size).ToList());
        }

        return result;
    }

    public static int CountOccurrences(this List<object> list1, List<object> list2)
    {
        if (list1.Count % list2.Count != 0)
        {
            return 0;
        }

        int count = 0;
        int length = list2.Count;

        for (int i = 0; i <= list1.Count - length; i += length)
        {
            bool match = true;
            for (int j = 0; j < length; j++)
            {
                if (!list1[i + j].Equals(list2[j]))
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                count++;
            }
            else
            {
                return 0;
            }
        }

        return count;
    }

    public static List<List<object>> MultiplyLists(this List<object> list1, List<int> list2)
    {
        if (list1.Count != list2.Count)
        {
            throw new ArgumentException("Both lists must have the same length.");
        }

        List<List<object>> result = [];

        for (int i = 0; i < list1.Count; i++)
        {
            List<object> innerList = [];

            for (int j = 0; j < list2[i]; j++)
            {
                innerList.Add(list1[i]);
            }

            result.Add(innerList);
        }

        return result;
    }

    public static (T, T) ToTuple<T>(this T[] array, int index1, int index2)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (index1 < 0 || index1 >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(index1));
        if (index2 < 0 || index2 >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(index2));

        return (array[index1], array[index2]);
    }
}
