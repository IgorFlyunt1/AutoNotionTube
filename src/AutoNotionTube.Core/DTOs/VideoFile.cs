namespace AutoNotionTube.Core.DTOs;

public class VideoFile
{
    public string FileName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Directory { get; set; } = null!;
    public int Seconds { get; set; }
    public double SizeMb { get; set; }
}
