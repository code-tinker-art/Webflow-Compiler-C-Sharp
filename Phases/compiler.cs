using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _Parser.Phases;

namespace _Compiler.Phases
{
    internal class Compiler
    {
        private Parser p = new();
        private List<Element> elements;
        public Compiler() {
            elements = new List<Element>();
        }

        public string convertToHTML(
    string code,
    Dictionary<string, string> components,
    string baseDir = "C:/"
)
        {
            this.elements = p.parse(code);

            foreach (var imp in p.includes)
            {
                string fullPath = Path.GetFullPath(Path.Combine(baseDir, imp.path));

                if (components.ContainsKey(imp.name))
                    continue;

                if (!File.Exists(fullPath))
                    throw new Exception($"Import file not found: {fullPath}");

                string importedCode = File.ReadAllText(fullPath);

                string compiled = convertToHTML(
                    importedCode,
                    components,
                    Path.GetDirectoryName(fullPath)
                );

                components[imp.name] = compiled;
            }

            List<string> converted =
                this.elements
                .Select(e => this.ConvertElementToHTML(e, components))
                .ToList();

            return string.Join("\n", converted);
        }

        private string ConvertElementToHTML(Element e, Dictionary<string, string> c)
        {
            if(c.ContainsKey(e.tagname)) return c[e.tagname];

            string html = $"<{e.tagname}{this.attrs(e)}>";
            if(e.content != null) html+= e.content;
            if (e.children.Count > 0) html += String.Join("\n", e.children.Select(elem => this.ConvertElementToHTML(elem, c)));
            if(e.closed || e.children.Count > 0 || e.content !=null) html += $"</{e.tagname}>";
            return html;
        }

        private string attrs(Element e)
        {
            return this.styles(e.styles) +
                this.data(e.datasets) +
                this.ids(e.ids) +
                this.classes(e.classes) +
                this.props(e.props);
        }

        private string classes(List<string> a)
        {
            return a.Count > 0 ? $" class=\"{string.Join(" ", a)}\"" : "";
        }

        private string ids(List<string> a)
        {
            return a.Count > 0 ? $" id=\"{string.Join(" ", a)}\"" : "";
        }

        private string props(List<Dictionary<string, string>> a)
        {
            return string.Join("", a.Select(x => $" {x["key"]}=\"{x["value"]}\""));
        }

        private string styles(List<Dictionary<string, string>> a)
        {
            return a.Count > 0
                ? $" style=\"{string.Join(";", a.Select(x => $"{x["key"]}:{x["value"]}"))}\""
                : "";
        }

        private string data(List<Dictionary<string, string>> a)
        {
            return string.Join("", a.Select(x => $" data-{x["key"]}=\"{x["value"]}\""));
        }
    }
}
