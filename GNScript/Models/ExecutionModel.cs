namespace GNScript.Models;
public class ExecutionModel
{
    public static ExecutionModel Empty => new(null, true);
    public object? Value { get; }
    public bool IsEmptyValue { get; }
    public ExecutionModelValueType ModelType { get; }

    private ExecutionModel(object? value, bool isEmptyValue)
    {
        var stringValue = (value ?? string.Empty).ToString();
        if (int.TryParse(stringValue, out var valueInt))
        {
            ModelType = ExecutionModelValueType.Int;
            Value = valueInt;
        }
        else
        {
            ModelType = ExecutionModelValueType.String;
            Value = stringValue;
        }

        IsEmptyValue = isEmptyValue;
    }

    public bool IsString()
    {
        return ModelType == ExecutionModelValueType.String;
    }

    public bool IsInt()
    {
        return ModelType == ExecutionModelValueType.Int;
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
}

public enum ExecutionModelValueType
{
    Int,
    String,
}