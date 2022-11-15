namespace LargeFileSorter;

public class Comparer : IComparer<string>
{
    private readonly char _separator;

    public Comparer(char separator = '.')
    {
        _separator = separator;
    }

    public int Compare(string? x, string? y)
    {
        var xSpan = x.AsSpan();
        var ySpan = y.AsSpan();
        
        var valX = xSpan[(xSpan.IndexOf(_separator) + 2)..];
        var valY = ySpan[(ySpan.IndexOf(_separator) + 2)..];
        var valRes = valX.CompareTo(valY, StringComparison.OrdinalIgnoreCase);
        return valRes != 0 ? valRes : long.Parse(xSpan[..xSpan.IndexOf(_separator)]).CompareTo(long.Parse(ySpan[..ySpan.IndexOf(_separator)]));
    }
}