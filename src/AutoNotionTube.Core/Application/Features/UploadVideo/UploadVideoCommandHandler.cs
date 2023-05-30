using AutoNotionTube.Core.Interfaces;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Video = AutoNotionTube.Domain.Entities.Video;
using GoogleVideo = Google.Apis.YouTube.v3.Data.Video;

namespace AutoNotionTube.Core.Application.Features.UploadVideo;

public class UploadVideoCommandHandler : IRequestHandler<UploadVideoCommand, GoogleVideo?>
{
    private readonly ILogger<UploadVideoCommandHandler> _logger;
    private readonly IYoutubeService _youtubeService;
    private readonly IVideoRepository _videoRepository;

    public UploadVideoCommandHandler(ILogger<UploadVideoCommandHandler> logger, IYoutubeService youtubeService, IVideoRepository videoRepository)
    {
        _logger = logger;
        _youtubeService = youtubeService;
        _videoRepository = videoRepository;
    }

    public async Task<GoogleVideo?> Handle(UploadVideoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading video: {VideoFile}", request.VideoFile);

        GoogleVideo? videoResponse = null;
        int uploadAttempts = 0;

        do
        {
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

                await using var fileStream = new FileStream(request.VideoFile, FileMode.Open);

                var videosInsertRequest = youTubeService.Videos.Insert(
                    new GoogleVideo { Snippet = video.Snippet, Status = video.Status },
                    "snippet,status", fileStream, "video/*");
                videosInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += (videoData) =>
                {
                    VideosInsertRequest_ResponseReceived(videoData);
                    videoResponse = videoData;
                };

                await videosInsertRequest.UploadAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Attempt {Attempt} of 3 failed to upload video to YouTube", uploadAttempts + 1);

                if (uploadAttempts < 2) 
                {
                    await Task.Delay(TimeSpan.FromMinutes(3), cancellationToken);
                }
            }

            uploadAttempts++;
        }
        while (videoResponse == null && uploadAttempts < 3); // Retry up to 3 times

        if (videoResponse?.Id is null || videoResponse.Status?.UploadStatus is not "uploaded")
        {
            _logger.LogError("All attempts failed to upload video to YouTube");
            await _videoRepository.MoveFailedVideo(request.VideoFile);
        }

        return videoResponse;
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
