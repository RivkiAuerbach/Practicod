
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        var languageOption = new Option<string>("--language", "language option");
        var outputOption = new Option<FileInfo>("--output", "output option");
        var noteOption = new Option<bool>("--note", "note option");
        var sortOption = new Option<string>("--sort", "sort option");
        var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "remove-empty-lines option");
        var authorOption = new Option<string>("--author", "author option");
        var createRspCommand = new Command("create-rsp", "Create response file");

        languageOption.AddAlias("-l");
        outputOption.AddAlias("-o");
        noteOption.AddAlias("-n");
        sortOption.AddAlias("-s");
        removeEmptyLinesOption.AddAlias("-re");
        authorOption.AddAlias("-a");

        createRspCommand.SetHandler(() =>
        {
            Console.Write("Enter value for language: ");
            var languageValue = Console.ReadLine();

            Console.Write("Enter value for output: ");
            var outputValue = Console.ReadLine();

            Console.Write("Enter value for note (true/false): ");
            var noteValue = Console.ReadLine();
            while (!(noteValue == "true" || noteValue == "false"))
            {
                Console.Write("Enter again value for note (true/false): ");
                noteValue = Console.ReadLine();
            }
            bool.TryParse(noteValue, out bool note);

            Console.Write("Enter value for sort (filetype/alfabetic): ");
            var sortValue = Console.ReadLine();

            Console.Write("Enter value for remove-empty-lines (true/false): ");
            var removeEmptyLinesValue = Console.ReadLine();
            while (!(removeEmptyLinesValue == "true" || removeEmptyLinesValue == "false"))
            {
                Console.Write("Enter again value for remove-empty-lines (true/false): ");
                removeEmptyLinesValue = Console.ReadLine();
            }
            bool.TryParse(removeEmptyLinesValue, out bool removeEmptyLines);

            Console.Write("Enter value for author: ");
            var authorValue = Console.ReadLine();

            string rspContent = $"--language {languageValue}" +
                                $" --output {outputValue}" +
                                $" --note {note}" +
                                $" --sort {sortValue}" +
                                $" --remove-empty-lines {removeEmptyLines}" +
                                $" --author {authorValue}";

            File.WriteAllText("response.rsp", rspContent);
            Console.WriteLine("Response file 'response.rsp' created successfully.");
        });

        var bundleCommand = new Command("bundle", "Bundle command");

        bundleCommand.AddOption(languageOption);
        bundleCommand.AddOption(outputOption);
        bundleCommand.AddOption(noteOption);
        bundleCommand.AddOption(sortOption);
        bundleCommand.AddOption(removeEmptyLinesOption);
        bundleCommand.AddOption(authorOption);
        bundleCommand.SetHandler((language, output, note, sort, removeEmptyLines, author) =>
        {
            try
            {
                string[] files;

                if (language.ToLower() == "all")
                {
                    files = Directory.GetFiles(Directory.GetCurrentDirectory());
                    files = files.Where(file => !file.Contains("bin") && !file.Contains("debug")).ToArray();
                }
                else
                {
                    var languages = language.Split(',').Select(lang => lang.Trim());
                    files = languages
                        .SelectMany(lang => Directory.GetFiles(Directory.GetCurrentDirectory(), $"*.{lang}"))
                        .Where(file => !file.Contains("bin") && !file.Contains("debug"))
                        .Distinct()
                        .ToArray();
                }
                if (files.Length == 0)
                {
                    Console.WriteLine($"No files found with the {language} extension.");
                    return;
                }

                files = SortFiles(files, sort);
                using (StreamWriter writer = new StreamWriter(output?.FullName ?? "output.txt"))
                {
                    if(author!=" ")
                    {
                        writer.WriteLine($"// Author: {author}");
                    }
                    if (note)
                    {                        
                        writer.WriteLine($"// Source code path: {Directory.GetCurrentDirectory()}");
                    }
                    foreach (var file in files)
                    {
                        string fileContent = File.ReadAllText(file);

                        if (removeEmptyLines)
                        {
                            fileContent = RemoveEmptyLines(fileContent);
                        }

                        writer.WriteLine($"// Start of {Path.GetFileName(file)}");
                        writer.WriteLine(fileContent);
                        writer.WriteLine($"// End of {Path.GetFileName(file)}");
                        writer.WriteLine();
                    }
                }
                Console.WriteLine($"Files bundled successfully. Output saved to {output?.FullName ?? "output.txt"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }, languageOption, outputOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

        var rootCommand = new RootCommand("root command");
        rootCommand.AddCommand(bundleCommand);
        rootCommand.AddCommand(createRspCommand);

        rootCommand.InvokeAsync(args).Wait();
    }

    static string RemoveEmptyLines(string input)
    {
        string[] lines = input.Split(Environment.NewLine, StringSplitOptions.None);
        lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        return string.Join(Environment.NewLine, lines);
    }

    static string[] SortFiles(string[] files, string sortBy)
    {
        if (sortBy.ToLower() == "filetype")
        {
            Array.Sort(files, (file1, file2) =>
            {
                string ext1 = Path.GetExtension(file1);
                string ext2 = Path.GetExtension(file2);
                return ext1.CompareTo(ext2);
            });
        }
        else
        {
            Array.Sort(files);
        }

        return files;
    }
}




