using BenchmarkDotNet.Attributes;

namespace LargeFileSorter.Benchmarks;

[MemoryDiagnoser(true)]
public class Benchmarks
{

    private FileSplitter _fileSplitter;
    private FileMerger _fileMerger;
    
    [GlobalSetup]
    public void Setup()
    {
        
        var tmpFolder = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpFolder);
        
        var helper = new FileHelper(tmpFolder);
        
        _fileSplitter = new FileSplitter(
            500_000,
            16,
            "/Users/asuzdalcev/trash/10m.txt",
            helper);

        _fileMerger = new FileMerger(
            4,
            "/Users/asuzdalcev/trash/sorted_10m.txt",
            helper,
            tmpFolder,
            1,
            1);

    }

    [Benchmark]
    public async Task SortFileAsync()
    {
        await _fileSplitter.SplitFileAndSortChunksAsync(CancellationToken.None);
        await _fileMerger.MergeFilesAsync(CancellationToken.None);
    }
}