using AutoNotionTube.Core.Application.Features.CreateNotionNote;
using AutoNotionTube.Core.Application.Features.DeleteVideo;
using AutoNotionTube.Core.Application.Features.GetCaptions;
using AutoNotionTube.Core.Application.Features.GetGPTSummarize;
using AutoNotionTube.Core.Application.Features.GetVideos;
using AutoNotionTube.Core.Application.Features.UploadVideo;
using AutoNotionTube.Core.DTOs;
using AutoNotionTube.Core.Extensions;
using MediatR;

namespace AutoNotionTube.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMediator _mediator;

    public Worker(ILogger<Worker> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var videoFiles = await GetVideoFiles(stoppingToken);

            if (videoFiles is { Count : 0 }) continue;

            await ProcessAllVideoFiles(videoFiles, stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task<IReadOnlyCollection<VideoFile>> GetVideoFiles(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting worker to get video files at: {Time}", DateTimeOffset.Now);

        var videoFiles = await _mediator.Send(new GetVideosQuery(), stoppingToken);

        if (videoFiles is { Count : 0 })
        {
            _logger.LogInformation("No new video files found");
        }

        return videoFiles;
    }

    private async Task ProcessAllVideoFiles(IEnumerable<VideoFile> videoFiles, CancellationToken stoppingToken)
    {
        foreach (var videoFile in videoFiles)
        {
            var uploadVideoResponse = await ProcessVideoFile(videoFile, stoppingToken);
            await CreateAndSaveNotionNote(uploadVideoResponse, stoppingToken);
        }
    }

    private async Task<UploadVideoResponse> ProcessVideoFile(VideoFile videoFile, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Found video file: {VideoFile}", videoFile.FileName);

        var youtubeResponse = await UploadVideo(videoFile, stoppingToken);
        await DeleteVideo(videoFile, stoppingToken);
        var captions = await GetCaptions(youtubeResponse, videoFile, stoppingToken);

        return new UploadVideoResponse
        {
            VideoFile = videoFile, YoutubeVideoId = youtubeResponse.VideoId, YoutubeVideoCaption = captions,
        };
    }

    private async Task<YoutubeResponse> UploadVideo(VideoFile videoFile, CancellationToken stoppingToken)
    {
        return await _mediator.Send(
            new UploadVideoCommand { VideoFile = videoFile.FileName, VideoTitle = videoFile.Title, },
            stoppingToken);
    }

    private async Task DeleteVideo(VideoFile videoFile, CancellationToken stoppingToken)
    {
        await _mediator.Send(new DeleteVideoCommand { VideoFile = videoFile.FileName, }, stoppingToken);
    }

    private async Task<string> GetCaptions(YoutubeResponse youtubeResponse, VideoFile videoFile, CancellationToken stoppingToken)
    {
        return await _mediator.Send(
            new GetCaptionsQuery
            {
                VideoId = youtubeResponse.VideoId, Seconds = videoFile.Seconds, SizeMb = videoFile.SizeMb,
            }, stoppingToken);
    }

    private async Task CreateAndSaveNotionNote(UploadVideoResponse uploadVideoResponse, CancellationToken stoppingToken)
    {
        var openApiSummarize = await GetOpenApiResponse(uploadVideoResponse, stoppingToken);

        await CreateNotionNote(uploadVideoResponse, openApiSummarize, stoppingToken);
    }

    private async Task<OpenApiResponse> GetOpenApiResponse(UploadVideoResponse uploadVideoResponse, CancellationToken stoppingToken)
    {
        return await _mediator.Send(
            new GetOpenApiResponseQuery { Captions = uploadVideoResponse.YoutubeVideoCaption },
            stoppingToken);
    }

    private async Task CreateNotionNote(UploadVideoResponse uploadVideoResponse, OpenApiResponse openApiResponse,
        CancellationToken stoppingToken)
    {
        NotionNoteRequest note = new()
        {
            Title = uploadVideoResponse.VideoFile.Title,
            Tags = openApiResponse.Tags,
            ShortSummary = openApiResponse.ShortSummary,
            Steps = openApiResponse.Steps,
            Summary = openApiResponse.Summary,
            IframeVideo = uploadVideoResponse.YoutubeVideoId.GetVideoYoutubeUrl()
        };

        await _mediator.Send(
            new CreateNotionNoteCommand { NotionNoteRequest = note },
            stoppingToken);
    }


    //Test max quota reached for youtube api and wait 10 minutes
    //Test error when move the same video to failed folder add to video name current date time

    // Upload Transcript ChatGPT
    //Upload iframe and result from ChatGPT to Notion

    private async Task Test(CancellationToken stoppingToken)
    {
        NotionNoteRequest noteRequest = new NotionNoteRequest
        {
            Title = "AutoNotionTube test" + DateTime.Now,
            Steps = "Steps",
            ShortSummary = "ShortSummary",
            Summary = "Summary",
            Tags = new List<string> { "Azure", "AppService", "Availability", "Zone" },
            DatabaseId = "f6cd2e1b3c6a445f8eb136e03130ef6d",
            IframeVideo = "fD5rUFNqKZc".GetVideoYoutubeUrl()
        };

        var notionResponse = await _mediator.Send(
            new CreateNotionNoteCommand { NotionNoteRequest = noteRequest, }, stoppingToken);

        // var captions =
        //     await _mediator.Send(
        //         new GetCaptionsQuery { VideoId = "fD5rUFNqKZc", Seconds = 1, SizeMb = 0.1, }, stoppingToken);
        //
        // if (!string.IsNullOrWhiteSpace(captions))
        // {
        //     var openApiResponse = await _mediator.Send(
        //         new GetOpenApiResponseQuery { Captions = captions, VideoTitle = "Azure AppService Availability Zone" },
        //         stoppingToken);
        //
        //     NotionNoteRequest noteRequest = new()
        //     {
        //         Title = "Azure AppService Availability Zone" + DateTime.Now,
        //         ShortSummary = openApiResponse.ShortSummary,
        //         Steps = openApiResponse.Steps,
        //         Summary = openApiResponse.Summary,
        //         Tags = openApiResponse.Tags,
        //         DatabaseId = "f6cd2e1b3c6a445f8eb136e03130ef6d",
        //         IframeVideo = "fD5rUFNqKZc".GetVideoEmbedIframe()
        //     };
        //
        //     var notionResponse = await _mediator.Send(
        //         new CreateNotionNoteCommand { NotionNoteRequest = noteRequest, }, stoppingToken);
        // }
    }
}
