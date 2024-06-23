namespace GNScript.Helpers;
public static class ExceptionsHelper
{
    public static void FailIfTrue(bool condition, string? exceptionMessage = null)
    {
        if (condition)
            throw new Exception(exceptionMessage ?? "Invalid operation");
    }
    
    public static void FailIfFalse(bool condition, string? exceptionMessage = null)
    {
        if (condition == false)
            throw new Exception(exceptionMessage ?? "Invalid operation");
    }
}
