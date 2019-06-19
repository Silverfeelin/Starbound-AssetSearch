using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using Pak;

namespace AssetSearch
{
    internal class Options
    {
        [Option('i', "input", Required = true, HelpText = "Directory or pak file to search in. Directories are scanned recursively for pak files.")]
        public string InputPath { get; set; }

        [Option('p', "pattern", HelpText = "Asset file name (and path) pattern.")]
        public string Pattern { get; set; }

        [Option('o', "output", HelpText = "Output directory.")]
        public string OutputPath { get; set; }

        [Option('k', "keep", HelpText = "Keep open.")]
        public bool KeepOpen { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Run);
        }

        private static void Run(Options options)
        {
            var inp = options.InputPath;
            var outp = options.OutputPath;
            var save = !string.IsNullOrWhiteSpace(outp) && Directory.Exists(outp);
            var pat = options.Pattern;
            var isDirectory = Directory.Exists(inp);

            Console.WriteLine("Starbound - Asset Search");
            Console.Write("- "); Console.Write($"{(isDirectory ? "Directory" : "File")}: ".PadLeft(11));
            WriteColoredLine(ConsoleColor.Cyan, inp);
            Console.Write("- "); Console.Write("Pattern: ".PadLeft(11));
            WriteColoredLine(ConsoleColor.Cyan, pat);
            if (save)
            {
                Console.Write("- "); Console.Write("Output: ".PadLeft(11));
                WriteColoredLine(ConsoleColor.Cyan, outp);
            }

            Console.WriteLine("Starting search...");


            // Get files
            var files = isDirectory ? GetPaks(options.InputPath)
                : File.Exists(inp) ? new[] {options.InputPath}
                : null;

            if (!(files?.Length > 0))
            {
                Wait("No file(s) found.");
                return;
            }
            
            if (string.IsNullOrEmpty(pat))
                pat= "*";
            else if (!pat.Contains("/"))
                pat = $"*/{pat}";
            pat = $"^{pat}$";

            var pattern = new Regex(pat.Replace(".", "\\.").Replace('?', '.').Replace("*", ".*"));
            var reader = new PakReader();
            // Scan files
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                WriteColoredLine(ConsoleColor.Yellow, $"{file}");

                using (var fs = File.Open(file, FileMode.Open))
                using (var binaryReader = new BinaryReader(fs))
                {
                    var pak = reader.Read(binaryReader);
                    foreach (var item in pak.Items)
                    {
                        if (!pattern.IsMatch(item.Path)) continue;
                        Console.WriteLine($"- {item.Path}");

                        if (save)
                            SaveFile(binaryReader, outp, fileName, item);
                    };
                }

                //if (save)
                    //WriteColoredLine();
            }

            if (options.KeepOpen)
            {
                Wait("Press any key to exit...");
            }
        }

        private static string[] GetPaks(string directory)
            => Directory.GetFiles(directory, "*.pak", SearchOption.AllDirectories);

        private static void SaveFile(BinaryReader binaryReader, string directory, string subDirectory, PakItem item)
        {
            var path = Path.Combine(directory, subDirectory, item.Path.Substring(1));
            var dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);
            
            using (var fs = File.Open(path, FileMode.Create))
            {
                var bytes = PakReader.ReadItem(binaryReader, item);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        private static void WriteColored(ConsoleColor color, string text, params object[] args)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text, args);
            Console.ForegroundColor = c;
        }

        private static void WriteColoredLine(ConsoleColor color, string text, params object[] args)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, args);
            Console.ForegroundColor = c;
        }

        private static void Wait(string text, params object[] args)
        {
            Console.WriteLine(text, args);
            Console.ReadKey(true);
        }
    }
}
