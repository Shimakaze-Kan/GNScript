namespace GNScript.Exceptions;
public class ReturnException : Exception
{
    public object ReturnValue { get; }

    public ReturnException(object returnValue)
    {
        ReturnValue = returnValue;
    }
}
