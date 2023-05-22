using System.Reflection;

namespace AutoNotionTube.Worker;

public class GetApplicationRootDirectory
{
    public string GetAppRootDirectory()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyLocation = assembly.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        var appRootDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", "..", ".."));
        return appRootDirectory;
    }
    
    public string GetAppRootDirectory2()
    {
        string appRootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        return appRootDirectory;
    }
    
    public List<string> GetAllFiles(string directoryPath)
    {
        List<string> allFiles = new List<string>();

        if (Directory.Exists(directoryPath))
        {
            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            allFiles.AddRange(files);
        }

        return allFiles;
    }
    
    public List<string> FindFolders(string rootDirectoryPath, params string[] folderNames)
    {
        List<string> matchingFolders = new List<string>();

        if (Directory.Exists(rootDirectoryPath))
        {
            foreach (string folderName in folderNames)
            {
                string[] folders = Directory.GetDirectories(rootDirectoryPath, folderName, SearchOption.AllDirectories);
                matchingFolders.AddRange(folders);
            }
        }

        return matchingFolders;
    }
    
    public string FindFolder(string rootDirectoryPath, string folderName)
    {
        string matchingFolder = null;

        if (Directory.Exists(rootDirectoryPath))
        {
            string[] folders = Directory.GetDirectories(rootDirectoryPath, folderName, SearchOption.AllDirectories);
            if (folders.Length > 0)
            {
                matchingFolder = folders[0];
            }
        }

        return matchingFolder;
    }
}
