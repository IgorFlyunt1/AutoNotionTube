using System.Text.RegularExpressions;
using AutoNotionTube.Core.Application.Features.CreateNotionNote;
using AutoNotionTube.Core.Application.Features.DeleteVideo;
using AutoNotionTube.Core.Application.Features.GetCaptions;
using AutoNotionTube.Core.Application.Features.GetGPTSummarize;
using AutoNotionTube.Core.Application.Features.GetVideos;
using AutoNotionTube.Core.Application.Features.UploadVideo;
using AutoNotionTube.Core.DTOs;
using AutoNotionTube.Core.Extensions;
using AutoNotionTube.Domain.Entities;
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
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

            await Test(stoppingToken);

            // IReadOnlyCollection<VideoFile> videoFiles = await _mediator.Send(new GetVideosQuery(), stoppingToken);
            //
            // if (videoFiles is { Count : 0 })
            // {
            //     _logger.LogInformation("No new video files found");
            // }
            //
            // foreach (var videoFile in videoFiles)
            // {
            //     _logger.LogInformation("Found video file: {VideoFile}", videoFile.FileName);
            //
            //     var youtubeResponse = await _mediator.Send(
            //         new UploadVideoCommand { VideoFile = videoFile.FileName, VideoTitle = videoFile.Title, },
            //         stoppingToken);
            //
            //     if (youtubeResponse?.Id is not null && youtubeResponse.Status?.UploadStatus == "uploaded")
            //     {
            //         await _mediator.Send(new DeleteVideoCommand { VideoFile = videoFile.FileName, }, stoppingToken);
            //
            //         var captions =
            //             await _mediator.Send(
            //                 new GetCaptionsQuery
            //                 {
            //                     VideoId = youtubeResponse.Id,
            //                     Seconds = videoFile.Seconds,
            //                     SizeMb = videoFile.SizeMb,
            //                 }, stoppingToken);
            //
            //         if (!string.IsNullOrWhiteSpace(captions))
            //         {
            //             var openApiResponse = await _mediator.Send(
            //                 new GetOpenApiResponseQuery { Captions = captions, VideoTitle = "Azure AppService Availability Zone" },
            //                 stoppingToken);
            //
            //             NotionNoteRequest note = new NotionNoteRequest
            //             {
            //                 Title = videoFile.Title,
            //                 Tags = openApiResponse.Tags,
            //                 Description =
            //                     $"{openApiResponse.ShortSummary} \\n {openApiResponse.Summary} \\n {openApiResponse.Steps}",
            //                 DatabaseId = videoFile.DatabaseId,
            //                 Embed = youtubeResponse.Id.GetVideoEmbedIframe()
            //             };
            //         }
            //     }
            // }
            //
            // await Task.Delay(1000, stoppingToken);
        }
        //Test max quota reached for youtube api and wait 10 minutes
        //Test error when move the same video to failed folder add to video name current date time

        // Upload Transcript ChatGPT
        //Upload iframe and result from ChatGPT to Notion
    }

    private async Task Test(CancellationToken stoppingToken)
    {
        NotionNoteRequest noteRequest = new NotionNoteRequest
        {
            Title = "Azure AppService Availability Zone",
            Steps = "Steps",
            ShortSummary = "ShortSummary",
            Summary = "Summary",
            Tags = new List<string> { "Azure", "AppService", "Availability", "Zone" },
            DatabaseId = "DatabaseId",
            Embed = "Embed"
        };
            
        var notionResponse = await _mediator.Send(
            new CreateNotionNoteCommand { NotionNoteRequest = noteRequest, }, stoppingToken);
        
        // var captions =
        //     await _mediator.Send(
        //         new GetCaptionsQuery { VideoId = "E26Vqpr-ASc", Seconds = 1, SizeMb = 0.1, }, stoppingToken);
        //
        // if (!string.IsNullOrWhiteSpace(captions))
        // {
        //     var openApiResponse = await _mediator.Send(
        //         new GetOpenApiResponseQuery { Captions = captions, VideoTitle = "Azure AppService Availability Zone" },
        //         stoppingToken);
        //
        //     NotionNoteRequest noteRequest = new NotionNoteRequest
        //     {
        //         Title = "Azure AppService Availability Zone",
        //         Description = "Description",
        //         Tags = openApiResponse.Tags,
        //         DatabaseId = "DatabaseId",
        //         Embed = "Embed"
        //     };
        //     
        //     var notionResponse = await _mediator.Send(
        //         new CreateNotionNoteCommand { NotionNoteRequest = noteRequest, }, stoppingToken);
        // }
    }
}
