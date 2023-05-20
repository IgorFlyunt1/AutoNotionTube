using AutoNotionTube.Core.Application.Features.UploadVideo;
using AutoNotionTube.Core.Constants;
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

            var videoFiles = ScanForNewVideoFiles();

            if (videoFiles is {Count : 0})
            {
                _logger.LogInformation("No new video files found");
            }

            foreach (string videoFile in videoFiles)
            {
                _logger.LogInformation("Found video file: {VideoFile}", videoFile);
                
                await _mediator.Send(new UploadVideoCommand
                    {
                        VideoFile = videoFile,
                        VideoTitle = "Default Video Title",
                    }, 
                    stoppingToken);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private IReadOnlyCollection<string> ScanForNewVideoFiles()
    {
        var directories = new List<string>
        {
            VideoFilesDirectoryConstants.MeetingsDirectory, VideoFilesDirectoryConstants.TutorialsDirectory
        };

        var videoFiles = new List<string>();

        foreach (string directory in directories)
        {
            videoFiles.AddRange(Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly));
        }

        return videoFiles;
    }
}
