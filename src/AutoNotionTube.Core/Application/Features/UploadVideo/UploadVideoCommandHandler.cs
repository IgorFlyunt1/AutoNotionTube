using AutoNotionTube.Core.Interfaces;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Video = AutoNotionTube.Domain.Entities.Video;

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
                    Title = "Default Video Title",
                    CategoryId = "22", // See https://developers.google.com/youtube/v3/docs/videoCategories/list
                    DefaultAudioLanguage = "en"
                },
                Status = new VideoStatus
                {
                    PrivacyStatus = "private", // or  unlisted "private" or "public"
                },
                
            };

            using var fileStream = new FileStream(request.VideoFile, FileMode.Open);

            var videosInsertRequest = youTubeService.Videos.Insert(new Google.Apis.YouTube.v3.Data.Video{ Snippet = video.Snippet}, "snippet,status", fileStream, "video/*");
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

    private void VideosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
    {
        switch (progress.Status)
        {
            case UploadStatus.Uploading:
                _logger.LogInformation("{0} bytes sent.", progress.BytesSent);
                break;

            case UploadStatus.Failed:
                _logger.LogError("An error prevented the upload from completing.\n{0}", progress.Exception);
                break;
        }
    }

    private void VideosInsertRequest_ResponseReceived(Google.Apis.YouTube.v3.Data.Video obj)
    {
        _logger.LogInformation("Video was successfully uploaded");

    }
}
