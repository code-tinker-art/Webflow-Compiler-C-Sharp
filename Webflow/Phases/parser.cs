using _Lexer.Phases;
using _TokenType.Phases;
using System;
using System.Collections.Generic;

namespace _Parser.Phases
{
    public class Element
    {
        public string? tagname;
        public List<Dictionary<string, string>> datasets = new(), props = new(), styles = new();
        public List<string> ids = new(), classes = new();
        public string content = "";
        public bool closed = false;
        public List<Element> children = new();

        public Element(string? tN) => tagname = tN;
    }

    public struct Import
    {
        public string? name;
        public string? path;

        public Import(string? n, string? p)
        {
            name = n;
            path = p;
        }
    }

    public class Parser
    {
        readonly string[] arr = {
            "Tag",
            "Colon",
            "Semicolon",
            "Props",
            "Dataset",
            "Classes",
            "Ids",
            "Content",
            "Style",
            "Block",
            "String",
            "From",
            "Import",
            "Eof"
        };
        private List<Token> src = new();
        private List<Token> tokens = new();
        private Lexer lexer = new();
        public List<Import> includes = new();
        public List<Element> tree { get; private set; } = new();

        public Parser()
        {
            src = new List<Token>();
            tokens = new List<Token>();
        }

        private Token current()
        {
            return this.src.Count > 0
                ? this.src[0]
                : new Token("Eof", TokenType.Eof);
        }

        private Token eat(int type)
        {
            if (this.src.Count == 0)
                throw new Exception("Unexpected end of input");

            Token t = this.src[0];
            this.src.RemoveAt(0);
            int eType = (int)t.type;
            if (!(eType == type))
                throw new Exception($"Unexpected token '{t.value}' of type '{t.type}', Expected {arr[type]}");

            return t;
        }

        public List<Element> parse(string? code)
        {
            this.includes.Clear();
            this.tree.Clear();

            this.src = lexer.Tokenize(code);
            this.tokens = new List<Token>(src);
            while (!this.current().type.Equals(TokenType.Eof))
            {
                if (this.current().type.Equals(TokenType.Import))
                {
                    if (this.tree.Count > 0)
                        throw new Exception("Imports must appear before elements");

                    this.parseImport();
                }
                else
                {
                    this.tree.Add(this.ParseElement());
                }
            }
            return this.tree;
        }

        private Element ParseElement()
        {
            string tag = this.eat((int)TokenType.Tag).value;
            Element elem = new Element(tag);
            this.eat((int)TokenType.Colon);

            while (true)
            {
                Token t = this.current();

                if (t.type.Equals(TokenType.Semicolon))
                {
                    this.eat((int)TokenType.Semicolon);
                    elem.closed = true;
                    break;
                }

                if (t.type.Equals(TokenType.Tag))
                {
                    elem.children.Add(this.ParseElement());
                    continue;
                }

                if (t.type == TokenType.Props ||
                    t.type == TokenType.Dataset ||
                    t.type == TokenType.Classes ||
                    t.type == TokenType.Ids ||
                    t.type == TokenType.Content ||
                    t.type == TokenType.Style)
                {
                    this.parseSet(elem);
                    continue;
                }

                throw new Exception($"Unexpected token {t.value} at token position {this.tokens.Count - this.src.Count - 1}");
            }

            return elem;
        }

        private void parseSet(Element elem)
        {
            Token t = this.current();

            switch (t.type)
            {
                case TokenType.Props:
                    this.eat((int)TokenType.Props);
                    elem.props = this.parseKeyValue(this.eat((int)TokenType.Block).value);
                    break;
                case TokenType.Dataset:
                    this.eat((int)TokenType.Dataset);
                    elem.datasets = this.parseKeyValue(this.eat((int)TokenType.Block).value);
                    break;
                case TokenType.Classes:
                    this.eat((int)TokenType.Classes);
                    elem.classes = this.parseList(this.eat((int)TokenType.Block).value);
                    break;
                case TokenType.Ids:
                    this.eat((int)TokenType.Ids);
                    elem.ids = this.parseList(this.eat((int)TokenType.Block).value);
                    break;
                case TokenType.Content:
                    this.eat((int)TokenType.Content);
                    elem.content = this.parseContent(this.eat((int)TokenType.Block).value);
                    break;
                case TokenType.Style:
                    this.eat((int)TokenType.Style);
                    elem.styles = this.parseKeyValue(this.eat((int)TokenType.Block).value);
                    break;
                default:
                    throw new Exception("Invalid set or missing semicolon");
            }
        }

        private string parseContent(string value)
        {
            string result = "";
            int i = 0;

            while (i < value.Length)
            {
                if (i + 1 < value.Length &&
                    value[i] == '\\' &&
                    value[i + 1] == ',')
                {
                    result += ",";
                    i += 2;
                }
                else
                {
                    result += value[i];
                    ++i;
                }
            }

            return result;
        }

        private List<string> parseList(string value)
        {
            List<string> arr = new();
            string curr = "";
            bool quotes = false;

            for (int i = 0; i <= value.Length; ++i)
            {
                char c = i == value.Length ? '\0' : value[i];

                if ((c == ',' && !quotes) || c == '\0')
                {
                    if (curr.Trim().Length > 0)
                        arr.Add(this.unquote(curr.Trim()));
                    curr = "";
                }
                else if (c == '"')
                {
                    quotes = !quotes;
                    curr += c;
                }
                else
                {
                    curr += c;
                }
            }

            return arr;
        }

        private string unquote(string v)
        {
            if (v.Length >= 2 && v[0] == '"' && v[v.Length - 1] == '"')
                return v.Substring(1, v.Length - 2);

            return v;
        }

        private List<Dictionary<string, string>> parseKeyValue(string value)
        {
            List<Dictionary<string, string>> arr = new();
            string curr = "";
            bool quotes = false;

            for (int i = 0; i <= value.Length; i++)
            {
                bool isEnd = (i == value.Length);
                char c = isEnd ? '\0' : value[i];

                if (isEnd || (c == ',' && !quotes))
                {
                    if (!string.IsNullOrWhiteSpace(curr))
                    {
                        string[] parts = curr.Split(':', 2);
                        string k = parts[0].Trim();
                        string v = parts.Length > 1 ? parts[1].Trim() : "";

                        arr.Add(new Dictionary<string, string>
                        {
                            { "key", k },
                            { "value", v }
                        });
                    }
                    curr = "";
                }
                else if (c == '"')
                {
                    quotes = !quotes;
                    curr += c;
                }
                else
                {
                    curr += c;
                }
            }

            return arr;
        }

        private void parseImport()
        {
            this.eat((int)TokenType.Import);
            string name = this.eat((int)TokenType.Tag).value;
            this.eat((int)TokenType.From);
            string pathName = this.eat((int)TokenType.String).value;
            this.eat((int)TokenType.Semicolon);

            this.includes.Add(new Import(name, pathName));
        }
    }
}