using AutoNotionTube.Core.Interfaces;
using AutoNotionTube.Domain.Entities;
using MediatR;

namespace AutoNotionTube.Core.Application.Features.GetVideos;

public sealed record GetVideosQuery : IRequest<IReadOnlyCollection<VideoFile>>;

public sealed class GetVideosQueryHandler : IRequestHandler<GetVideosQuery, IReadOnlyCollection<VideoFile>>
{
    private readonly IVideoRepository _videoRepository;

    public GetVideosQueryHandler(IVideoRepository videoRepository)
    {
        _videoRepository = videoRepository;
    }

    public async Task<IReadOnlyCollection<VideoFile>> Handle(GetVideosQuery request, CancellationToken cancellationToken)
    {
        return await _videoRepository.GetVideosAsync();
    }
}
