namespace AutoNotionTube.Domain.Entities;

public class NotionPage
{
    public string PageId { get; set; }
    public string EmbedUrl { get; set; }
    public string Summary { get; set; }
    public string FullText { get; set; }
    public List<string> Tags { get; set; }
}
