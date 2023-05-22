using AutoNotionTube.Core.Interfaces;
using AutoNotionTube.Domain.Entities;
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
                    Size = fileInfo.Length,
                    Directory = directory.GetLastPathComponent()
                };
                videoFiles.Add(videoFile);
            }
        }

        return Task.FromResult<IReadOnlyCollection<VideoFile>>(videoFiles);
    }
    
    
}

public static class StringExtensions
{
    public static string GetLastPathComponent(this string path)
    {
        char[] separators = { '\\', '/' };
        string[] parts = path.Split(separators);
        return parts[parts.Length - 1];
    }
}
