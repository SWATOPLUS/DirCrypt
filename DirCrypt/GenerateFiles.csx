#!/usr/bin/env dotnet-script
using System;
using System.IO;
using System.Linq;

if (Args.Count() != 3)
{
    Console.WriteLine("Usage: dotnet script GenerateFile.csx <directory> <count> <size (kbs)>");
    return;
}

var directory = Args[0];
var count = int.Parse(Args[1]);
var size = int.Parse(Args[2]);

var random = new Random();
var buffer = new byte[size];

if(!Directory.Exists(directory))
{
    Directory.CreateDirectory(directory);
}

foreach(var fileId in Enumerable.Range(0, count))
{
    var fileName = $"file{fileId}.bin";
    var path = Path.Combine(directory, fileName);

    random.NextBytes(buffer);

    File.WriteAllBytes(path, buffer);
}
