using System.Net;
using AutoNotionTube.Core.Interfaces;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Video = AutoNotionTube.Domain.Entities.Video;
using GoogleVideo = Google.Apis.YouTube.v3.Data.Video;

namespace AutoNotionTube.Core.Application.Features.UploadVideo;
/// <summary>
/// This class 'UploadVideoCommandHandler' adheres to the principles of Object-Oriented Programming (OOP) by using classes, encapsulation and inheritance.
/// It implements the IRequestHandler interface and encapsulates the data and methods for handling the upload video command.
/// SOLID principles are also followed:
/// - Single Responsibility Principle (SRP): Each method has a single responsibility.
/// - Open-Closed Principle (OCP): The class is open for extension, but closed for modification.
/// - Liskov Substitution Principle (LSP): The class uses interfaces, ensuring it behaves as expected.
/// - Interface Segregation Principle (ISP): The class only depends on the parts of interfaces that it uses.
/// - Dependency Inversion Principle (DIP): The class depends on abstractions rather than concrete implementations, with dependencies injected via its constructor.
/// </summary>
/// /// <summary>
/// This class 'UploadVideoCommandHandler' adheres to the principles of Object-Oriented Programming (OOP).
/// It uses classes and objects (UploadVideoCommandHandler, UploadVideoCommand, GoogleVideo, etc.), 
/// encapsulates data and methods together (e.g., in UploadVideoCommandHandler), 
/// and uses inheritance (UploadVideoCommandHandler implements IRequestHandler).
/// SOLID principles are also adhered to:
/// - Single Responsibility Principle (SRP): Each method in the class has one responsibility.
/// - Open-Closed Principle (OCP): The solution is open for extension but closed for modification.
/// - Liskov Substitution Principle (LSP): The solution adheres to the LSP as it uses interfaces like IRequestHandler.
/// - Interface Segregation Principle (ISP): Clients are not forced to depend on interfaces they don't use.
/// - Dependency Inversion Principle (DIP): The solution depends on abstractions (IYoutubeService, IVideoRepository) 
/// rather than concrete implementations, with dependencies injected into the class via its constructor.
/// </summary>
public class UploadVideoCommandHandler : IRequestHandler<UploadVideoCommand, GoogleVideo?>
{
    private readonly ILogger<UploadVideoCommandHandler> _logger;
    private readonly IYoutubeService _youtubeService;
    private readonly IVideoRepository _videoRepository;

    public UploadVideoCommandHandler(ILogger<UploadVideoCommandHandler> logger, IYoutubeService youtubeService,
        IVideoRepository videoRepository)
    {
        _logger = logger;
        _youtubeService = youtubeService;
        _videoRepository = videoRepository;
    }

    public async Task<GoogleVideo?> Handle(UploadVideoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading video: {VideoFile}", request.VideoFile);

        var uploadAttempts = 0;
        GoogleVideo? videoResponse = null;

        while (videoResponse == null && uploadAttempts < 3)
        {
            videoResponse = await TryUploadVideo(request, uploadAttempts, cancellationToken);
            uploadAttempts++;
        }

        await HandleUploadOutcome(videoResponse, request.VideoFile);
        return videoResponse;
    }

    private async Task<GoogleVideo?> TryUploadVideo(UploadVideoCommand request, int uploadAttempts,
        CancellationToken cancellationToken)
    {
        try
        {
            var youTubeService = await _youtubeService.GetService(cancellationToken);
            var video = CreateVideo(request.VideoTitle);
            var fileStream = new FileStream(request.VideoFile, FileMode.Open);
            VideosResource.InsertMediaUpload videosInsertRequest = CreateVideoInsertRequest(youTubeService, video, fileStream);

            return await UploadVideoAsync(videosInsertRequest, cancellationToken);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.Forbidden)
        {
            return await HandleFailedUpload(ex, uploadAttempts, cancellationToken) ? null : new GoogleVideo();
        }
        catch (Exception ex)
        {
            return await HandleFailedUpload(ex, uploadAttempts, cancellationToken) ? null : new GoogleVideo();
        }
    }

    private Video CreateVideo(string videoTitle)
    {
        return new Video
        {
            Snippet = new VideoSnippet { Title = videoTitle },
            Status = new VideoStatus
            {
                PrivacyStatus = "unlisted", // or  "private" or "public",
                MadeForKids = false
            }
        };
    }

    private VideosResource.InsertMediaUpload CreateVideoInsertRequest(YouTubeService? youTubeService, Video video,
        FileStream fileStream)
    {
        var videosInsertRequest = youTubeService.Videos.Insert(
            new GoogleVideo { Snippet = video.Snippet, Status = video.Status },
            "snippet,status", fileStream, "video/*");

        videosInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
        videosInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;

        return videosInsertRequest;
    }

    private async Task<GoogleVideo?> UploadVideoAsync(VideosResource.InsertMediaUpload videosInsertRequest,
        CancellationToken cancellationToken)
    {
        GoogleVideo? videoResponse = null;

        videosInsertRequest.ResponseReceived += (videoData) =>
        {
            VideosInsertRequest_ResponseReceived(videoData);
            videoResponse = videoData;
        };

        await videosInsertRequest.UploadAsync(cancellationToken);

        return videoResponse;
    }

    private async Task HandleUploadOutcome(GoogleVideo? videoResponse, string videoFile)
    {
        if (videoResponse?.Id is null || videoResponse.Status?.UploadStatus is not "uploaded")
        {
            _logger.LogError("All attempts failed to upload video to YouTube");
            await _videoRepository.MoveFailedVideo(videoFile);
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

    private async Task<bool> HandleFailedUpload(Exception progressException, int uploadAttempts,
        CancellationToken cancellationToken)
    {
        _logger.LogError(progressException, "Failed to upload video to YouTube");

        if (uploadAttempts < 2)
        {
            if (progressException is Google.GoogleApiException ex && ex.HttpStatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("Quota exceeded for YouTube API. Waiting for 10 minutes before next attempt...");
                await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);
                return true;
            }

            _logger.LogWarning("Attempt {Attempt} of 3 failed to upload video to YouTube", uploadAttempts + 1);
            await Task.Delay(TimeSpan.FromMinutes(3), cancellationToken);
            return true;
        }

        return false;
    }

    private void VideosInsertRequest_ResponseReceived(GoogleVideo video)
    {
        _logger.LogInformation("Video was successfully uploaded");
    }
}
