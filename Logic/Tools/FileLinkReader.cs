public class FileReader : ITool<FileReaderOptions, string>
{
    public static async Task<string> ExecuteAsync(FileReaderOptions toolOptions)
    {
        switch (toolOptions.Type)
        {
            case EFileReaderFileInputType.link:
                break;
            case EFileReaderFileInputType.path:
                break;
            default:
                throw new NullReferenceException("Unknown file input type");
        }
        throw new NotImplementedException();
    }

    private static async Task<Stream> ReadFileFromLink(Uri uri)
    {
        throw new NotImplementedException("not yet implemented!");
    }
    
}

public class FileReaderOptions : IToolOptions
{
    public EFileReaderFileInputType Type { get; }
}

public enum EFileReaderFileInputType
{
    link,
    path
}