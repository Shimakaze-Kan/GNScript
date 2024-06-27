using GNScript.Helpers;
using System.Collections;
using System.Text;

namespace GNScript.Models;
public class ExecutionModel
{
    public static ExecutionModel Empty => new(null, true);
    public object? Value { get; }
    public bool IsEmptyValue { get; }
    public ExecutionModelValueType ModelType { get; }

    private ExecutionModel(object? value, bool isEmptyValue)
    {
        if (isEmptyValue)
        {
            IsEmptyValue = isEmptyValue;
            return;
        }

        if (value == null)
        {
            ModelType = ExecutionModelValueType.Void;
        }
        else if (IsListType(value))
        {
            ModelType = ExecutionModelValueType.Array;
        }
        else if (IsDictionaryType(value))
        {
            ModelType = ExecutionModelValueType.RefBox;
        }
        else if (value is int)
        {
            ModelType = ExecutionModelValueType.Int;
        }
        else if (value is string)
        {
            ModelType = ExecutionModelValueType.String;
        }
        else
        {
            throw new Exception("Invalid type");
        }

        Value = value;
    }

    public bool IsString()
    {
        return ModelType == ExecutionModelValueType.String;
    }

    public bool IsInt()
    {
        return ModelType == ExecutionModelValueType.Int;
    }

    public bool IsArray()
    {
        return ModelType == ExecutionModelValueType.Array;
    }
    
    public bool IsRefBox()
    {
        return ModelType == ExecutionModelValueType.RefBox;
    }

    public static ExecutionModel FromObject(object value)
    {
        return new(value, false);
    }

    public static implicit operator ExecutionModel(int value)
    {
        return new(value, false);
    }
    
    public static implicit operator ExecutionModel(string value)
    {
        return new(value, false);
    }

    public static explicit operator int(ExecutionModel model)
    {
        if (model.IsEmptyValue)
        {
            throw new Exception("Expected value");
        }

        return (int)model.Value;
    }

    public static explicit operator string(ExecutionModel model)
    {
        if (model.IsEmptyValue)
        {
            throw new Exception("Expected value");
        }

        return (string)model.Value;
    }

    public static explicit operator List<object>(ExecutionModel model)
    {
        if (model.IsEmptyValue)
        {
            throw new Exception("Expected value");
        }

        return (model.Value as IList).Cast<object>().ToList().DeepCopy();
    }
    
    public static explicit operator Dictionary<FunctionVariableDictionaryKey, RefBoxElement>(ExecutionModel model)
    {
        if (model.IsEmptyValue)
        {
            throw new Exception("Expected value");
        }

        return model.Value as Dictionary<FunctionVariableDictionaryKey, RefBoxElement>;
    }

    public string ToPrintable()
    {
        if (IsEmptyValue)
            return string.Empty;

        return ConvertToString(Value);
    }

    private static string ConvertToString(object? value)
    {
        if (value == null)
            return "(void)";

        var model = FromObject(value);

        if (model.IsArray())
            return ConvertListToString((IList)value);

        if (model.IsRefBox())
            return ConvertDictionaryToString((IDictionary)value);

        if (model.IsString())
            return @$"""{value}""";

        return value.ToString() ?? string.Empty;
    }

    private static bool IsListType(object? o)
    {
        if (o == null) return false;
        return o is IList &&
               o.GetType().IsGenericType &&
               o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
    }

    private static bool IsDictionaryType(object? o)
    {
        if (o == null) return false;
        return o is IDictionary &&
               o.GetType().IsGenericType &&
               o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
    }

    private static string ConvertListToString(IList list)
    {
        var sb = new StringBuilder();
        sb.Append("[");

        for (int i = 0; i < list.Count; i++)
        {
            sb.Append(ConvertToString(list[i]));
            if (i < list.Count - 1)
                sb.Append(", ");
        }

        sb.Append("]");
        return sb.ToString();
    }

    private static string ConvertDictionaryToString(IDictionary dictionary)
    {
        var sb = new StringBuilder();
        sb.Append("{");

        foreach (DictionaryEntry kvp in dictionary)
        {
            var model = (RefBoxElement)kvp.Value;
            if (model.Type == RefBoxElementType.Function)
            {
                var arguments = model.Function.Parameters;
                sb.AppendFormat("{0} <- ({1}), ", (kvp.Key as FunctionVariableDictionaryKey).FunctionKey.FunctionName, string.Join(", ", arguments));
            }
            else
            {
                sb.AppendFormat("{0}: {1}, ", (kvp.Key as FunctionVariableDictionaryKey).VariableName, ConvertToString(model.Value));
            }
        }

        if (dictionary.Count > 0)
        {
            sb.Length -= 2; // Remove the last comma and space
        }

        sb.Append("}");
        return sb.ToString();
    }
}

public enum ExecutionModelValueType
{
    Int,
    String,
    Array,
    RefBox,
    Void
}