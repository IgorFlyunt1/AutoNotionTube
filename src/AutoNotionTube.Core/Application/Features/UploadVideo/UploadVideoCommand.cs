using MediatR;
using GoogleVideo = Google.Apis.YouTube.v3.Data.Video;

namespace AutoNotionTube.Core.Application.Features.UploadVideo;

public sealed class UploadVideoCommand : IRequest<GoogleVideo?>
{
    public string VideoFile { get; set; }
    public string VideoTitle { get; set; }
}