using System.Collections;

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

        if (IsListType(value))
        {
            ModelType = ExecutionModelValueType.Array;
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

        return (model.Value as IList).Cast<object>().ToList();
    }

    private static bool IsListType(object? o)
    {
        if (o == null) return false;
        return o is IList &&
               o.GetType().IsGenericType &&
               o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
    }
}

public enum ExecutionModelValueType
{
    Int,
    String,
    Array
}