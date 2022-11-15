using CommandLine;

namespace LargeFileCreator;

public class FileOptions
{
    [Option('l', "lines", Required = true, HelpText = "Count of lines in file")]
    public long FileSizeInLines { get; set; }
    
    [Option('o', "outFileName", Required = true, HelpText = "Output file name")]
    public string OutputFileName { get; set; } = null!;
    
    [Option('w', "words", Required = false, Default = false,
        HelpText = "Use word generator instead of random strings")]
    public bool UseWordGenerator { get; set; }
}