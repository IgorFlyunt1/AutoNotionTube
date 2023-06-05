using AutoNotionTube.Core.DTOs;

namespace AutoNotionTube.Core.Interfaces;

public interface IVideoRepository
{
    Task<IReadOnlyCollection<VideoFile>> GetVideosAsync();
    Task MoveFailedVideo(string videoFilePath);
    Task CreateCaptionFailedFile(string videoId, CancellationToken cancellationToken);
}
