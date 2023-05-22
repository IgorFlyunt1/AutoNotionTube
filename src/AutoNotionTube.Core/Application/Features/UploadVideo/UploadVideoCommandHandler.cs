using AutoNotionTube.Core.Interfaces;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Video = AutoNotionTube.Domain.Entities.Video;
using GoogleVideo = Google.Apis.YouTube.v3.Data.Video;

namespace AutoNotionTube.Core.Application.Features.UploadVideo;

public class UploadVideoCommandHandler : IRequestHandler<UploadVideoCommand, Unit>
{
    private readonly ILogger<UploadVideoCommandHandler> _logger;
    private readonly IYoutubeService _youtubeService;

    public UploadVideoCommandHandler(ILogger<UploadVideoCommandHandler> logger, IYoutubeService youtubeService)
    {
        _logger = logger;
        _youtubeService = youtubeService;
    }

    public async Task<Unit> Handle(UploadVideoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading video: {VideoFile}", request.VideoFile);

        try
        {
            var youTubeService = await _youtubeService.CreateService(cancellationToken);

            var video = new Video
            {
                Snippet = new VideoSnippet
                {
                    Title = request.VideoTitle,
                },
                Status = new VideoStatus
                {
                    PrivacyStatus = "unlisted", // or  unlisted "private" or "public",
                    MadeForKids = false
                },
            };

            using var fileStream = new FileStream(request.VideoFile, FileMode.Open);

            var videosInsertRequest = youTubeService.Videos.Insert(
                new GoogleVideo { Snippet = video.Snippet, Status = video.Status },
                "snippet,status", fileStream, "video/*");
            videosInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
            videosInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;

            await videosInsertRequest.UploadAsync(cancellationToken);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading video to YouTube");
            throw;
        }
    }

    private void VideosInsertRequest_ProgressChanged(IUploadProgress progress)
    {
        switch (progress.Status)
        {
            case UploadStatus.Uploading:
                _logger.LogInformation("{Bytes} bytes sent", progress.BytesSent);
                break;

            case UploadStatus.Failed:
                _logger.LogError("An error prevented the upload from completing.\n{Exception}", progress.Exception);
                break;
        }
    }

    private void VideosInsertRequest_ResponseReceived(GoogleVideo video)
    {
        _logger.LogInformation("Video was successfully uploaded");
    }
}
