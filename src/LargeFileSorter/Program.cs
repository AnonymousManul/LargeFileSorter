using System.Diagnostics;
using CommandLine;
using LargeFileSorter;
using LargeFileSorter.Options;


var ro = Parser.Default.ParseArguments<SorterOptions>(args).Value;
var sorter = new FileSorter(ro);
var sw = Stopwatch.StartNew();
await sorter.Sort(CancellationToken.None);
Console.WriteLine($"File {ro.InputFileName} was sorted in {sw.Elapsed}. Result: {ro.ResultFileName}");