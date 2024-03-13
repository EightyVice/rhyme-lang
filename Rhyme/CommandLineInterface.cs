﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhyme
{
    internal record CommandLineValues(FileInfo[] SourceFiles, FileInfo OutputFile);

    internal static class CommandLineInterface
    {
        public static CommandLineValues GetParametersFromArguments(string[] args)
        {
            FileInfo[] source_files = null;
            FileInfo output_executable = null;

            var outputExecutableOption = new Option<FileInfo>(
                aliases: new[] { "-o", "--output" },
                getDefaultValue: () => new FileInfo("program.exe"),
                description: "Output executable file"
            );

            var sourceFilesArgument = new Argument<FileInfo[]>("files", "Input source files")
            {
                Arity = ArgumentArity.OneOrMore,
            };

            var rootCommand = new RootCommand
            {
                sourceFilesArgument,
                outputExecutableOption,
            };

            rootCommand.SetHandler((InvocationContext ctx) =>
            {
                var results = ctx.ParseResult;
                source_files = results.GetValueForArgument(sourceFilesArgument);
                output_executable = results.GetValueForOption(outputExecutableOption);
            });

            rootCommand.Invoke(args);

            return new CommandLineValues(source_files, output_executable);
        }
    }
}
