using GNScript.Models;
using System.Collections;

namespace GNScript.Helpers;
public static class ArrayHelpers
{
    public static string ToPrintableArray(this ExecutionModel executionModel)
    {
        if (executionModel.IsEmptyValue)
            return string.Empty;

        if (executionModel.IsArray() == false)
            return string.Empty;

        return ConvertListToString((List<object>)executionModel);
    }

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
}
