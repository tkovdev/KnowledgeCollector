using Aspose.Pdf.Text;
using Microsoft.Graph;

namespace KnowledgeCollector.Logic.Tools;
public record FileReaderArgs(string Uri);

public class FileReader(GraphServiceClient graphClient, HttpClient httpClient) : ToolBase<FileReaderArgs>
{
    public override string Name => "read_document";
    public override string Description => "Read text from a SharePoint link to a PDF. Use when the user provides a document URL.";
    public override BinaryData ParametersSchema => BinaryData.FromString("""
        {
          "type": "object",
          "properties": {
            "uri": {
              "type": "string",
              "description": "The full URL of the document, copied exactly from the user's message."
            }
          },
          "required": ["uri"]
        }
        """);

    protected override Task<string> RunAsync(FileReaderArgs args)
    {
        var uri = new Uri(args.Uri);
        switch (uri.Host)
        {
            case var u when u.Contains("sharepoint.com"): 
                return ReadSharepointFileFromLink(uri, graphClient);
            default:
                return ReadFileFromLink(uri, httpClient);
        }
        
    }

    private static async Task<string> ReadSharepointFileFromLink(Uri uri, GraphServiceClient graphClient)
    {
        string hostname = uri.Host;
        string[] pathArr = uri.AbsolutePath.Split('/');

        if (!hostname.Contains("sharepoint.com")) throw new NotImplementedException("Only sharepoint links are accepted");
        if (!pathArr[1].Equals("sites")) throw new NotImplementedException("The relative path of the sharepoint link does not appear to be correct. Links should include /sites/<somesitename>");
        
        string sitePath = Path.Join(pathArr[1], pathArr[2]);
        string fileRelativePath = Path.Join(pathArr.Where((v,i) => i != 0 && i != 1 && i != 2).ToArray());
        
        var site = await graphClient.Sites[$"{hostname}:{sitePath}"].GetAsync() ?? throw new NullReferenceException($"Could not find sharepoint site: {sitePath}");
        string siteId = site.Id ?? throw new NullReferenceException("Could not find ID of sharepoint site");

        var drive = await graphClient.Sites[siteId].Drive.GetAsync() ?? throw new NullReferenceException($"Could not find sharepoint drive for site: {sitePath}");
        string driveId = drive.Id ?? throw new NullReferenceException("Could not find ID of sharepint drive");

        var stream = await graphClient.Drives[driveId]
                        .Items[$"root:/{fileRelativePath}"]
                        .Content.GetAsync() ?? throw new NullReferenceException("Could not read PDF into a stream");
        
        var ext = Path.GetExtension(fileRelativePath);
        switch (ext)
        {
            case ".pdf": 
                return ReadPDFText(stream);
            default:
                throw new NotImplementedException($"A file handler for type: {ext} is not defined");
        }
    }

    private static async Task<string> ReadFileFromLink(Uri uri, HttpClient httpClient)
    {
        var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        
        var ext = GetFileExtension(response);
        switch (ext)
        {
            case ".pdf": 
                return ReadPDFText(stream);
            default:
                throw new NotImplementedException($"A file handler for type: {ext} is not defined");
        }
    }

    private static string ReadPDFText(Stream stream)
    {
        var textAbsorber = new TextAbsorber();
        using (var document = new Aspose.Pdf.Document(stream))
        {
            document.Pages.Accept(textAbsorber);
        }
        return textAbsorber.Text;
    }

    private static string GetFileExtension(HttpResponseMessage response)
    {
        var contentDisposition = response.Content.Headers.ContentDisposition;
        if (contentDisposition != null)
        {
            string fileName = (contentDisposition.FileNameStar ?? contentDisposition.FileName) ?? throw new NullReferenceException("FileName & FileStar null");
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                string ext = Path.GetExtension(fileName.Trim('"'));
                if (!string.IsNullOrEmpty(ext)) return ext;
            }
        }

        string mimeType = response.Content.Headers.ContentType?.MediaType ?? throw new NullReferenceException("MediaType null");
        if (!string.IsNullOrWhiteSpace(mimeType))
        {
            string ext = MapMimeTypeToExtension(mimeType);
            if (!string.IsNullOrEmpty(ext)) return ext;
        }

        string urlPath = response.RequestMessage?.RequestUri?.LocalPath ?? throw new NullReferenceException("LocalPath null");
        if (!string.IsNullOrEmpty(urlPath))
        {
            string ext = Path.GetExtension(urlPath);
            if (!string.IsNullOrEmpty(ext)) return ext;
        }

        throw new Exception("Unable to determine file type");
    }

    private static string MapMimeTypeToExtension(string mimeType)
    {
        return mimeType.ToLowerInvariant() switch
        {
            "application/pdf" => ".pdf",
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "text/plain" => ".txt",
            "application/zip" => ".zip",
            _ => throw new Exception("File extension unknown")
        };
    }
}

