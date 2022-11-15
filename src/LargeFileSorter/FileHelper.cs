namespace LargeFileSorter;

public class FileHelper
{
    // ReSharper disable once InconsistentNaming
    private const string TEMP_FILE_EXTENSION = ".tmpf";

    private readonly string _tmpFolder;

    public FileHelper(string tmpFolder)
    {
        _tmpFolder = tmpFolder;
    }

    public string[] GetFileNames() => Directory.GetFiles(_tmpFolder, $"*{TEMP_FILE_EXTENSION}");

    public string GetFullTempFileNamePath(string fileName) => Path.Combine(_tmpFolder, fileName);
    
    public string GetFullTempFileNamePath() => GetFullTempFileNamePath($"{Guid.NewGuid()}{TEMP_FILE_EXTENSION}");
}