using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymanticAnalyzer
{
    class LexicalAnalyzer
    {
        // Global controllers
        public static bool readNextChar = true;
        public static bool EOF;

        // Global variables
        public static int currentLine = 1;
        static char currentChar;
        public static string nextToken;
        public static int tokenCode;
        public static int positionInTable;

        public static int stringCounter = 0;

        // Global resources
        public static SymbolTable symbolTable = new SymbolTable();
        public static ReserveTable ReserveWordsTable;
        public static ReserveTable CodeTable;
        static DFA LEXDFA;
        static StreamReader fileToTokenize;
        static StreamReader Trace;
        static StreamReader ErrorHandler;

        // Constant identifiers
        const int START = 0;
        const int NEWLINE = 10;
        const int COMMENT_BODY = 12;
        const int COMMENT_ENDING_STAR = 13;
        const int GET_OUT = 38;
        const int COMMENT_NOT_ENDED = 39;
        const int STRING_NOT_ENDED = 40;
        const int UNREC_CHAR = 41;

        public LexicalAnalyzer()
        {
            InitializeStructures();
            InitializeInputFile("");
        }

        public static string GetNextToken(bool TraceOn)
        {
            Token newToken = new Token(); // Temporary Token storage
            int currentState = 0; // The state of the DFA now
            int previousState = 0; // The state of the DFA 1 step behind
            bool continueBuilding = true; // Used to see if the token is over
            string lexemeTobuild = ""; // The token string

            // If we are out of characters, stop
            if (fileToTokenize.EndOfStream)
            {
                EOF = true;
            }

            // If the token is over, or the file is completely read, stop
            while (continueBuilding && !fileToTokenize.EndOfStream)
            {
                // Set to false if we need to not consume a character during
                // this pass
                if (readNextChar)
                {
                    currentChar = Convert.ToChar(fileToTokenize.Read());
                    currentChar = char.ToUpper(currentChar);
                }

                if (IsNewline(currentChar))
                {
                    currentLine += 1;
                }

                if (TraceOn && IsNewline(currentChar))
                {
                    Console.WriteLine(currentLine - 1 + ": " + Trace.ReadLine());
                }

                // Save the state you are currently in in case the next state is exit
                previousState = currentState;

                // O P E R A T E T H E M A C H I N E
                currentState = LEXDFA.storedDFA[GetCharacterDFAIndex(currentChar), currentState];

                // Machine is not in an escape or error state
                if (currentState != GET_OUT
                    && currentState != COMMENT_NOT_ENDED
                    && currentState != STRING_NOT_ENDED
                    && currentState != UNREC_CHAR)
                {
                    lexemeTobuild += currentChar;
                    readNextChar = true;
                }
                // Machine has just read a \n token out of context
                // Escape this
                else if ((currentState == GET_OUT && previousState == START))
                {
                    continueBuilding = false;
                    readNextChar = true;
                }
                // Machine has just been put into the escape state
                // Machine has constructed a valid token to this point (accept)
                // Find out what kind of token it is and return it
                else if (currentState == GET_OUT && LEXDFA.Includes(previousState))
                {
                    continueBuilding = false;
                    readNextChar = false;

                    switch (previousState)
                    {
                        case 1: // Identifier found (variable)[string]
                            lexemeTobuild = TruncateIdentifier(lexemeTobuild);
                            // Don't add reserve words to symbol table
                            if (IsReserveWord(lexemeTobuild))
                            {
                                newToken._lexeme = lexemeTobuild;
                                newToken._tokenCode = ReserveWordsTable.LookupName(lexemeTobuild);
                            }
                            // Do add other identifiers
                            else
                            {
                                newToken._lexeme = lexemeTobuild;
                                newToken._tokenCode = 50;

                                // Set global position in symbol table
                                positionInTable = AddToSymbolTable(newToken, 1);
                            }
                            break;
                        case 2: // Numeric Constant (constant)[integer]
                            lexemeTobuild = TruncateNumericConstant(lexemeTobuild);

                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 51;

                            // Set global position in symbol table
                            positionInTable = AddToSymbolTable(newToken, 2);
                            break;
                        case 3: // Numeric Constant (constant)[float]
                            lexemeTobuild = TruncateNumericConstant(lexemeTobuild);

                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 52;

                            // Set global position in symbol table
                            positionInTable = AddToSymbolTable(newToken, 3);
                            break;
                        case 6: // Numeric Constant (constant)[float+exp]
                            lexemeTobuild = TruncateNumericConstant(lexemeTobuild);

                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 52;

                            // Set global position in symbol table
                            positionInTable = AddToSymbolTable(newToken, 3);
                            break;
                        case 9: // String found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 53;

                            positionInTable = AddToSymbolTable(newToken, 4);
                            break;
                        case 14: // (**) comment
                            // Ignore this one
                            newToken._lexeme = "";
                            newToken._tokenCode = 999;

                            nextToken = newToken._lexeme;
                            tokenCode = newToken._tokenCode;
                            break;
                        case 17: // ## comment
                            // Also here
                            newToken._lexeme = "";
                            newToken._tokenCode = 999;

                            nextToken = newToken._lexeme;
                            tokenCode = newToken._tokenCode;
                            break;
                        case 18: // / found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 30;
                            break;
                        case 19: // * found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 31;
                            break;
                        case 20: // + found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 32;
                            break;
                        case 21: // - found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 33;
                            break;
                        case 22: // ( found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 34;
                            break;
                        case 23: // ) found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 35;
                            break;
                        case 24: // ; found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 36;
                            break;
                        case 25: // : found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 47;
                            break;
                        case 26: // := found
                            newToken._lexeme = ":=";
                            newToken._tokenCode = 37;
                            break;
                        case 27: // > found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 38;
                            break;
                        case 28: // < found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 39;
                            break;
                        case 29: // >= found
                            newToken._lexeme = ">=";
                            newToken._tokenCode = 40;
                            break;
                        case 30: // <= found
                            newToken._lexeme = "<=";
                            newToken._tokenCode = 41;
                            break;
                        case 31: // = found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 42;
                            break;
                        case 32: // <> found
                            newToken._lexeme = "<>";
                            newToken._tokenCode = 43;
                            break;
                        case 33: // , found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 44;
                            break;
                        case 34: // [ found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 45;
                            break;
                        case 35: // ] found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 46;
                            break;
                        case 36: // . found
                            newToken._lexeme = lexemeTobuild;
                            newToken._tokenCode = 48;
                            break;
                    }

                    // Set the global string and get out
                    nextToken = newToken._lexeme;
                    tokenCode = newToken._tokenCode;
                    return nextToken;
                }
                // Machine has just been put into the escape state
                // Machine has constructed an invalid token (reject)
                else if (currentState == GET_OUT && !LEXDFA.Includes(previousState))
                {
                    continueBuilding = false;
                    if (previousState == COMMENT_BODY || previousState == COMMENT_ENDING_STAR)
                    {
                        Console.WriteLine
                            ("//////////////////ERROR - Comment not closed before EOF - ERROR//////////////////");
                    }
                }
                // Machine has encountered a newline character before
                // string fully terminates
                else if (currentState == STRING_NOT_ENDED)
                {
                    Console.WriteLine
                        ("//////////////////ERROR - Unterminated String Found - ERROR//////////////////");
                    continueBuilding = false;
                }
                // Machine encountered non recognized character
                else if (currentState == UNREC_CHAR)
                {
                    lexemeTobuild += currentChar;

                    // Some undefined input character
                    newToken._lexeme = lexemeTobuild;
                    newToken._tokenCode = 99;

                    nextToken = newToken._lexeme;
                    tokenCode = newToken._tokenCode;

                    // Set token code to 99 and get out
                    continueBuilding = false;
                    readNextChar = true;
                    return nextToken;
                }
            }
            return null;
        }

        // Adds an identifier or constant to the symbol table
        public static int AddToSymbolTable(Token tokenToAdd, int WhatKindOfSymbol)
        {
            // Switch on whether its a variable or not
            switch (WhatKindOfSymbol)
            {
                case 1: // Add a variable name
                    return symbolTable.AddSymbol(tokenToAdd._lexeme, 1, "");
                case 2: // Add an integer constant
                    try // If the int value is too big, report and set to 0
                    {
                        return symbolTable.AddSymbol(tokenToAdd._lexeme, 2, int.Parse(tokenToAdd._lexeme));
                    }
                    catch
                    {
                        Console.WriteLine
                                ("//////////////////ERROR - INT TOO BIG - ERROR//////////////////");
                        return symbolTable.AddSymbol(tokenToAdd._lexeme, 2, (int)0);
                    }
                case 3: // Add a float constant
                    return symbolTable.AddSymbol(tokenToAdd._lexeme, 2, double.Parse(tokenToAdd._lexeme));
                case 4:
                    stringCounter++;
                    return symbolTable.AddSymbol("StrConst" + stringCounter, 2, (string)tokenToAdd._lexeme);
            }
            return -1;
        }

        public static bool IsNewline(char character)
        {
            if (character == NEWLINE)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Checks to see if identifier belongs to the reserved list
        public static bool IsReserveWord(string identifier)
        {
            if (ReserveWordsTable.LookupName(identifier) != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // If Identifier is longer than 30 characters, trim it down
        public static string TruncateIdentifier(string identifier)
        {
            if (identifier.Count() > 30)
            {
                Console.WriteLine
                    ("////////////////// WARN - Identifier too long, truncating. - WARN //////////////////");
                string truncatedString = "";

                for (int i = 0; i < 30; i++)
                {
                    truncatedString += identifier[i];
                }
                return truncatedString;
            }
            else
            {
                return identifier;
            }
        }

        // If Numeric constant is longer than 16 characters, trim it
        public static string TruncateNumericConstant(string numeriConstant)
        {
            if (numeriConstant.Count() > 16)
            {
                Console.WriteLine
                    ("////////////////// WARN - Numeric constant too long, truncating. - WARN //////////////////");
                string truncatedConstant = "";

                for (int i = 0; i < 16; i++)
                {
                    truncatedConstant += numeriConstant[i];
                }
                return truncatedConstant;
            }
            else
            {
                return numeriConstant;
            }
        }

        // This method returns the index to the first component of the DFA[x,y]
        // the character currently being looked at determines x, which is used
        // to look into the DFA
        public static int GetCharacterDFAIndex(char character)
        {
            if (character == ' ' || character == 9)
            {
                return 0;
            }
            else if (char.IsLetter(character) && (character != 'E'))
            {
                //Console.WriteLine("Character is a letter, not E though.");
                return 1;
            }
            else if (char.IsDigit(character))
            {
                //Console.WriteLine("Character is a digit.");
                return 2;
            }
            else if (character == 'E')
            {
                //Console.WriteLine("Character is E.");
                return 3;
            }
            else if (character == '+')
            {
                //Console.WriteLine("Character is +");
                return 4;
            }
            else if (character == '-')
            {
                //Console.WriteLine("Character is -");
                return 5;
            }
            else if (character == '"')
            {
                //Console.WriteLine("Character is \"");
                return 6;
            }
            else if (character == '*')
            {
                //Console.WriteLine("Character is *");
                return 7;
            }
            else if (character == '(')
            {
                //Console.WriteLine("Character is (");
                return 8;
            }
            else if (character == ')')
            {
                //Console.WriteLine("Character is )");
                return 9;
            }
            else if (character == '/')
            {
                //Console.WriteLine("Character is /");
                return 10;
            }
            else if (character == ';')
            {
                //Console.WriteLine("Character is ;");
                return 11;
            }
            else if (character == ':')
            {
                //Console.WriteLine("Character is :");
                return 12;
            }
            else if (character == '=')
            {
                //Console.WriteLine("Character is =");
                return 13;
            }
            else if (character == '<')
            {
                //Console.WriteLine("Character is <");
                return 14;
            }
            else if (character == '>')
            {
                //Console.WriteLine("Character is >");
                return 15;
            }
            else if (character == ',')
            {
                //Console.WriteLine("Character is ,");
                return 16;
            }
            else if (character == '.')
            {
                //Console.WriteLine("Character is .");
                return 17;
            }
            else if (character == '[')
            {
                //Console.WriteLine("Character is ]");
                return 18;
            }
            else if (character == ']')
            {
                //Console.WriteLine("Character is ]");
                return 19;
            }
            else if (character == '$')
            {
                //Console.WriteLine("Character is $");
                return 20;
            }
            else if (character == '_')
            {
                //Console.WriteLine("Character is _");
                return 21;
            }
            else if (character == 13 || character == 10)
            {
                //Console.WriteLine("Character is \\n");
                return 22;
            }
            else if (character == '#')
            {
                //Console.WriteLine("Character is #");
                return 23;
            }
            else // Anything thats not listed anywhere else is unrecognized
            {
                return 25;
            }
        }

        // Print token
        public static void PrintToken(string token, int tkCode)
        {
            Console.Write
                ("Lexeme: " + token.PadRight(10) + " Token Code: " + tkCode.ToString().PadRight(10));
            Console.Write
                (" Mnemonic: " + CodeTable.LookupCode(tkCode).PadRight(10));

            // If token is within the symbol table, report it index
            if (CodeTable.LookupName(CodeTable.LookupCode(tkCode)) == 50 ||
                CodeTable.LookupName(CodeTable.LookupCode(tkCode)) == 51 ||
                CodeTable.LookupName(CodeTable.LookupCode(tkCode)) == 52)
            {
                Console.Write
                    (" Symbol Table Index: " + symbolTable.LookupSymbol(token));
            }
            Console.WriteLine();
        }

        // Opens the file for reading
        public static void InitializeInputFile(string inputfile)
        {
            fileToTokenize = new StreamReader("testsyn.txt");
            Trace = new StreamReader("testsyn.txt");
            ErrorHandler = new StreamReader("testsyn.txt");
        }

        public static void InitializeStructures()
        {
            LEXDFA = new DFA(); // Creates DFA
            ReserveWordsTable = CreateReserveWordsTable(); // Creates reserve word table
            CodeTable = CreateCodeTable(); // Creates mnemonic table
        }

        // Create the list of reserved words
        public static ReserveTable CreateReserveWordsTable()
        {
            ReserveTable reserveWordsTable = new ReserveTable(25);

            reserveWordsTable.Add("GOTO", 0);
            reserveWordsTable.Add("INTEGER", 1);
            reserveWordsTable.Add("TO", 2);
            reserveWordsTable.Add("DO", 3);
            reserveWordsTable.Add("IF", 4);
            reserveWordsTable.Add("THEN", 5);
            reserveWordsTable.Add("ELSE", 6);
            reserveWordsTable.Add("FOR", 7);
            reserveWordsTable.Add("OF", 8);
            reserveWordsTable.Add("WRITELN", 9);
            reserveWordsTable.Add("BEGIN", 10);
            reserveWordsTable.Add("END", 11);
            reserveWordsTable.Add("ARRAY", 12);
            reserveWordsTable.Add("VAR", 13);
            reserveWordsTable.Add("WHILE", 14);
            reserveWordsTable.Add("UNIT", 15);
            reserveWordsTable.Add("LABEL", 16);
            reserveWordsTable.Add("REPEAT", 17);
            reserveWordsTable.Add("UNTIL", 18);
            reserveWordsTable.Add("PROCEDURE", 19);
            reserveWordsTable.Add("DOWNTO", 20);
            reserveWordsTable.Add("READLN", 21);
            reserveWordsTable.Add("RETURN", 22);
            reserveWordsTable.Add("FLOAT", 23);
            reserveWordsTable.Add("STRING", 24);

            return reserveWordsTable;
        }

        // Create mnemonic list
        public static ReserveTable CreateCodeTable()
        {
            ReserveTable codeTable = new ReserveTable(50);

            codeTable.Add("GOTO", 0);
            codeTable.Add("INTG", 1);
            codeTable.Add("TO__", 2);
            codeTable.Add("DO__", 3);
            codeTable.Add("IF__", 4);
            codeTable.Add("THEN", 5);
            codeTable.Add("ELSE", 6);
            codeTable.Add("FOR_", 7);
            codeTable.Add("OF__", 8);
            codeTable.Add("WRTL", 9);
            codeTable.Add("BEGN", 10);
            codeTable.Add("END_", 11);
            codeTable.Add("ARRY", 12);
            codeTable.Add("VAR_", 13);
            codeTable.Add("WHLE", 14);
            codeTable.Add("UNIT", 15);
            codeTable.Add("LABL", 16);
            codeTable.Add("REPT", 17);
            codeTable.Add("UNTL", 18);
            codeTable.Add("PCDR", 19);
            codeTable.Add("DWNT", 20);
            codeTable.Add("RDLN", 21);
            codeTable.Add("RTRN", 22);
            codeTable.Add("FLT_", 23);
            codeTable.Add("STR_", 24);
            codeTable.Add("SLSH", 30);
            codeTable.Add("STAR", 31);
            codeTable.Add("PLUS", 32);
            codeTable.Add("MINS", 33);
            codeTable.Add("LPRN", 34);
            codeTable.Add("RPRN", 35);
            codeTable.Add("SCLN", 36);
            codeTable.Add("BCMS", 37);
            codeTable.Add("GRTR", 38);
            codeTable.Add("LEST", 39);
            codeTable.Add("GRTE", 40);
            codeTable.Add("LSTE", 41);
            codeTable.Add("EQUL", 42);
            codeTable.Add("EQQL", 43);
            codeTable.Add("CMMA", 44);
            codeTable.Add("LBRK", 45);
            codeTable.Add("RBRK", 46);
            codeTable.Add("COLN", 47);
            codeTable.Add("PERD", 48);
            codeTable.Add("IDTY", 50);
            codeTable.Add("NCNI", 51);
            codeTable.Add("NCNF", 52);
            codeTable.Add("SCN_", 53);
            codeTable.Add("BAD_", 99);

            return codeTable;
        }
    }
}
