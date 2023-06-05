using AutoNotionTube.Core.DTOs;
using MediatR;
using GoogleVideo = Google.Apis.YouTube.v3.Data.Video;

namespace AutoNotionTube.Core.Application.Features.UploadVideo;

public sealed class UploadVideoCommand : IRequest<YoutubeResponse>
{
    public string VideoFile { get; set; } = null!;
    public string VideoTitle { get; set; } = null!;
}