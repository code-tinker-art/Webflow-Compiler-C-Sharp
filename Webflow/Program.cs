using _Compiler.Phases;
using System;
using System.Collections.Generic;
using System.IO;

void Run()
{
    var compiler = new Compiler();
    Console.WriteLine("Paste the path to your .webf file or write a inline code.\ntype #quit to quit the process\nHappy coding\n\n");
    while (true)
    {
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
            continue;

        if (input.Equals("#quit"))
            break;

        if (File.Exists(input))
        {
            string code = File.ReadAllText(input);

            string html = compiler.convertToHTML(
                code,
                new Dictionary<string, string>(),
                Path.GetDirectoryName(Path.GetFullPath(input)) ?? "C:/"
            );

            string output = Path.ChangeExtension(input, ".html");
            File.WriteAllText(output, html);

            Console.WriteLine($"Written: {output}");
        }
        else
        {
            string html = compiler.convertToHTML(
                input,
                new Dictionary<string, string>()
            );

            Console.WriteLine(html);
        }
    }
}

Run();
