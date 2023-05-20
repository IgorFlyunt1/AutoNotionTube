namespace AutoNotionTube.Domain.Entities;

public class Video
{
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public bool UploadStatus { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}
