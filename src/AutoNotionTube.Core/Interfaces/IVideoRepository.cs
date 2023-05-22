using AutoNotionTube.Domain.Entities;

namespace AutoNotionTube.Core.Interfaces;

public interface IVideoRepository
{
    Task<IReadOnlyCollection<VideoFile>> GetVideosAsync();
}
