using AutoNotionTube.Core.Interfaces;
using AutoNotionTube.Domain.Entities;
using AutoNotionTube.Infrastructure.Extensions;
using AutoNotionTube.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace AutoNotionTube.Infrastructure.Repositories;

public class LocalVideoRepository : IVideoRepository
{
    private readonly VideoFilesDirectorySettings _videoFilesDirectorySettings;

    public LocalVideoRepository(IOptions<VideoFilesDirectorySettings> videoFilesDirectorySettings)
    {
        _videoFilesDirectorySettings = videoFilesDirectorySettings.Value;
    }

    public Task<IReadOnlyCollection<VideoFile>> GetVideosAsync()
    {
        var directories = new List<string>
        {
            _videoFilesDirectorySettings.MeetingsDirectory, _videoFilesDirectorySettings.TutorialsDirectory
        };

        var videoFiles = new List<VideoFile>();

        foreach (var directory in directories)
        {
            string[] files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var videoFile = new VideoFile
                {
                    FileName = fileInfo.FullName,
                    Title = fileInfo.Name.Replace(fileInfo.Extension, string.Empty),
                    SizeMb = fileInfo.Length / 1024.0 / 1024.0,
                    Seconds = fileInfo.FullName.GetVideoDurationInSeconds(),
                    Directory = directory.GetLastPathComponent()
                };
                videoFiles.Add(videoFile);
            }
        }

        return Task.FromResult<IReadOnlyCollection<VideoFile>>(videoFiles);
    }
    
    public Task MoveFailedVideo(string videoFilePath)
    {
        var failedVideosDirectory = _videoFilesDirectorySettings.FailedVideosDirectory;
        var fileInfo = new FileInfo(videoFilePath);
        var dateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmm");
        var newFileName = $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}_{dateTimeNow}{fileInfo.Extension}";
        var destinationFilePath = Path.Combine(failedVideosDirectory, newFileName);
        File.Move(videoFilePath, destinationFilePath);
        return Task.CompletedTask;
    }
}