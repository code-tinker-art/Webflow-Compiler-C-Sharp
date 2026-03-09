using _Compiler.Phases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

var compiler = new Compiler();
string currentDir = Directory.GetCurrentDirectory();

if (args.Length > 0)
{
    string input = args[0];

    if (input == "-v" || input == "--version")
    {
        var v = Assembly.GetExecutingAssembly().GetName().Version;
        Console.WriteLine($"v{v?.Major}.{v?.Minor}.{v?.Build}");
        return;
    }

    string[] files = input == "."
        ? Directory.GetFiles(currentDir, "*.webf")
        : new[] { Path.GetFullPath(input, currentDir) };

    foreach (var file in files)
    {
        if (File.Exists(file))
        {
            string code = File.ReadAllText(file);
            string html = compiler.convertToHTML(
                code,
                new Dictionary<string, string>(),
                Path.GetDirectoryName(file) ?? currentDir
            );

            string output = Path.ChangeExtension(file, ".html");
            File.WriteAllText(output, html);
            Console.WriteLine($"Emitted: {Path.GetFileName(output)}");
        }
        else
        {
            Console.WriteLine($"Error: {file} not found.");
        }
    }
}else
{
    Console.WriteLine("Usage: webflow <filename>");
}
