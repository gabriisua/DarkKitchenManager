namespace Roscoff.Application.Wrappers;

public class Result<T>
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public T? Data { get; set; }

    // Costruttore vuoto per la serializzazione
    public Result() { }

    // Costruttore per il SUCCESSO
    internal Result(T data, string? message = null)
    {
        Succeeded = true;
        Message = message;
        Data = data;
    }

    // Costruttore per l'ERRORE
    internal Result(string message, List<string>? errors = null)
    {
        Succeeded = false;
        Message = message;
        Errors = errors;
    }

    // Metodi statici helper per facilitare la creazione nei Service
    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T>(data, message);
    }

    public static Result<T> Failure(string message, List<string>? errors = null)
    {
        return new Result<T>(message, errors);
    }
}