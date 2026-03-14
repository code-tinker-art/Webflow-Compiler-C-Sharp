using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _TokenType.Phases;

namespace _Lexer.Phases
{
    public struct Token
    {
        public string value;
        public TokenType type;

        public Token(string Value, TokenType Type)
        {
            value = Value;
            type = Type;
        }
    };
    public class Lexer
    {
        public string code;
        private List<string> src;
        private List<Token> tokens;
        public Lexer() {
            this.code = "";
            this.src = new List<string>();
            this.tokens = new List<Token>();
        }

        public List<Token> Tokenize(string? code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            tokens.Clear();

            this.code = code; 
            this.src = this.code.Select(c => c.ToString()).ToList();
            var map = new Dictionary<string, TokenType>
            {
                {"from", TokenType.From},
                {"import", TokenType.Import },
                { "props", TokenType.Props},
                { "content", TokenType.Content},
                { "classes", TokenType.Classes }, 
                {"ids", TokenType.Ids }, 
                { "dataset", TokenType.Dataset}, 
                { "styles", TokenType.Style}
            };
            while (this.src.Count > 0) {
                string c = this.src[0];
                if (c == ":")
                {
                    tokens.Add(this.token(c, TokenType.Colon));
                    this.src.RemoveAt(0);
                }
                else if (c == ";")
                {
                    tokens.Add(this.token(c, TokenType.Semicolon));
                    this.src.RemoveAt(0);
                }
                else if (c == "{")
                {
                    this.src.RemoveAt(0);
                    string value = "";
                    while (this.src.Count > 0)
                    {
                        if (this.src.Count >= 2 && this.src[0].Equals("\\") && this.src[1].Equals("{"))
                        {
                            value += "{";
                            this.src.RemoveAt(0);
                            this.src.RemoveAt(0);
                        }
                        else if (this.src.Count >= 2 && this.src[0].Equals("\\") && this.src[1].Equals("}"))
                        {
                            value += "}";
                            this.src.RemoveAt(0);
                            this.src.RemoveAt(0);
                        }
                        else if (this.src.Count >= 2 && this.src[0].Equals("\\") && this.src[1].Equals("\\"))
                        {
                            value += "\\";
                            this.src.RemoveAt(0);
                            this.src.RemoveAt(0);
                        }
                        else if (this.src[0].Equals("}"))
                        {
                            break;
                        }
                        else
                        {
                            value += this.src[0];
                            this.src.RemoveAt(0);
                        }
                    }

                    if (this.src.Count == 0 || this.src[0] != "}")
                    {
                        throw new Exception("Unclosed block found..");
                    }

                    this.src.RemoveAt(0);
                    tokens.Add(this.token(value, TokenType.Block));
                }
                else if (c == "\"")
                {
                    this.src.RemoveAt(0);
                    string value = ""; 
                    while (this.src.Count > 0) {
                        if (this.src.Count >= 2 && this.src[0].Equals("\\") && this.src[1].Equals("\""))
                        {
                            value += "\"";
                            this.src.RemoveAt(0);
                            this.src.RemoveAt(0);
                        }
                        else if (this.src.Count >= 2 && this.src[0].Equals("\\") && this.src[1].Equals("\\"))
                        {
                            value += "\\";
                            this.src.RemoveAt(0);
                            this.src.RemoveAt(0);
                        }
                        else if (this.src[0].Equals("\""))
                            break;
                        else
                        {
                            value += this.src[0];
                            this.src.RemoveAt(0);
                        }
                    }
                    if (this.src.Count == 0 || this.src[0] != "\"")
                    {
                        throw new Exception("Unclosed string found...");
                    }
                    this.src.RemoveAt(0);
                    tokens.Add(this.token(value, TokenType.String));
                }else if (this.src.Count >= 2 && this.src[0] == "-" && this.src[1] == "-")
                {
                    while (this.src.Count > 0 && this.src[0] != "\n")
                        this.src.RemoveAt(0);

                    if (this.src.Count > 0)
                        this.src.RemoveAt(0);
                } else if ((this.src[0].ToLower() != this.src[0].ToUpper()) || "0123456789".Contains(this.src[0]))
                {
                    string str = "";
                    while(this.src.Count > 0 && (this.src[0].ToUpper() != this.src[0].ToLower() || "0123456789".Contains(this.src[0])))
                    {
                        str += this.src[0];
                        this.src.RemoveAt(0);
                    }
                    TokenType type = map.GetValueOrDefault(str.ToLower(), TokenType.Tag);
                    tokens.Add(token(str, type));
                }else if(" \n\r\t".Contains(c))
                {
                    src.RemoveAt(0);
                }
                else
                {
                    throw new Exception($"Unexpected char {c}");
                }
            }
            tokens.Add(token("EOF", TokenType.Eof));
            return tokens;
        }

        public void printSrc()
        {
            if (this.src != null)
            {

                foreach (var t in this.src)
                {
                    Console.WriteLine(t);
                }
            }
            else
            {
                Console.WriteLine("Empty src[] array");
            }
        }

        private Token token(string? value, TokenType type)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return new Token(value, type);
        }
    }
}
