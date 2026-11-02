public interface ITool<TOptions, TResult>
    where TOptions : IToolOptions
{
    public abstract static Task<TResult> ExecuteAsync(TOptions toolOptions);
}

public interface IToolOptions {}