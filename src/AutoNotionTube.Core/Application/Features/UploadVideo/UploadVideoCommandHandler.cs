using System.Net;
using AutoNotionTube.Core.DTOs;
using AutoNotionTube.Core.Exceptions;
using AutoNotionTube.Core.Interfaces;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using GoogleVideo = Google.Apis.YouTube.v3.Data.Video;

namespace AutoNotionTube.Core.Application.Features.UploadVideo;
/// <summary>
/// This class 'UploadVideoCommandHandler' adheres to the principles of Object-Oriented Programming (OOP) by using classes, encapsulation and inheritance.
/// It implements the IRequestHandler interface and encapsulates the data and methods for handling the upload youtubeApiVideo command.
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
public class UploadVideoCommandHandler : IRequestHandler<UploadVideoCommand, YoutubeResponse>
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

    public async Task<YoutubeResponse> Handle(UploadVideoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading youtubeApiVideo: {VideoFile}", request.VideoFile);

        var uploadAttempts = 0;
        YoutubeResponse? videoResponse = null;

        while (videoResponse == null && uploadAttempts < 3)
        {
            videoResponse = await TryUploadVideo(request, uploadAttempts, cancellationToken);
            uploadAttempts++;
        }

        await HandleUploadOutcome(videoResponse, request.VideoFile);
        
        return videoResponse!;
    }

    private async Task<YoutubeResponse?> TryUploadVideo(UploadVideoCommand request, int uploadAttempts,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var fileStream = new FileStream(request.VideoFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            var youTubeService = await _youtubeService.GetService(cancellationToken);
            var video = CreateVideo(request.VideoTitle);
            VideosResource.InsertMediaUpload videosInsertRequest = CreateVideoInsertRequest(youTubeService, video, fileStream);

            return await UploadVideoAsync(videosInsertRequest, cancellationToken);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.Forbidden)
        {
            return await HandleFailedUpload(ex, uploadAttempts, cancellationToken) ? null : new YoutubeResponse();
        }
        catch (Exception ex)
        {
            return await HandleFailedUpload(ex, uploadAttempts, cancellationToken) ? null : new YoutubeResponse();
        }
    }

    private YoutubeApiVideo CreateVideo(string videoTitle)
    {
        return new YoutubeApiVideo
        {
            Snippet = new VideoSnippet { Title = videoTitle },
            Status = new VideoStatus
            {
                PrivacyStatus = "unlisted", // or  "private" or "public",
                MadeForKids = false
            }
        };
    }

    private VideosResource.InsertMediaUpload CreateVideoInsertRequest(YouTubeService? youTubeService, YoutubeApiVideo youtubeApiVideo,
        FileStream? fileStream)
    {
        var videosInsertRequest = youTubeService.Videos.Insert(
            new GoogleVideo { Snippet = youtubeApiVideo.Snippet, Status = youtubeApiVideo.Status },
            "snippet,status", fileStream, "video/*");

        videosInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
        videosInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;

        return videosInsertRequest;
    }

    private async Task<YoutubeResponse> UploadVideoAsync(VideosResource.InsertMediaUpload videosInsertRequest,
        CancellationToken cancellationToken)
    {
        GoogleVideo? videoResponse = null;

        videosInsertRequest.ResponseReceived += (videoData) =>
        {
            VideosInsertRequest_ResponseReceived(videoData);
            videoResponse = videoData;
        };

        await videosInsertRequest.UploadAsync(cancellationToken);

        return videoResponse?.Id is not null && videoResponse.Status?.UploadStatus == "uploaded"
            ? new YoutubeResponse
            {
                VideoId = videoResponse.Id,
                UploadStatus = videoResponse.Status.UploadStatus
            }
            : throw new YoutubeFailureToUploadVideoException("Failed to upload video to YouTube");
    }

    private async Task HandleUploadOutcome(YoutubeResponse? videoResponse, string videoFile)
    {
        if (videoResponse?.VideoId is null || videoResponse?.UploadStatus is not "uploaded")
        {
            _logger.LogError("All attempts failed to upload youtubeApiVideo to YouTube");
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
        _logger.LogError(progressException, "Failed to upload youtubeApiVideo to YouTube");

        if (uploadAttempts < 2)
        {
            if (progressException is Google.GoogleApiException ex && ex.HttpStatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("Quota exceeded for YouTube API. Waiting for 10 minutes before next attempt...");
                await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);
                return true;
            }

            _logger.LogWarning("Attempt {Attempt} of 3 failed to upload youtubeApiVideo to YouTube", uploadAttempts + 1);
            await Task.Delay(TimeSpan.FromMinutes(3), cancellationToken);
            return true;
        }

        return false;
    }

    private void VideosInsertRequest_ResponseReceived(GoogleVideo video)
    {
        _logger.LogInformation("YoutubeApiVideo was successfully uploaded");
    }
}
