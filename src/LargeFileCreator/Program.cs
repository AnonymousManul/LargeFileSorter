using CommandLine;
using LargeFileCreator;
using FileOptions = LargeFileCreator.FileOptions;

var options = Parser.Default.ParseArguments<FileOptions>(args).Value;
var fc = new FileCreator(options);
Console.WriteLine("Creating file..");
await fc.CreateFileAsync();
Console.WriteLine("File was created.");