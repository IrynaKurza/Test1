namespace Test1.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
    
}

public class ConflictException : Exception
{
    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
}