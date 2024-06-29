namespace GNScript.Models;
public enum ArrayProperty
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

public enum CommonValueProperty
{
    Type
}

public enum StringProperty
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

public enum BoxProperty
{
    IsInstanceOf,
    HasField,
    HasFunction,
    ReflectionSetField,
}

public enum IntProperty
{
    ToString
}
