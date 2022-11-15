using CommandLine;

namespace LargeFileSorter.Options;

// ReSharper disable once ClassNeverInstantiated.Global
public class SorterOptions
{
    [Option('i', "inFileName", Required = true, HelpText = "Input file name")]
    public string InputFileName { get; set; } = null!;

    [Option('b', "readBufferSize", Required = false, Default = 16, HelpText = "Read buffer size in MB")]
    public int ReadInputFileBufferSizeInMb { get; set; }

    [Option('r', "rowsLimitInChunk", Required = false, Default = 500_000, HelpText = "Maximum lines in one chunk")]
    public int RowsLimitInChunk { get; set; }

    [Option('c', "chunkedFileReadBufferSizeInMb", Required = false, Default = 1,
        HelpText = "Chunked file read buffer size")]
    public int ReadBufferSizeInMb { get; set; }


    [Option('w', "chunkedFileWriteBufferSize", Required = false, Default = 1,
        HelpText = "Chunked file read buffer size")]
    public int WriteChunkedFileBufferSizeInMb { get; set; }

    [Option('o', "outFileName", Required = true, HelpText = "Output file name")]
    public string ResultFileName { get; set; } = null!;

    [Option('m', "mergeChunksCount", Required = false, Default = 4, HelpText = "Count of chunks to merge per run")]
    public int ChunksToMergePerRun { get; set; }

    [Option('t', "tempFolder", Required = false, Default = null, HelpText = "Temp folder for chunks")]
    public string? TempFolder { get; set; }
}