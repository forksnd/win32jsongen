﻿// <copyright file="Program.cs" company="https://github.com/marlersoft">
// Copyright (c) https://github.com/marlersoft. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using JsonWin32Generator;

internal class Program
{
    private static int Main()
    {
        string repoDir = JsonWin32Common.FindWin32JsonRepo();
        string apiDir = JsonWin32Common.GetAndVerifyWin32JsonApiDir(repoDir);
        CleanDir(apiDir);
        string versionFile = Path.Combine(repoDir, "version.txt");
        File.Delete(versionFile);
        var generateTimer = Stopwatch.StartNew();
        {
            using FileStream metadataFileStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location!)!, "Windows.Win32.winmd"));
            using PEReader peReader = new PEReader(metadataFileStream);
            Console.WriteLine("OutputDirectory: {0}", apiDir);
            JsonGenerator.Generate(peReader.GetMetadataReader(), apiDir);
        }

        {
            using var fileStream = new FileStream(versionFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var streamWriter = new StreamWriter(fileStream, new UTF8Encoding(false));
            streamWriter.Write(MetadataVersion.Value);
        }

        Console.WriteLine("Generation time: {0}", generateTimer.Elapsed);
        return 0;
    }

    private static void CleanDir(string dir)
    {
        if (Directory.Exists(dir))
        {
            foreach (string file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }
        else
        {
            Directory.CreateDirectory(dir);
        }
    }
}
