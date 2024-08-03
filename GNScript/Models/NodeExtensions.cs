namespace GNScript.Models;
public enum ArrayExtension
{
    Length,
    Reverse,
    ToString,
    RemoveAt,
    AddAt,
    Append,
    Prepend,
    ReplaceAt,
    Has
}

public enum CommonValueExtension
{
    Type
}

public enum StringExtension
{
    ToLower,
    ToUpper,
    Reverse,
    ToArray,
    Length,
    Split,
    ReplaceAt,
    ToInt,
    CanConvertToInt
}

public enum BoxExtension
{
    IsInstanceOf,
    HasField,
    HasFunction,
    ReflectionSetField,
}

public enum IntExtension
{
    ToString
}
