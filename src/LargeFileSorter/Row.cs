namespace LargeFileSorter;

public readonly struct Row
{
    public string Line { get; init; }
    public int StreamReaderIndex { get; init; }

}