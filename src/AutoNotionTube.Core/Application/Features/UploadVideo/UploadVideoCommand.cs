using MediatR;

namespace AutoNotionTube.Core.Application.Features.UploadVideo;

public sealed class UploadVideoCommand : IRequest<Unit>
{
    public string VideoFile { get; set; }
    public string VideoTitle { get; set; }
}