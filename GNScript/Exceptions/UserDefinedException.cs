namespace GNScript.Exceptions;
public class UserDefinedException : Exception
{
    public UserDefinedException(string message)
        :base(message)
    {
    }
}
