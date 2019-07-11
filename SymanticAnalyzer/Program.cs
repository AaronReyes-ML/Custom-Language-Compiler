using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymanticAnalyzer
{
    class Program
    {

        // File input and display variables
        private static string FILENAME = "";
        private static bool showSymbolTable = false;
        private static bool showQuadTable = false;

        static void Main(string[] args)
        {

            //args = new string[]{"", "false", "false", "true", "true", "true", "true"};
            //args = new string[]{"", "false", "false", "false", "false", "false", "false"};
            args = new string[]{"", "true", "true", "true", "true", "true", "true"};

            // Used when input is accepted from the command line, otherwise will execute
            // a default file found it the directory of the exe called "testsyn.txt"
            if (args.Length > 0)
            {
                if (args.Length >= 1)
                {
                    FILENAME = args[0]; // The filename of the sourcecode
                }
                if (args.Length >= 2 && args[1] == "true")
                {
                    SyntaxAnalyzer.traceOn = true; // Toggles on 'enter/exit' display mode
                }
                if (args.Length >= 3 && args[2] == "true")
                {
                    SyntaxAnalyzer.verbose = true; // Toggles on reporting of the lexemes and indeces
                }
                if (args.Length >= 4 && args[3] == "true")
                {
                    SyntaxAnalyzer.extraverbose = true; // Toggles on source line echo
                }
                if (args.Length >= 5 && args[4] == "true")
                {
                    SyntaxAnalyzer.interpretTraceOn = true; // Toggles on quad code echo
                }
                if (args.Length >= 6 && args[5] == "true")
                {
                    // Toggles visibility of the symbol table at the end of execution
                    showSymbolTable = true;
                }
                if (args.Length >= 7 && args[6] == "true")
                {
                    // Toggles visibility of the quad table at the end of execution
                    showQuadTable = true;
                }
            }

            // Compiles the source file
            // Conditional variables set by command line input
            SyntaxAnalyzer.DoParse(FILENAME);
            
            // If the source file was compiled without errors
            // the user will have the chance to interpret when ready
            // EOF makes sure all file contents were processed
            // Sometimes errors happen in the last line which doesn't trigger Error occured
            // Error is used in this case
            // Error occured is a global error variable set when an error is caught
            if (LexicalAnalyzer.EOF && !SyntaxAnalyzer.Error && !SyntaxAnalyzer.ErrorOccured)
            {
                Console.WriteLine("Press enter to execute");

                Console.ReadLine();

                if (showSymbolTable)
                {
                    LexicalAnalyzer.symbolTable.PrintSymbolTable();
                }
                if (showQuadTable)
                {
                    SyntaxAnalyzer.programQuadTable.PrintQuadTable();
                }

                Interpreter.IntepretQuads(SyntaxAnalyzer.programQuadTable,
                                            LexicalAnalyzer.symbolTable,
                                            SyntaxAnalyzer.interpretTraceOn);
                if (showSymbolTable)
                {
                    LexicalAnalyzer.symbolTable.PrintSymbolTable();
                }
            }

            Console.ReadLine();
        }
    }
}
