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

                await _mediator.Send(
                    new UploadVideoCommand { VideoFile = videoFile.FileName, VideoTitle = videoFile.Title, },
                    stoppingToken);
            }

            // Get Video Url and Iframe
            // If Video is Succwssfully Uploaded, Delete Video File
            // If Video is Not Successfully Uploaded, Retry Upload
            // If Video is Not Successfully Uploaded After 3 Tries, Send Telegram Message
            // Figure out how to get info that video was processed by YouTube to 100% and is ready to get transcript
            // Get Transcript
            // Upload Transcript ChatGPT
            //Upload iframe and result from ChatGPT to Notion

            await Task.Delay(1000, stoppingToken);
        }
    }

    // private IReadOnlyCollection<VideoFile> ScanForNewVideoFiles()
    // {
    //     var directories = new List<string>
    //     {
    //         _videoFilesDirectorySettings.MeetingsDirectory, _videoFilesDirectorySettings.TutorialsDirectory
    //     };
    //
    //     var root1 = new GetApplicationRootDirectory();
    //     string dir1 = root1.GetAppRootDirectory();
    //     string dir2 = root1.GetAppRootDirectory2();
    //
    //     //dir1 show all files in the directory and subdirectories
    //     //dir2 show all files in the directory and subdirectories
    //
    //     var sub1 = root1.GetAllFiles(dir1);
    //     var sub2 = root1.GetAllFiles(dir2);
    //
    //     var meetingFolderPath = root1.FindFolder(dir1, "MeetingVideos");
    //     var tutorialFolderPath = root1.FindFolder(dir2, "TutorialVideos");
    //
    //     var meetingFolderPath2 = root1.FindFolder(dir2, "MeetingVideos");
    //     var tutorialFolderPath2 = root1.FindFolder(dir1, "TutorialVideos");
    //
    //     var videoFiles = new List<VideoFile>();
    //
    //     foreach (string directory in directories)
    //     {
    //         var files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);
    //
    //         foreach (var file in files)
    //         {
    //             var title = Path.GetFileNameWithoutExtension(file);
    //             videoFiles.Add(new VideoFile { FileName = file, Title = title });
    //         }
    //     }
    //
    //     return videoFiles;
    // }
}
