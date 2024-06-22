namespace GNScript.Models;
public class CallReturnValue
{
    public object ReturnValue { get; }

    public CallReturnValue(object returnValue)
    {
        ReturnValue = returnValue;
    }
}
