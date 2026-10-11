namespace CURSOVA.Application.Common;

public class NotFoundException(string message) : Exception(message)
{
    public static NotFoundException For<T>(object key) => new($"{typeof(T).Name} '{key}' не знайдено.");
}

public class ConflictException(string message) : Exception(message);
