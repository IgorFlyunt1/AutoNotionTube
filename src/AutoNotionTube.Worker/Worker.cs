using System.Text.RegularExpressions;
using AutoNotionTube.Core.Application.Features.DeleteVideo;
using AutoNotionTube.Core.Application.Features.GetCaptions;
using AutoNotionTube.Core.Application.Features.GetVideos;
using AutoNotionTube.Core.Application.Features.UploadVideo;
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

            IReadOnlyCollection<VideoFile> videoFiles = await _mediator.Send(new GetVideosQuery(), stoppingToken);

            if (videoFiles is { Count : 0 })
            {
                _logger.LogInformation("No new video files found");
            }

            foreach (var videoFile in videoFiles)
            {
                _logger.LogInformation("Found video file: {VideoFile}", videoFile.FileName);

                var uploadedResponse = await _mediator.Send(
                    new UploadVideoCommand { VideoFile = videoFile.FileName, VideoTitle = videoFile.Title, },
                    stoppingToken);

                if (uploadedResponse?.Id is not null && uploadedResponse.Status?.UploadStatus == "uploaded")
                {
                    await _mediator.Send(new DeleteVideoCommand { VideoFile = videoFile.FileName, }, stoppingToken);

                    var captions =
                        await _mediator.Send(
                            new GetCaptionsQuery
                            {
                                VideoId = uploadedResponse.Id,
                                Seconds = videoFile.Seconds,
                                SizeMb = videoFile.SizeMb,
                            }, stoppingToken);

                    if (!string.IsNullOrWhiteSpace(captions))
                    {
                       _logger.LogInformation(captions);
                    }
                }
            }

            // Upload Transcript ChatGPT
            //Upload iframe and result from ChatGPT to Notion

            await Task.Delay(1000, stoppingToken);
        }
    }
}
