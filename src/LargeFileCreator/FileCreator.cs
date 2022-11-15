using System.Text;
using CrypticWizard.RandomWordGenerator;

namespace LargeFileCreator;

public class FileCreator
{
    private const long MIN_INDEX = 1;
    private const long MAX_INDEX = 100_000_000;

    private const int MIN_WORD_SIZE = 5;
    private const int MAX_WORD_SIZE = 50;

    private const int DUPLICATE_WORDS_COUNT = 100;

    private readonly FileOptions _options;
    private readonly WordGenerator _wordGenerator;

    private readonly HashSet<string> _duplicatedWords;

    public FileCreator(FileOptions options)
    {
        _options = options;
        _wordGenerator = new WordGenerator();
        _duplicatedWords = new HashSet<string>();
        PrefillDuplicatedWords();
    }

    public async Task CreateFileAsync()
    {
        var directory = Path.GetDirectoryName(_options.OutputFileName);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        
        await using var fileStream = File.Create(_options.OutputFileName);
        await using var sw = new StreamWriter(fileStream);
        for (var i = 0; i < _options.FileSizeInLines; ++i)
        {
            await sw.WriteLineAsync($"{GetIndex()}. {GetWord(i % 100 == 0)}");
        }
    }

    private static long GetIndex() => Random.Shared.NextInt64(MIN_INDEX, MAX_INDEX);

    private string GetWord(bool isDup) => isDup switch
    {
        true => _duplicatedWords.ElementAt(Random.Shared.Next(0, DUPLICATE_WORDS_COUNT - 1)),
        _ when _options.UseWordGenerator => _wordGenerator.GetWord(WordGenerator.PartOfSpeech.noun),
        _ => GetRandomString()
    };


    private static string GetRandomString()
    {
        var length = Random.Shared.Next(MIN_WORD_SIZE, MAX_WORD_SIZE);
        var sb = new StringBuilder(length);
        for (var i = 0; i < sb.Capacity; ++i)
            sb.Append(GetChar());
        return sb.ToString();
    }

    private static char GetChar() => (char)Random.Shared.Next('a', 'z');

    private void PrefillDuplicatedWords()
    {
        for (var i = 0; i < DUPLICATE_WORDS_COUNT; ++i)
            _duplicatedWords.Add(GetWord(false));
    }
}