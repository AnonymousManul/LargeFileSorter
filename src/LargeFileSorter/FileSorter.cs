using LargeFileSorter.Options;

namespace LargeFileSorter;

public class FileSorter
{
    private readonly SorterOptions _sorterOptions;
    private readonly string _tmpFolder;

    public FileSorter(SorterOptions sorterOptions)
    {
        _sorterOptions = sorterOptions;
        if (!string.IsNullOrEmpty(sorterOptions.TempFolder))
        {
            if (!Directory.Exists(sorterOptions.TempFolder))
                Directory.CreateDirectory(sorterOptions.TempFolder);
            _tmpFolder = sorterOptions.TempFolder;
        }
        else
        {
            _tmpFolder = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tmpFolder);
        }
    }

    public async Task Sort(CancellationToken cancellationToken)
    {
        var helper = new FileHelper(_tmpFolder);
        var splitter = new FileSplitter(
            _sorterOptions.RowsLimitInChunk,
            _sorterOptions.ReadInputFileBufferSizeInMb,
            _sorterOptions.InputFileName,
            helper);

        var merger = new FileMerger(
            _sorterOptions.ChunksToMergePerRun,
            _sorterOptions.ResultFileName,
            helper,
            _tmpFolder,
            _sorterOptions.ReadBufferSizeInMb,
            _sorterOptions.WriteChunkedFileBufferSizeInMb);

        await splitter.SplitFileAndSortChunksAsync(cancellationToken);
        await merger.MergeFilesAsync(cancellationToken);
    }
}