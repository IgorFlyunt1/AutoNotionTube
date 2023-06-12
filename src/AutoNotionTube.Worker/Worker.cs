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

            if (videoFiles is { Count : 0 })
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                continue;
            }

            await ProcessAllVideoFiles(videoFiles, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
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
        string captions = await GetCaptions(youtubeResponse, videoFile, stoppingToken);

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

    private async Task<string> GetCaptions(YoutubeResponse youtubeResponse, VideoFile videoFile,
        CancellationToken stoppingToken)
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

    private async Task<OpenApiResponse> GetOpenApiResponse(UploadVideoResponse uploadVideoResponse,
        CancellationToken stoppingToken)
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
}
