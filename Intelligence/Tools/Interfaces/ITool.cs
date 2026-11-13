using System.Text.Json;

namespace KnowledgeCollector.Intelligence.Tools;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    BinaryData ParametersSchema { get; }
    Task<string> ExecuteAsync(string argumentsJson);
}

public abstract class ToolBase<TArgs> : ITool
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract BinaryData ParametersSchema { get; }

    protected abstract Task<string> RunAsync(TArgs args);

    public Task<string> ExecuteAsync(string argumentsJson)
    {
        var args = JsonSerializer.Deserialize<TArgs>(argumentsJson, JsonOptions)
            ?? throw new InvalidOperationException("Invalid tool arguments");
        return RunAsync(args);
    }
}