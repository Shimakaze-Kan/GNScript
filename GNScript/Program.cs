﻿using GNScript;
using GNScript.Helpers;

internal class Program
{
    private static Interpreter Interpreter;
    private static void Main(string[] args)
    {
        Clear();
        while (true)
        {
            end:

            try
            {
                var currentAllCode = "";
                var line = "";

                Console.Write("> ");
                do
                {
                    line = Console.ReadLine();
                    currentAllCode += line + Environment.NewLine;

                    var trimmedLine = line.Trim();
                    var trimmedUppercaseLine = trimmedLine.ToUpper();

                    var readProgramFromFile = trimmedUppercaseLine.StartsWith("READ ") || trimmedUppercaseLine.StartsWith("READCLS ");
                    if (readProgramFromFile)
                    {
                        (var command, var path) = trimmedLine.Split().ToTuple(0, 1);
                        var code = File.ReadAllText(path);

                        if (string.Equals(command, "readcls", StringComparison.OrdinalIgnoreCase))
                        {
                            Clear();
                        }
                        currentAllCode = code;
                        line = "";
                        Console.WriteLine("File content:" + Environment.NewLine);
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(currentAllCode + Environment.NewLine);
                        Console.ResetColor();
                    } 
                    else if (trimmedUppercaseLine == "CLS")
                    {
                        currentAllCode = "";
                        Clear();
                        goto end;
                    }
                    else if (trimmedUppercaseLine == "DUMP")
                    {
                        Interpreter.Dump();
                        goto end;
                    }
                    else if (trimmedUppercaseLine == "EXIT")
                    {
                        Environment.Exit(0);
                    }
                }
                while (line.Trim() != "");

                var lexer = new Lexer(currentAllCode);
                var tokens = lexer.Tokenize();

                var parser = new Parser(tokens);
                var ast = parser.Parse();

                Interpreter.Run(ast);
            }
            catch (Exception ex)
            {
                Interpreter.ResetScopesAboveRoot();
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    private static void Clear()
    {
        Console.Clear();
        Console.WriteLine("\x1b[3J");
        Console.Clear();
        Console.WriteLine("GN Script Interpreter");
        Interpreter = new Interpreter();
    }
}