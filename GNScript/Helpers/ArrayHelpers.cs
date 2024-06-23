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

        var list = (executionModel.Value as IList).Cast<object>().ToList();
        return $"[{string.Join(", ", list)}]";
    }
}
