namespace AutoNotionTube.Domain.Entities;

public class VideoFile
{
    public string FileName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public long Size { get; set; }
    public string? Directory { get; set; }
}
