using GNScript;

internal class Program
{
    private static Interpreter Interpreter;
    private static void Main(string[] args)
    {
        //string code = @"
        //    function add(a, b)
        //        return a + b

        //    function mul(a, b)
        //        return a * b

        //    y = input
        //    if y <> 1
        //        x = add(y, 3)
        //        x = mul(x,10)
        //    else
        //        x = mul(5,3)
        //    end

        //    print x

        //    count = 0
        //    while count < 5
        //        print count
        //        count = count + 1
        //    end

        //    print ""---------------""

        //    for i = 0; i < 3; i = i + 1
        //        print i
        //    end

        //    myString = ""Hello, world!""
        //    print myString
        //";

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

                    if (line.Trim().ToUpper() == "CLS")
                    {
                        currentAllCode = "";
                        Clear();
                        goto end;
                    }

                    if (line.Trim().ToUpper() == "DUMP")
                    {
                        Interpreter.Dump();
                        goto end;
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
                Console.WriteLine("Błąd: " + ex.Message);
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