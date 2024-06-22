namespace GNScript.Models;
public class CallReturnValue
{
    public object ReturnValue { get; }
    public bool IsVoid { get; private set; }

    private CallReturnValue()
    {
    }

    public CallReturnValue(object returnValue)
    {
        ReturnValue = returnValue;
    }

    public static CallReturnValue CreateVoidReturnValue()
    {
        return new()
        {
            IsVoid = true,
        };
    }
}
