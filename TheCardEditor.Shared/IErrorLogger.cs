namespace TheCardEditor.Shared;

public interface IErrorLogger
{
    Task LogError(string message, string stackTrace = "");
}
