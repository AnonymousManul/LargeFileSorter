namespace LargeFileSorter;

public class FileMerger
{
    private readonly int _chunksToMergePerRun;
    private readonly string _resultFileName;
    private readonly FileHelper _fileHelper;
    private readonly string _tmpFolder;
    private readonly int _readBufferSizeInMb;
    private readonly int _writeChunkedFileBufferSizeInMb;

    public FileMerger(
        int chunksToMergePerRun,
        string resultFileName,
        FileHelper fileHelper,
        string tmpFolder,
        int readBufferSizeInMb,
        int writeChunkedFileBufferSizeInMb)
    {
        _chunksToMergePerRun = chunksToMergePerRun;
        _resultFileName = resultFileName;
        _fileHelper = fileHelper;
        _tmpFolder = tmpFolder;
        _readBufferSizeInMb = readBufferSizeInMb;
        _writeChunkedFileBufferSizeInMb = writeChunkedFileBufferSizeInMb;
    }
    
    public async Task MergeFilesAsync(CancellationToken cancellationToken)
    {
        var chunks = _fileHelper.GetFileNames()
            .Chunk(_chunksToMergePerRun)
            .ToList();

        while (chunks.Any())
        {
            await Task.WhenAll(
                chunks.Where(chunk => chunk.Length > 1)
                    .Select(chunk => Merge(chunk, cancellationToken)));

            var files = _fileHelper.GetFileNames();
            if (files.Length == 1)
            {
                MoveResult(files[0], _resultFileName);
                return;
            }

            chunks = files.Chunk(_chunksToMergePerRun).ToList();
        }
    }

    private async Task Merge(
        IReadOnlyList<string> filesToMerge,
        CancellationToken cancellationToken)
    {
        await using var outputStream = File.OpenWrite(_fileHelper.GetFullTempFileNamePath());
        var (streamReaders, rows) = await CreateReaders(filesToMerge);
        var finishedStreamReaders = new List<int>(streamReaders.Length);
        var done = false;
        await using var outputWriter = new StreamWriter(outputStream,
            bufferSize: _writeChunkedFileBufferSizeInMb * 1024 * 1024);
        var comparer = new Comparer();
        while (!done)
        {
            var (row, index) = GetMinRow(rows, comparer);
            await outputWriter.WriteLineAsync(row.Line.AsMemory(), cancellationToken);

            if (streamReaders[row.StreamReaderIndex].EndOfStream)
            {
                var indexToRemove = rows.FindIndex(x => x.StreamReaderIndex == row.StreamReaderIndex);
                rows.RemoveAt(indexToRemove);
                finishedStreamReaders.Add(row.StreamReaderIndex);
                done = finishedStreamReaders.Count == streamReaders.Length;
                continue;
            }

            var value = await streamReaders[row.StreamReaderIndex].ReadLineAsync();
            rows[index] = new Row { Line = value!, StreamReaderIndex = row.StreamReaderIndex };
        }

        CloseReadersAndDeleteFiles(streamReaders, filesToMerge);
    }

    private async Task<(StreamReader[] StreamReaders, List<Row> rows)> CreateReaders(IReadOnlyList<string> sortedFiles)
    {
        var streamReaders = new StreamReader[sortedFiles.Count];
        var rows = new List<Row>(sortedFiles.Count);
        for (var i = 0; i < sortedFiles.Count; i++)
        {
            var sortedFilePath = _fileHelper.GetFullTempFileNamePath(sortedFiles[i]);
            var sortedFileStream = File.OpenRead(sortedFilePath);
            streamReaders[i] = new StreamReader(sortedFileStream, bufferSize: _readBufferSizeInMb * 1024 * 1024);
            var row = new Row
            {
                Line = (await streamReaders[i].ReadLineAsync())!,
                StreamReaderIndex = i
            };
            rows.Add(row);
        }

        return (streamReaders, rows);
    }

    private void MoveResult(string mergedFile, string resultFile)
    {
        var directoryName = Path.GetDirectoryName(Path.GetFullPath(resultFile));
        if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            Directory.CreateDirectory(directoryName);

        File.Move(mergedFile, resultFile, true);
        Directory.Delete(_tmpFolder);
    }
    
    private static void CloseReadersAndDeleteFiles(IReadOnlyList<StreamReader> streamReaders, IReadOnlyList<string> filesToMerge)
    {
        for (var i = 0; i < streamReaders.Count; i++)
        {
            streamReaders[i].Close();
            File.Delete(filesToMerge[i]);
        }
    }

    private static (Row row, int index) GetMinRow(IReadOnlyList<Row> rows, IComparer<string> comparer)
    {
        var minRow = rows[0];
        var minIdx = 0;
        for (var i = 1; i < rows.Count; ++i)
        {
            if (comparer.Compare(minRow.Line, rows[i].Line) >= 0) 
                continue;
            minIdx = i;
            minRow = rows[i];
        }

        return (minRow, minIdx);
    }
    
}