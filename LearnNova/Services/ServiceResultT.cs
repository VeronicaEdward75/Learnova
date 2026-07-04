namespace LearnNova.Services;

public record ServiceResult<T>(bool Succeeded, IEnumerable<string> Errors, T? Data)
{
    public static ServiceResult<T> Success(T data) => new(true, [], data);
    public static ServiceResult<T> Fail(IEnumerable<string> errors) => new(false, errors, default);
    public static ServiceResult<T> Fail(string error) => new(false, [error], default);
}
