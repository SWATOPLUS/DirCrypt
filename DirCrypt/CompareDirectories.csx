#!/usr/bin/env dotnet-script
using System;
using System.IO;
using System.Linq;

if (Args.Count() != 2)
{
    Console.WriteLine("Usage: dotnet script CompareDirectories.csx <directory1> <directory2>");
    return;
}

var directory1 = Args[0];
var directory2 = Args[1];

var files1 = Directory.GetFiles(directory1)
    .Select(x => Path.GetRelativePath(directory1, x))
    .ToArray();
var files2 = Directory.GetFiles(directory2)
    .Select(x => Path.GetRelativePath(directory2, x))
    .ToArray();

if (!files1.SequenceEqual(files2))
{
    Console.WriteLine("Different file names");

    var added = files2.Except(files1);
    var removed = files1.Except(files2);

    Console.WriteLine($"Added {string.Join(", ", added)}");
    Console.WriteLine($"Removed {string.Join(", ", removed)}");

    return;
}

var allOkFlag = true;

foreach (var file in files1)
{
    var file1 = File.ReadAllBytes(Path.Combine(directory1, file));
    var file2 = File.ReadAllBytes(Path.Combine(directory2, file));

    if (!file1.SequenceEqual(file2))
    {
        Console.WriteLine($"{file} are different");
        allOkFlag = false;
    }
}

if(allOkFlag)
{
    Console.WriteLine("All files are same");
}
