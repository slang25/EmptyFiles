﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EmptyFiles;
using Xunit;
using Xunit.Abstractions;

public class Tests :
    XunitContextBase
{
    [Fact]
    public void Unknown_extension()
    {
        Assert.Throws<Exception>(() => AllFiles.GetPathFor("txt"));
        Assert.False(AllFiles.TryGetPathFor("txt", out var result));
        Assert.Null(result);
        Assert.False(AllFiles.TryGetPathFor(".txt", out result));
        Assert.Null(result);
        Assert.False(AllFiles.TryCreateFile("foo.txt"));
        Assert.Null(result);
        Assert.Throws<Exception>(() => AllFiles.GetPathFor(".txt"));
        Assert.Throws<Exception>(() => AllFiles.CreateFile("foo.txt"));
    }

    [Fact]
    public void GetPathFor()
    {
        #region GetPathFor
        var path = AllFiles.GetPathFor("jpg");
        #endregion
        var path2 = AllFiles.GetPathFor(".jpg");
        Assert.NotNull(path);
        Assert.NotNull(path2);
        Assert.True(File.Exists(path));
        Assert.True(File.Exists(path2));
    }

    [Fact]
    public void CreateFile()
    {
        var pathOfFileToCreate = "file.jpg";
        File.Delete(pathOfFileToCreate);
        #region CreateFile
        AllFiles.CreateFile(pathOfFileToCreate);
        #endregion
        Assert.True(File.Exists(pathOfFileToCreate));
        File.Delete(pathOfFileToCreate);

        AllFiles.CreateFile("foo.txt", true);
        Assert.True(File.Exists("foo.txt"));
        File.Delete("foo.txt");

        Assert.True(AllFiles.TryCreateFile(pathOfFileToCreate));
        Assert.True(File.Exists(pathOfFileToCreate));
        File.Delete(pathOfFileToCreate);

        Assert.False(AllFiles.TryCreateFile("foo.txt"));
        Assert.False(File.Exists("foo.txt"));
        File.Delete("foo.txt");

        Assert.True(AllFiles.TryCreateFile("foo.txt", true));
        Assert.True(File.Exists("foo.txt"));
        File.Delete("foo.txt");
    }

    [Fact]
    public void IsEmptyFile()
    {
        #region IsEmptyFile
        var path = AllFiles.GetPathFor("jpg");
        Assert.True(AllFiles.IsEmptyFile(path));
        var temp = Path.GetTempFileName();
        Assert.False(AllFiles.IsEmptyFile(temp));
        #endregion

        var path2 = AllFiles.GetPathFor(".jpg");
        Assert.True(AllFiles.IsEmptyFile(path2));
        File.Delete(temp);
    }

    [Fact]
    public void Aliases()
    {
        var path = AllFiles.GetPathFor("jpeg");
        Assert.True(AllFiles.IsEmptyFile(path));

        Assert.Contains("jpeg", AllFiles.ImageExtensions);
    }

    [Fact]
    public void AllPaths()
    {
        Assert.NotEmpty(AllFiles.AllPaths);
        #region AllPaths
        foreach (var path in AllFiles.AllPaths)
        {
            Trace.WriteLine(path);
        }
        #endregion
    }

    [Fact]
    public void UseFile()
    {
        var pathToFile = SourceFile;
        #region UseFile
        AllFiles.UseFile(Category.Document, pathToFile);
        Assert.Contains(pathToFile, AllFiles.DocumentPaths);
        #endregion
    }

    [Fact]
    public async Task WriteExtensions()
    {
        var md = Path.Combine(SourceDirectory, "extensions.include.md");
        File.Delete(md);
        await using var writer = File.CreateText(md);
        await WriteCategory(writer, "Archive", AllFiles.Archives);
        await WriteCategory(writer, "Document", AllFiles.Documents);
        await WriteCategory(writer, "Image", AllFiles.Images);
        await WriteCategory(writer, "Sheet", AllFiles.Sheets);
        await WriteCategory(writer, "Slide", AllFiles.Slides);
    }

    static async Task WriteCategory(StreamWriter writer, string category, IReadOnlyDictionary<string, EmptyFile> files)
    {
        await writer.WriteLineAsync("");
        await writer.WriteLineAsync($"### {category}");
        await writer.WriteLineAsync("");
        foreach (var file in files.OrderBy(x=>x.Key))
        {
            var size = Size.Suffix(new FileInfo(file.Value.Path).Length);
            await writer.WriteLineAsync($"  * {file.Key} ({size})");
        }
    }

    public Tests(ITestOutputHelper output) :
        base(output)
    {
    }
}