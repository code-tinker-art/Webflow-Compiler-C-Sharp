using _Parser.Phases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _Compiler.Phases
{
    internal class Compiler
    {
        private Dictionary<string, Element> componentDefs = new();

        public string convertToHTML(
            string code,
            Dictionary<string, string> components,
            string baseDir = "C:/",
            bool isImport = false
        )
        {
            Parser p = new Parser();
            List<Element> elements = p.parse(code);

            foreach (var imp in p.includes)
            {
                string fullPath = Path.GetFullPath(Path.Combine(baseDir, imp.path));

                if (!File.Exists(fullPath))
                    throw new Exception($"Import file not found: {fullPath}");

                string importedCode = File.ReadAllText(fullPath);

                RegisterComponents(importedCode, imp.name, Path.GetDirectoryName(fullPath) ?? baseDir);
            }

            if (isImport)
                return "";

            List<string> html = new();

            foreach (var e in elements)
                html.Add(Render(e));

            return string.Join("\n", html);
        }

        private void RegisterComponents(string code, string name, string baseDir)
        {
            Parser p = new Parser();
            List<Element> elements = p.parse(code);

            if (elements.Count == 0)
                throw new Exception("Component file contains no elements");

            componentDefs[name] = elements[0];

            foreach (var imp in p.includes)
            {
                string fullPath = Path.GetFullPath(Path.Combine(baseDir, imp.path));

                if (!File.Exists(fullPath))
                    throw new Exception($"Import file not found: {fullPath}");

                string importedCode = File.ReadAllText(fullPath);

                RegisterComponents(importedCode, imp.name, Path.GetDirectoryName(fullPath) ?? baseDir);
            }
        }

        private string Render(Element e)
        {
            if (componentDefs.ContainsKey(e.tagname))
                return Render(componentDefs[e.tagname]);

            string html = $"<{e.tagname}{Attrs(e)}>";

            if (!string.IsNullOrEmpty(e.content))
                html += e.content;

            if (e.children.Count > 0)
                html += string.Join("\n", e.children.Select(Render));

            if (e.closed || e.children.Count > 0 || !string.IsNullOrEmpty(e.content))
                html += $"</{e.tagname}>";

            return html;
        }

        private string Attrs(Element e)
        {
            return Styles(e.styles) +
                   Data(e.datasets) +
                   Ids(e.ids) +
                   Classes(e.classes) +
                   Props(e.props);
        }

        private string Classes(List<string> a)
        {
            return a.Count > 0 ? $" class=\"{string.Join(" ", a)}\"" : "";
        }

        private string Ids(List<string> a)
        {
            return a.Count > 0 ? $" id=\"{string.Join(" ", a)}\"" : "";
        }

        private string Props(List<Dictionary<string, string>> a)
        {
            return string.Join("", a.Select(x => $" {x["key"]}=\"{x["value"]}\""));
        }

        private string Styles(List<Dictionary<string, string>> a)
        {
            return a.Count > 0
                ? $" style=\"{string.Join(";", a.Select(x => $"{x["key"]}:{x["value"]}"))}\""
                : "";
        }

        private string Data(List<Dictionary<string, string>> a)
        {
            return string.Join("", a.Select(x => $" data-{x["key"]}=\"{x["value"]}\""));
        }
    }
}