using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;

namespace LargeFileSorter;

public class FileSplitter
{

    private static ReadOnlySpan<byte> NewLine => new[] { (byte)'\n' };
    
    private readonly int _rowsLimitInChunk;
    private readonly int _readInputFileBufferSizeInMb;
    private readonly string _inputFileName;
    private readonly FileHelper _fileHelper;
    
    public FileSplitter(
        int rowsLimitInChunk, 
        int readInputFileBufferSizeInMb, 
        string inputFileName, 
        FileHelper fileHelper)
    {
        _rowsLimitInChunk = rowsLimitInChunk;
        _readInputFileBufferSizeInMb = readInputFileBufferSizeInMb;
        _inputFileName = inputFileName;
        _fileHelper = fileHelper;
    }
    
    public async Task SplitFileAndSortChunksAsync(CancellationToken cancellationToken)
    {
        await using var fileStream = File.OpenRead(_inputFileName);
        var pipeReader =
            PipeReader.Create(fileStream,
                new StreamPipeReaderOptions(bufferSize: _readInputFileBufferSizeInMb * 1024 * 1024));

        var tasks = new List<Task>();
        var linesCounter = 0;
        var comparer = new Comparer();
        var chunk = new List<string>(_rowsLimitInChunk);

        while (true)
        {
            var result = await pipeReader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;

            string? tmpStr;
            while ((tmpStr = ReadLine(ref buffer)) is not null)
            {
                chunk.Add(tmpStr);
                linesCounter++;
                if (linesCounter != _rowsLimitInChunk) 
                    continue;
                var chunkToSort = chunk;
                tasks.Add(SortAndWriteChunkAsync(chunkToSort, comparer));
                linesCounter = 0;
                chunk = new List<string>(_rowsLimitInChunk);
            }

            pipeReader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted) break;
        }

        await pipeReader.CompleteAsync();
        if (linesCounter > 0)
        {
            tasks.Add(SortAndWriteChunkAsync(chunk, comparer));
        }

        await Task.WhenAll(tasks);
    }

    private async Task SortAndWriteChunkAsync(List<string> chunk, IComparer<string> comparer)
    {
        await Task.Yield();
        chunk.Sort(comparer);
        await File.WriteAllLinesAsync(_fileHelper.GetFullTempFileNamePath(), chunk);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? ReadLine(ref ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
        if (!reader.TryReadTo(out ReadOnlySpan<byte> line, NewLine))
            return default;
        buffer = buffer.Slice(reader.Position);
        return Encoding.UTF8.GetString(line);
    }

}