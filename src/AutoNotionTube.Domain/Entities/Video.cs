using Google.Apis.YouTube.v3.Data;

namespace AutoNotionTube.Domain.Entities;

public class Video
{
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public bool UploadStatus { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
    public VideoSnippet Snippet { get; set; }
    public VideoStatus Status { get; set; }
}
