using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymanticAnalyzer
{
    class SyntaxAnalyzer
    {
        // Global variables
        public static bool traceOn = false;
        public static bool interpretTraceOn = false;

        // Error and warn globals
        public static bool Error = false;
        public static bool ErrorOccured = false;
        public static bool Warn = false;
        public static bool UNDECLAREDERROR = false;
        public static int ErrorCount = 0;

        // Identificatiion globals
        static bool isProgIdentifier = false;
        static bool isLabel = false;
        static bool isVariable = false;
        static bool isLabelDecleration = false;
        static bool isVariableDecleration = false;
        static int gotoLabelIndexToUpdate = -1;
        static string UNIQUEIDENTIFIER = "";

        public static bool verbose = false;
        public static bool extraverbose = false;

        // Output globals
        static int numberOfSpaces = 0;
        static bool force;

        // Label/Variable data structures
        static List<string> declaredLables;
        static List<string> declaredVariables;

        static List<string> usedLabels;
        static List<string> usedVariables;

        static List<string> unusedVariables;
        static List<string> unusedLabels;

        // Used to ensure that variables are
        // in the symbol table with their correct types
        static List<int> indexesToUpdateType;

        // The quad table to be used for interpretation
        public static QuadTable programQuadTable = new QuadTable();

        // Increments with the number of temp variables needed
        private static int tempCounter = 0;
        // Increments with the number of string constants needed
        private static int StringConstCounter = 0;

        // The value of float data type for symbols
        // Used during float promotion
        private const int DOUBLEDATATYPE = 1;

        // Used for relational expressions
        // to determine which branch operation should be applied
        private const int relopEqual = 1;
        private const int relopNotEqual = 2;
        private const int relopLess = 3;
        private const int relopGreater = 4;
        private const int relopLessEqual = 5;
        private const int relopGreaterEqual = 6;

        // Used to indicate which quad code should be used
        private static int quadStopCode = 0;
        private static int quadDivCode = 1;
        private static int quadMulCode = 2;
        private static int quadSubCode = 3;
        private static int quadAddCode = 4;
        private static int quadMovCode = 5;
        private static int quadSTICode = 6;
        private static int quadLDICode = 7;
        private static int quadBNZCode = 8;
        private static int quadBNPCode = 9;
        private static int quadBNNCode = 10;
        private static int quadBZCode = 11;
        private static int quadBPCode = 12;
        private static int quadBNCode = 13;
        private static int quadBRCode = 14;
        private static int quadBINDRCode = 15;
        private static int quadPrintCode = 16;


        // Generates a temporary symbol for storage of intermediate data
        static public int GenSymbol(string symbolName, int value)
        {
            // Counter is increased each time to prevent symbol overlap
            // with generated symbols
            tempCounter++;
            // % is used to prevent accidental symbol overlap
            // with naturally occuring symbols
            return LexicalAnalyzer.symbolTable.AddSymbol("%" + symbolName, 2, value);
        }

        // Used to determine if the generated symbol needs to be changed to a float
        static public void RecognizeFloatPromotion(int op1Index, int op2Index, int tempIndex)
        {
            // If either of the operands is a float, promote temp to float
            if (LexicalAnalyzer.symbolTable.GetSymbol(op1Index).GetDataType() == DOUBLEDATATYPE ||
                LexicalAnalyzer.symbolTable.GetSymbol(op2Index).GetDataType() == DOUBLEDATATYPE)
            {
                LexicalAnalyzer.symbolTable.UpdateSymbolDataType(tempIndex, DOUBLEDATATYPE);
            }

        }

        public static void DoParse(string inputfie = "")
        {
            // Setup Lists to hold declaration and use data
            InitializeLists();
            // Setup DFA, Reserve tables
            LexicalAnalyzer.InitializeStructures();
            // Setup file for lexical and syntax analysis
            LexicalAnalyzer.InitializeInputFile(inputfie);

            programQuadTable.SetSymbolTable(LexicalAnalyzer.symbolTable);

            // Add symbols for basic operations
            GenSymbol("Minus", -1);
            GenSymbol("Plus", 1);
            // Add a symbol for for loop counting
            GenSymbol("ForLoopInc", 1);

            // Get the first available token before starting analysis
            GetAndValidateNextToken();
            // Call to the start non terminal
            ProgramStart();


            // Succsess condition: Nothing left to read from file
            // and no errors detected
            if (LexicalAnalyzer.EOF && !Error && !ErrorOccured && !UNDECLAREDERROR)
            {
                if (Warn)
                {
                    Console.WriteLine("Compilation Completed, no errors, one or more warnings");
                    Console.WriteLine("Error Count: " + ErrorCount);
                }
                else
                {
                    Console.WriteLine("Compilation Completed, no errors");
                    Console.WriteLine("Error Count" + ErrorCount);
                }
            }
            else // parsing completed, but found errors during parsing 
            {
                Console.WriteLine("Compilation Completed, with errors, will not execute");
                Console.WriteLine("Error Count: " + ErrorCount);
            }

            Console.WriteLine();

            DetectUnused(); // find and report unused labels and variables
            SyntaxUnusedWarning();
        }
        // Continue parsing file even though an error has occured
        // stop processesing current statement and move on to the next
        // available statement
        static void HandleErrors()
        {
            Console.WriteLine("Error Occured, proceeding");
            ErrorOccured = true;
            ErrorCount++;
            Error = false;
            FlushCurrentStatement();
            SkipTokensUntilStatement();
            Statement();
        }

        // Rids the current stack of the error causing statement
        static void FlushCurrentStatement()
        {
            if (traceOn)
            {
                Console.WriteLine("------FLUSHING------");
            }
            while (!IsSemicolon())
            {
                GetAndValidateNextToken();
            }
            if (traceOn)
            {
                Console.WriteLine("------FLUSHING------");
            }
        }

        // Moves past all non-statement possible tokens
        static void SkipTokensUntilStatement()
        {
            while (!IsStatement())
            {
                GetAndValidateNextToken();
            }
        }

        // Returns true if the current token is a semicolon
        static bool IsSemicolon()
        {
            if (CurrentCode() == "SCLN")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Used to display all unused variables and labels that have been declared
        static void DetectUnused()
        {
            foreach (string declaredVariable in declaredVariables)
            {
                if (!usedVariables.Contains(declaredVariable))
                {
                    unusedVariables.Add(declaredVariable);
                }
            }

            foreach (string declaredLabel in declaredLables)
            {
                if (!usedLabels.Contains(declaredLabel))
                {
                    unusedLabels.Add(declaredLabel);
                }
            }
        }

        // Returns true if the current token can start a statement
        static bool IsStatement()
        {
            if (CheckForLabel()
                || CurrentCode() == "IDTY"
                || CurrentCode() == "BEGN"
                || CurrentCode() == "IF__"
                || CurrentCode() == "WHILE"
                || CurrentCode() == "REPT"
                || CurrentCode() == "FOR_"
                || CurrentCode() == "GOTO"
                || CurrentCode() == "WRTL")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Reports general syntax error
        // Error
        static void SyntaxError(string expected, string got, bool specialError = false)
        {
            // Used for unique identifier re-use
            if (specialError == true)
            {
                Console.WriteLine("Error, Program unique identifier " + UNIQUEIDENTIFIER +
                    " used again.");
            }

            Console.WriteLine("Error at line: " + LexicalAnalyzer.currentLine +
                ", Expected token: " + expected + ", Got: " + got);
            // Global error code toggled to true
            Error = true;
        }

        // Reports when labels or variables are redeclared improperly
        // Error
        static void SyntaxDeclareError(bool labelError)
        {
            if (labelError)
            {
                Console.WriteLine("Error at line: " + LexicalAnalyzer.currentLine
                    + " Label attempted to be declared with same name as existing variable: " + LexicalAnalyzer.nextToken);
                Error = true;
            }
            else
            {
                Console.WriteLine("Error at line: " + LexicalAnalyzer.currentLine
                    + " Variable attempted to be declared with same name as existing label: " + LexicalAnalyzer.nextToken);
                Error = true;
            }
        }

        // Reports when labels or variables are used as the opposite type
        // Warning
        static void SyntaxWrongUseError(bool labelError)
        {
            if (labelError)
            {
                Console.WriteLine("Warning at line: " + LexicalAnalyzer.currentLine
                + " Label attempted to be used like variable: " + LexicalAnalyzer.nextToken);
                Warn = true; // Do not error out, just set global warn flag
            }
            else
            {
                Console.WriteLine("Warning at line: " + LexicalAnalyzer.currentLine
                + " variable attempted to be used like label: " + LexicalAnalyzer.nextToken);
                Warn = true;
            }
        }

        // Reports when a label or variable is used before it is declared
        // Warning
        static void SyntaxUndeclaredError(bool labelError)
        {
            if (labelError)
            {
                Console.WriteLine("/// WARN - UNDECLARED LABEL - WARN ///" + " " + LexicalAnalyzer.nextToken);
                Warn = true;
                UNDECLAREDERROR = true;
            }
            else
            {
                Console.WriteLine("/// WARN - UNDECLARED VARIABLE - WARN ///" + " " + LexicalAnalyzer.nextToken);
                Warn = true;
                UNDECLAREDERROR = true;
            }
        }

        // Reports when variables or labels have gone unused
        // Warning
        static void SyntaxUnusedWarning()
        {
            if (unusedLabels.Count > 0)
            {
                Console.WriteLine("Unused labels: ");
                foreach (string label in unusedLabels)
                {
                    Console.Write(label + ", ");
                }
                Warn = true;
                Console.WriteLine();
            }

            if (unusedVariables.Count > 0)
            {
                Console.WriteLine("\nUnused Variables: ");
                foreach (string variable in unusedVariables)
                {
                    Console.Write(variable + ", ");
                }
                Warn = true;
                Console.WriteLine();
            }
        }

        // Gets the mnemonic associated with the currernt token code for comparison
        static string CurrentCode()
        {
            return LexicalAnalyzer.CodeTable.LookupCode(LexicalAnalyzer.tokenCode);
        }

        // Start non terminal
        // All non terminals follow the same pattern.
        // Check for error condition, display enter message,
        // attempt to match tokens, call next non terminal,
        // raise error condition if no desired token found,
        // return and display exit message when finished.
        // Additional comments provided in unique cases
        static int ProgramStart()
        {
            if (Error) // If there are any errors, don't continue
            {
                return -1; // Immediately exit
            }

            if (traceOn) // Displays enter message for current non terminal
            {
                Debug(true, "Program");
            }

            // Ensure current token matches expected token
            // Use CurrentCode() to match the mnemonic of the token code
            if (CurrentCode() == "UNIT")
            {
                GetAndValidateNextToken();  // Ready next token for comparison
                ProgIdentifier(); // Call next non terminal
                if (CurrentCode() == "SCLN")
                {
                    GetAndValidateNextToken();
                    Block();
                    if (CurrentCode() == "PERD")
                    {
                        GetAndValidateNextToken();
                    }
                    else
                    {
                        SyntaxError("Period", LexicalAnalyzer.nextToken);
                    }
                }
                else // If unable to find expected token, return a syntax error
                {
                    SyntaxError("Semicolon", LexicalAnalyzer.nextToken);
                }
            }
            else // If unable to find expected token, return a syntax error
            {
                SyntaxError("UNIT", LexicalAnalyzer.nextToken);
            }

            if (traceOn) // Displays exit message for current non terminal
            {
                Debug(false, "Program");
            }

            programQuadTable.AddQuad(quadStopCode, 0, 0, 0);

            return 0;
        }

        static int Block()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Block");
            }

            if (CurrentCode() == "LABL")
            {
                LabelDeclaration();
            }

            if (CurrentCode() == "VAR_")
            {
                while (CurrentCode() == "VAR_" && !Error)
                {
                    VariableDeclarationSection();
                }
            }

            BlockBody();

            if (traceOn)
            {
                Debug(false, "Block");
            }

            return 0;
        }

        static int BlockBody()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "BlockBody");
            }

            if (CurrentCode() == "BEGN")
            {
                GetAndValidateNextToken();
                Statement();

                if (Error)
                {
                    HandleErrors();
                }

                // Multiple statements are supported so
                // continue to get more statements while statement semicolon pairs exist
                // Some errors leave us without a semicolon but with a need to continue
                // force is used in these cases
                while (CurrentCode() == "SCLN" || force)
                {
                    force = false;

                    if (Error)
                    {
                        HandleErrors();
                    }
                    else
                    {
                        GetAndValidateNextToken();
                        Statement();
                    }

                    if (CurrentCode() != "SCLN" && CurrentCode() != "END_")
                    {
                        SyntaxError(";", LexicalAnalyzer.nextToken);
                        force = true;
                    }
                }
                if (CurrentCode() == "END_")
                {
                    GetAndValidateNextToken();
                }
                else
                {
                    SyntaxError("End or Semicolon", LexicalAnalyzer.nextToken);
                }
            }
            else
            {
                SyntaxError("Begin", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "BlockBody");
            }
            return 0;
        }

        static int LabelDeclaration()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "LabelDeclaration");
            }

            if (CurrentCode() == "LABL")
            {
                GetAndValidateNextToken();
                isLabelDecleration = true;
                Identifier();
                isLabelDecleration = false;
                while (CurrentCode() == "CMMA" && !Error)
                {
                    GetAndValidateNextToken();
                    isLabelDecleration = true;
                    Identifier();
                    isLabelDecleration = false;
                }
                if (CurrentCode() == "SCLN")
                {
                    GetAndValidateNextToken();
                }
                else
                {
                    SyntaxError(";", LexicalAnalyzer.nextToken);
                }
            }
            else
            {
                SyntaxError("Label", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "LabelDeclaration");
            }

            return 0;
        }

        static int VariableDeclarationSection()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "VariableDeclarationSection");
            }

            if (CurrentCode() == "VAR_")
            {
                GetAndValidateNextToken();
                VariableDeclaration();
            }
            else
            {
                SyntaxError("Var", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "VariableDeclarationSection");
            }

            return 0;
        }

        static int VariableDeclaration()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "VariableDeclaration");
            }

            indexesToUpdateType.Clear();
            int returnedType = -1;
            // Needs at least one variable
            if (CurrentCode() == "IDTY")
            {
                while (CurrentCode() == "IDTY" && !Error)
                {
                    isVariableDecleration = true;
                    Identifier();
                    isVariableDecleration = false;
                    while (CurrentCode() == "CMMA" && !Error)
                    {
                        GetAndValidateNextToken();
                        isVariableDecleration = true;
                        Identifier();
                        isVariableDecleration = false;
                    }
                    if (CurrentCode() == "COLN")
                    {
                        GetAndValidateNextToken();
                        returnedType = Type_();

                        foreach (int index in indexesToUpdateType)
                        {
                            LexicalAnalyzer.symbolTable.UpdateSymbolDataType(index, returnedType);
                        }

                        if (CurrentCode() == "SCLN")
                        {
                            GetAndValidateNextToken();
                        }
                        else
                        {
                            SyntaxError(";", LexicalAnalyzer.nextToken);
                        }
                    }
                    else
                    {
                        SyntaxError(":", LexicalAnalyzer.nextToken);
                    }
                }
            }
            else
            {
                SyntaxError("Identifier", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "VariableDeclaration");
            }

            return 0;
        }

        static int ProgIdentifier()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "ProgIdentifier");
            }

            // Used during <identifier> call 
            // to identify this token as the program identifier
            isProgIdentifier = true;

            // Each program can only have 1 unique identifier 
            // so its lexime is stored here
            UNIQUEIDENTIFIER = LexicalAnalyzer.nextToken;
            Identifier();

            isProgIdentifier = false;

            if (traceOn)
            {
                Debug(false, "ProgIdentifier");
            }
            return 0;
        }

        static int Statement()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Statement");
            }

            // General use indexes into symbol table
            // Assigned as left and right operands during code generation
            int left;
            int right;

            // The following are indexes into the symbol table
            int labelSymbolIndex; // Used for label code generation
            int branchTarget; // Used for branch exit conditions
            int branchQuad; // Used for branch exit conditions
            int saveTop; // Used to indicate the top of a loop
            int patchElse; // Used to update else condition in if-else
            int forLoopVarIndex; // Used to store the variable used for for loop incremet
            int conditionIndex; // Used to store for loop comparison value
            int printTarget; // Used to store what needs to be printed

            if (CheckForLabel())
            {
                while (CheckForLabel() && !Error)
                {
                    labelSymbolIndex = Label(); // Store where the label is in the symbol table

                    // Make sure that the symbol table value for the label has the correct quad location
                    LexicalAnalyzer.symbolTable.UpdateSymbolIntvalue(labelSymbolIndex,
                        programQuadTable.NextQuad());

                    // IT IS POSSIBLE TO HAVE SEEN THE GOTO COMMAND BEFORE YOU EVER SEE THE LABEL USED
                    // IN THIS CASE IT IS NECESSARY TO UPDATE THE QUAD THAT HAS ALREADY BEEN ADDED TO
                    // THE QUAD TABLE BY 'GOTO'
                    if (gotoLabelIndexToUpdate != -1)
                    {
                        // Update the destination of the goto that was added earlier
                        programQuadTable.SetQuadOp3(gotoLabelIndexToUpdate,
                            LexicalAnalyzer.symbolTable.GetSymbolValue(labelSymbolIndex));
                        gotoLabelIndexToUpdate = -1;
                    }

                    if (CurrentCode() == "COLN")
                    {
                        GetAndValidateNextToken();
                    }
                    else
                    {
                        SyntaxError(":", LexicalAnalyzer.nextToken);
                    }
                }
            }

            if (CurrentCode() == "IDTY")
            {
                left = Variable();
                if (CurrentCode() == "BCMS")
                {
                    GetAndValidateNextToken();
                    // Must be one of the two
                    if (CurrentCode() == "PLUS"
                        || CurrentCode() == "MINS"
                        || CurrentCode() == "NCNI"
                        || CurrentCode() == "NCNF"
                        || CurrentCode() == "IDTY"
                        || CurrentCode() == "LPRN")
                    {
                        right = SimpleExpression();
                        programQuadTable.AddQuad(quadMovCode, right, 0, left);
                    }
                    else if (CurrentCode() == "SCN_")
                    {
                        right = StringConst();
                        programQuadTable.AddQuad(quadMovCode, right, 0, left);
                    }
                    else
                    {
                        SyntaxError("SimpleExpression OR StringLiteral", LexicalAnalyzer.nextToken);
                    }
                }
            }
            else if (CurrentCode() == "BEGN")
            {
                BlockBody();
            }
            else if (CurrentCode() == "IF__")
            {
                GetAndValidateNextToken();
                branchQuad = Relexpression(); //Allows to jump around the true condition
                if (CurrentCode() == "THEN")
                {
                    GetAndValidateNextToken();
                    Statement(); // Generates interior quads for true evaluation
                    if (CurrentCode() == "ELSE") //Optional
                    {
                        GetAndValidateNextToken();
                        patchElse = programQuadTable.NextQuad(); // Save quad to jump around else

                        programQuadTable.AddQuad(quadBRCode, 0, 0, 0); // For use with false condition
                        programQuadTable.SetQuadOp3(branchQuad, 
                            programQuadTable.NextQuad()); // conditional jump

                        Statement(); // Generates else quads

                        programQuadTable.SetQuadOp3(patchElse, 
                            programQuadTable.NextQuad());  // Update else to point to proper location
                    }
                    else
                    {
                        // Used if no else encountered to fix the if condition
                        programQuadTable.SetQuadOp3(branchQuad,
                            programQuadTable.NextQuad());
                    }
                }
                else
                {
                    SyntaxError("THEN", LexicalAnalyzer.nextToken);
                }
            }
            else if (CurrentCode() == "WHLE")
            {
                GetAndValidateNextToken();
                saveTop = programQuadTable.NextQuad(); // Indicates the quad at the top of the loop
                branchQuad = Relexpression(); // Gets the quad that will be used as the branch target
                if (CurrentCode() == "DO__")
                {
                    GetAndValidateNextToken();
                    Statement(); // Generates quads in the loop body
                    programQuadTable.AddQuad(quadBRCode, 0, 0, saveTop); // Jump to top of loop

                    programQuadTable.SetQuadOp3(branchQuad, programQuadTable.NextQuad()); // Points out of while
                }
                else
                {
                    SyntaxError("DO", LexicalAnalyzer.nextToken);
                }
            }
            else if (CurrentCode() == "REPT")
            {
                GetAndValidateNextToken();
                branchTarget = programQuadTable.NextQuad(); // Save location of code to repeat
                Statement(); // Generate quads for code to repeat
                if (CurrentCode() == "UNTL")
                {
                    GetAndValidateNextToken();
                    branchQuad = Relexpression();  // Retunrs the quad to be used in conditional jump
                    programQuadTable.SetQuadOp3(branchQuad, branchTarget); // used to jump back up if needed
                }
                else
                {
                    SyntaxError("UNTIL", LexicalAnalyzer.nextToken);
                }
            }
            else if (CurrentCode() == "FOR_")
            {
                int forloopassign; // Used to store constant used for initial assignment
                int compareLeft; // Used to compare the current indexer value to the comparison value
                int compareRight; // Used to compare the current indexeer value to the comparison value
                int compareTemp; // Temp variable used to store intermediate data (math operations)

                GetAndValidateNextToken();
                forLoopVarIndex = Variable(); // the index of the indexer the "i" in "for i ="
                if (CurrentCode() == "BCMS")
                {
                    GetAndValidateNextToken();
                    forloopassign = SimpleExpression(); // The initial value to be places in the indexer
                    programQuadTable.AddQuad(quadMovCode, forloopassign, 0, forLoopVarIndex);  // Does the assign

                    if (CurrentCode() == "TO__")
                    {
                        GetAndValidateNextToken();
                        conditionIndex = SimpleExpression(); // The value to compare against the indexer
                        if (CurrentCode() == "DO__")
                        {
                            saveTop = programQuadTable.NextQuad(); // Indicates the top of the for loop

                            // This is the comparison to be done each time the loop iterates
                            compareLeft = forLoopVarIndex;
                            compareRight = conditionIndex;
                            compareTemp = GenSymbol("ForLooptemp" + tempCounter, 0);

                            // If either operand is a double, promote the temp variable
                            RecognizeFloatPromotion(compareLeft, compareRight, compareTemp);

                            // Do the comparison
                            programQuadTable.AddQuad(quadSubCode, compareLeft, compareRight, compareTemp);
                            branchQuad = programQuadTable.NextQuad();
                            programQuadTable.AddQuad(quadBPCode, compareTemp, 0, 0); // Used to stay in or exit the loop

                            GetAndValidateNextToken();
                            Statement(); // Generates quads inside the loop

                            // Handles the for loop incrementation
                            programQuadTable.AddQuad(quadAddCode, forLoopVarIndex,
                                LexicalAnalyzer.symbolTable.LookupSymbol("%ForLoopInc"), forLoopVarIndex);

                            // Branch back to the top of the loop
                            programQuadTable.AddQuad(quadBRCode, 0, 0, saveTop);
                            // Update the exit loop branch to point outside of the loop
                            programQuadTable.SetQuadOp3(branchQuad, programQuadTable.NextQuad());
                        }
                        else
                        {
                            SyntaxError("DO", LexicalAnalyzer.nextToken);
                        }
                    }
                    else
                    {
                        SyntaxError("TO", LexicalAnalyzer.nextToken);
                    }
                }
                else
                {
                    SyntaxError(":=", LexicalAnalyzer.nextToken);
                }
            }
            else if (CurrentCode() == "GOTO")
            {
                GetAndValidateNextToken();
                left = Label();  // Returns the index of the labal
                // Jumps to the quad index stored as the value of the label in the symbol table
                gotoLabelIndexToUpdate = programQuadTable.AddQuad(quadBRCode, 0, 0, 
                    LexicalAnalyzer.symbolTable.GetSymbolValue(left));
            }
            else if (CurrentCode() == "WRTL")
            {
                GetAndValidateNextToken();
                if (CurrentCode() == "LPRN")
                {
                    GetAndValidateNextToken();
                    // Must be one of the two
                    if (CurrentCode() == "PLUS"
                        || CurrentCode() == "MINS"
                        || CurrentCode() == "NCNI"
                        || CurrentCode() == "NCNF"
                        || CurrentCode() == "IDTY"
                        || CurrentCode() == "LPRN")
                    {
                        printTarget = SimpleExpression();  // Returns the index of the expression to print
                        programQuadTable.AddQuad(quadPrintCode, printTarget, 0, 0); // Prints
                    }
                    else if (CurrentCode() == "SCN_")
                    {
                        printTarget = StringConst(); // Returns the index of the string to print
                        programQuadTable.AddQuad(quadPrintCode, printTarget, 0, 0); // Prints
                    }
                    else
                    {
                        SyntaxError("SimpleExpression OR StringLiteral", LexicalAnalyzer.nextToken);
                    }
                    if (CurrentCode() == "RPRN")
                    {
                        GetAndValidateNextToken();
                    }
                    else
                    {
                        SyntaxError(")", LexicalAnalyzer.nextToken);
                    }
                }
                else
                {
                    SyntaxError("(", LexicalAnalyzer.nextToken);
                }
            }
            else
            {
                SyntaxError(
                    "Variable, BlockBody, if, while, repeat, for," +
                    "goto or writeline", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "Statement");
            }
            return 0;
        }

        // Returns variable symbol table index
        static int Variable()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Variable");
            }

            int result = -1;

            // Identifies this identifier as a variable
            if (CheckForVariable())
            {
                isVariable = true;
                result = Identifier();
                isVariable = false;
            }
            else
            {
                SyntaxWrongUseError(false);
            }

            if (CurrentCode() == "LBRK")
            {
                GetAndValidateNextToken();
                SimpleExpression();
                if (CurrentCode() == "RBRK")
                {
                    GetAndValidateNextToken();
                }
                else
                {
                    SyntaxError("]", LexicalAnalyzer.nextToken);
                }
            }

            if (traceOn)
            {
                Debug(false, "Variable");
            }
            return result;
        }


        // Returns label symbol table index
        static int Label()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Label");
            }

            int result = -1;

            // Identifies this identifier as a label
            if (CheckForLabel())
            {
                isLabel = true;
                result = Identifier();
                isLabel = false;
            }
            else
            {
                SyntaxWrongUseError(true);
            }

            if (traceOn)
            {
                Debug(false, "Label");
            }

            return result;
        }

        // Returns a quad with the proper branch expression
        static int Relexpression()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Relexpression");
            }

            int left;
            int right;
            int saveRelop;
            int result;
            int temp;

            // Store the indexes for the left "a"
            // relop "<" etc.
            // right "b"
            left = SimpleExpression();
            saveRelop = Relop();
            right = SimpleExpression();

            // Temporary variable used for storing the result
            temp = GenSymbol("temp" + tempCounter, 0);


            // Promote the temp to a float if necessary
            RecognizeFloatPromotion(left, right, temp);

            // Subtract right from left "a-b" and store in temp
            programQuadTable.AddQuad(quadSubCode, left, right, temp);
            result = programQuadTable.NextQuad();

            // Get the proper relop code and add the quad for branching
            programQuadTable.AddQuad(RelopToOpcode(saveRelop), temp, 0, 0);

            if (traceOn)
            {
                Debug(false, "Relexpression");
            }

            return result;
        }

        // Returns the proper quad code for the desired relational expression
        static int RelopToOpcode(int relop)
        {
            switch (relop)
            {
                case relopEqual:
                    return quadBNZCode;
                case relopNotEqual:
                    return quadBZCode;
                case relopLess:
                    return quadBNNCode;
                case relopGreater:
                    return quadBNPCode;
                case relopLessEqual:
                    return quadBPCode;
                case relopGreaterEqual:
                    return quadBNCode;
                default:
                    return -1;
            }
        }

        // Returns the relop symbol code for the given token
        static int Relop()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Relop");
            }

            int result = -1;

            if (CurrentCode() == "EQUL"
                || CurrentCode() == "LEST"
                || CurrentCode() == "GRTR"
                || CurrentCode() == "EQQL"
                || CurrentCode() == "LSTE"
                || CurrentCode() == "GRTE")
            {
                if (CurrentCode() == "EQUL")
                {
                    result = relopEqual;
                }
                else if (CurrentCode() == "EQQL")
                {
                    result = relopNotEqual;
                }
                else if (CurrentCode() == "LEST")
                {
                    result = relopLess;
                }
                else if (CurrentCode() == "GRTR")
                {
                    result = relopGreater;
                }
                else if (CurrentCode() == "LSTE")
                {
                    result = relopLessEqual;
                }
                else if (CurrentCode() == "GRTE")
                {
                    result = relopGreaterEqual;
                }
                GetAndValidateNextToken();
            }
            else
            {
                SyntaxError(
                    "=, <, >, <>, <=, >=", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "Relop");
            }

            return result;
        }


        // Returns the simble table index for the current simple expression
        static int SimpleExpression()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Simple Expression");
            }

            // Symbol table indexes for comparison
            int left = 0;
            int right = 0;
            int signval = 0;
            int temp = 0;
            int opcode = 0;

            // If a <sign> exists here we must take that path
            // Do not consume input, let the <sign> handle Get Next Token
            // Non terminals handle token error checking
            if (CurrentCode() == "PLUS" || CurrentCode() == "MINS")
            {
                // If sign is '-' we need to multiply whatever the next expression is by '-1'
                signval = Sign(); // Symbol table index, 0 for '-1' or 1 for '1'
                // We are interested in the value of the index stored in signval
                signval = LexicalAnalyzer.symbolTable.GetSymbolValue(signval);

                // Since there can be more than one term, left will continue to hold
                // the current index to the evaluated simple expression
                left = Term();

                // Multiply by '-1' if the sign was a '-'
                if (signval == -1)
                {
                    programQuadTable.AddQuad(quadMulCode, left,
                        LexicalAnalyzer.symbolTable.LookupSymbol("%Minus"), left);
                }

                // Let <addop> handle get next token
                while (CurrentCode() == "PLUS" || CurrentCode() == "MINS")
                {
                    // Same idea as with the sign above
                    signval = Addop();
                    signval = LexicalAnalyzer.symbolTable.GetSymbolValue(signval);
                    
                    // Determines which quad code to use '+' or '-'
                    if (signval == 1)
                    {
                        opcode = quadAddCode;
                    }
                    else
                    {
                        opcode = quadSubCode;
                    }
                    right = Term(); // Stores the right term
                    temp = GenSymbol("temp" + tempCounter, 0); // Creates a temp variable for intermediate data
                    RecognizeFloatPromotion(left, right, temp); // Promote if necessary
                    programQuadTable.AddQuad(opcode, left, right, temp); // Store the value of the operation
                    left = temp; // Store the current result in left, because we might need to evaluate more
                }
            }
            else // Same as above just no sign
            {
                left = Term();
                while (CurrentCode() == "PLUS" || CurrentCode() == "MINS")
                {
                    signval = Addop();
                    signval = LexicalAnalyzer.symbolTable.GetSymbolValue(signval);

                    if (signval == 1)
                    {
                        opcode = quadAddCode;
                    }
                    else
                    {
                        opcode = quadSubCode;
                    }
                    right = Term();
                    temp = GenSymbol("temp" + tempCounter, 0);
                    RecognizeFloatPromotion(left, right, temp);
                    programQuadTable.AddQuad(opcode, left, right, temp);
                    left = temp;
                }
            }

            if (traceOn)
            {
                Debug(false, "Simple Expression");
            }
            return left;
        }

        // Retuns symbol table index 0 or 1, 0 is - 1 is +
        static int Addop()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Addop");
            }

            int result = -1;
            if (CurrentCode() == "PLUS" || CurrentCode() == "MINS")
            {
                if (CurrentCode() == "PLUS")
                {
                    result = LexicalAnalyzer.symbolTable.LookupSymbol("%Plus"); // 1
                }
                else
                {
                    result = LexicalAnalyzer.symbolTable.LookupSymbol("%Minus"); // 0
                }
                GetAndValidateNextToken();
            }
            else
            {
                SyntaxError("+ or -", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "Addop");
            }
            return result;
        }

        // Returns symbol table index 0 or 1, 0 is - 1 is +
        static int Sign()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Sign");
            }

            int result = -1;

            if (CurrentCode() == "PLUS" || CurrentCode() == "MINS")
            {
                if (CurrentCode() == "PLUS")
                {
                    result = LexicalAnalyzer.symbolTable.LookupSymbol("%Plus");
                }
                else
                {
                    result = LexicalAnalyzer.symbolTable.LookupSymbol("%Minus");
                }
                GetAndValidateNextToken();
            }
            else
            {
                SyntaxError("+ or -", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "Sign");
            }
            return result;
        }

        static int Term()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Term");
            }

            int left = 0;
            int right = 0;
            int temp = 0;
            int opcode = 0;

            left = Factor(); // There can be more than one factor so left will store the current total term

            // Let <mulop> handle Get Next Token and error checking
            while ((CurrentCode() == "STAR" || CurrentCode() == "SLSH") && !Error)
            {
                // Set opcode for multiplication or division
                if (CurrentCode() == "STAR")
                {
                    opcode = quadMulCode;
                }
                else
                {
                    opcode = quadDivCode;
                }
                Mulop();
                right = Factor();  // Store the value of the factor on the right
                temp = GenSymbol("temp" + tempCounter, 0);  // Temp symbol to hold intermediate value

                RecognizeFloatPromotion(left, right, temp); // Promote if necessary

                programQuadTable.AddQuad(opcode, left, right, temp); // Do the multiplication or division
                left = temp; // store the total in left, keep going if needed
            }

            if (traceOn)
            {
                Debug(false, "Term");
            }
            return left;
        }

        static int Mulop()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Mulop");
            }

            if (CurrentCode() == "STAR" || CurrentCode() == "SLSH")
            {
                GetAndValidateNextToken();
            }
            else
            {
                SyntaxError("* or /", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "Mulop");
            }
            return 0;
        }


        // Returns the index of the current factor
        static int Factor()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Factor");
            }

            int result = -1;

            // Determine if we should take the <unsigned costant> path
            // by checking if the current token is an unsigned constant.
            // Let <unsigned constant> handle Get Next Token and error handling
            if (CurrentCode() == "NCNF" || CurrentCode() == "NCNI")
            {
                result = UnsignedConstant();
            }
            // Determine if we should take the <variable> path
            else if (CurrentCode() == "IDTY")
            {
                result = Variable();
            }
            else if (CurrentCode() == "LPRN") // Otherwise check for parethesis case
            {
                GetAndValidateNextToken();
                result = SimpleExpression();
                if (CurrentCode() == "RPRN")
                {
                    GetAndValidateNextToken();
                }
                else
                {
                    SyntaxError(")", LexicalAnalyzer.nextToken);
                }
            }
            else // If no conditions are met return error
            {
                SyntaxError("(, Unsigned Constant or Variable Name",
                    LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "Factor");
            }
            return result;
        }


        // Returns the type value recognized by symbols
        // Return value is used to update the type of all variables being declared
        static int Type_()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Type");
            }

            int result = -1;

            if (CurrentCode() == "INTG"
                || CurrentCode() == "FLT_"
                || CurrentCode() == "STR_")
            {
                if (CurrentCode() == "INTG")
                {
                    result = 0;
                }
                else if (CurrentCode() == "FLT_")
                {
                    result = 1;
                }
                else if (CurrentCode() == "STR_")
                {
                    result = 2;
                }
                SimpleType();
            }
            else if (CurrentCode() == "ARRY")
            {
                GetAndValidateNextToken();
                if (CurrentCode() == "LBRK")
                {
                    GetAndValidateNextToken();
                    if (CurrentCode() == "NCNI")
                    {
                        GetAndValidateNextToken();
                        if (CurrentCode() == "RBRK")
                        {
                            GetAndValidateNextToken();
                            if (CurrentCode() == "OF__")
                            {
                                GetAndValidateNextToken();
                                if (CurrentCode() == "INTG")
                                {
                                    GetAndValidateNextToken();
                                }
                                else
                                {
                                    SyntaxError("INTEGER", LexicalAnalyzer.nextToken);
                                }
                            }
                            else
                            {
                                SyntaxError("OF", LexicalAnalyzer.nextToken);
                            }
                        }
                        else
                        {
                            SyntaxError("]", LexicalAnalyzer.nextToken);
                        }
                    }
                    else
                    {
                        SyntaxError("IntType", LexicalAnalyzer.nextToken);
                    }
                }
                else
                {
                    SyntaxError("[", LexicalAnalyzer.nextToken);
                }
            }
            else
            {
                SyntaxError("INTEGER, FLOAT, STRING, ARRAY", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "Type");
            }

            return result;
        }

        static int SimpleType()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "SimpleType");
            }

            if (CurrentCode() == "INTG"
                || CurrentCode() == "FLT_"
                || CurrentCode() == "STR_")
            {
                GetAndValidateNextToken();
            }
            else
            {
                SyntaxError("INTEGER, FLOAT, or STRING", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "SimpleType");
            }

            return 0;
        }

        static int Constant()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Constant");
            }

            if (CurrentCode() == "PLUS" || CurrentCode() == "MINS")
            {
                Sign();
            }

            UnsignedConstant();

            if (traceOn)
            {
                Debug(false, "Constant");
            }

            return 0;
        }

        // Returns the index of the current unsigned constant
        static int UnsignedConstant()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Unsigned Constant");
            }

            int result = -1;

            result = UnsignedNumber();

            if (traceOn)
            {
                Debug(false, "Unsigned Constant");
            }
            return result;
        }

        // Returns the index of the current unsigned number
        static int UnsignedNumber()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Unsigned Number");
            }

            int result = -1;

            if (CurrentCode() == "NCNF" || CurrentCode() == "NCNI")
            {
                result = LexicalAnalyzer.positionInTable;
                GetAndValidateNextToken();
            }
            else
            {
                SyntaxError("Float or Integer", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "Unsigned Number");
            }
            return result;
        }

        // Returns the index of the current identifier
        static int Identifier()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "Identifier");
            }

            int result = -1;
            int indexToAdd;
            // Catch unique identifier being used again
            if (LexicalAnalyzer.nextToken == UNIQUEIDENTIFIER && !isProgIdentifier)
            {
                SyntaxError("Identifier", LexicalAnalyzer.nextToken, true);
            }

            // Ensure that an label's kind value is set to label
            if (CurrentCode() == "IDTY" && isLabelDecleration)
            {
                SetToLabel();
            }

            // Checks for attempts to use a variable with no declaration
            if (isVariable)
            {
                if (!declaredVariables.Contains(LexicalAnalyzer.nextToken))
                {
                    SyntaxUndeclaredError(false);
                    declaredVariables.Add(LexicalAnalyzer.nextToken);
                }
            }

            // Checks for attempts to use a label with no declaration
            if (isLabel)
            {
                if (!declaredLables.Contains(LexicalAnalyzer.nextToken))
                {
                    SyntaxUndeclaredError(true);
                    declaredLables.Add(LexicalAnalyzer.nextToken);
                }
            }

            // Checks for attempts to redeclare a variable as a label
            if (isLabelDecleration)
            {
                if (declaredVariables.Contains(LexicalAnalyzer.nextToken))
                {
                    SyntaxDeclareError(true);
                }
                else
                {
                    // Add the token to the list of labels
                    declaredLables.Add(LexicalAnalyzer.nextToken);
                }
            }

            // Checks for attempts to redeclare a label as a variable
            if (isVariableDecleration)
            {
                if (declaredLables.Contains(LexicalAnalyzer.nextToken))
                {
                    SyntaxDeclareError(false);
                }
                else
                {
                    declaredVariables.Add(LexicalAnalyzer.nextToken);
                }
            }

            // Variables are defualt declared as strings,
            // We dont know what the type is until we see ": INTEGER" etc.
            // All variables declared on one line have one type
            // Save each variable to be updated once we know what that type is
            if (isVariableDecleration)
            {
                indexToAdd = LexicalAnalyzer.positionInTable;
                indexesToUpdateType.Add(indexToAdd);
            }

            // Keeps track of which lables and variables have been used
            if (isLabel)
            {
                usedLabels.Add(LexicalAnalyzer.nextToken);
            }
            if (isVariable)
            {
                usedVariables.Add(LexicalAnalyzer.nextToken);
            }

            if (CurrentCode() == "IDTY")
            {
                result = LexicalAnalyzer.positionInTable;
                GetAndValidateNextToken();
            }
            else
            {
                SyntaxError("Identifier", LexicalAnalyzer.nextToken);
            }

            //LexicalAnalyzer.symbolTable.PrintSymbolTable();

            if (traceOn)
            {
                Debug(false, "Identifier");
            }
            return result;
        }


        // Returns the index of the current string constant
        static int StringConst()
        {
            if (Error)
            {
                return -1;
            }

            if (traceOn)
            {
                Debug(true, "StringConst");
            }

            int result = -1;

            if (CurrentCode() == "SCN_")
            {
                result = LexicalAnalyzer.positionInTable;
                //result =  LexicalAnalyzer.symbolTable.AddSymbol("StringConst" + StringConstCounter, 2, LexicalAnalyzer.nextToken);

                GetAndValidateNextToken();
            }
            else
            {
                SyntaxError("String Constant", LexicalAnalyzer.nextToken);
            }

            if (traceOn)
            {
                Debug(false, "StringConst");
            }

            return result;
        }

        // Used to indicate when a non terminal is entered and exited
        // Entering indicates whether it is entering or exiting
        // Name is the name of the non terminal
        static void Debug(bool entering, string name)
        {
            if (entering)
            {
                WriteSpaces(true);
                Console.WriteLine("ENTERING: " + name);
            }
            else
            {
                WriteSpaces(false);
                Console.WriteLine("Exiting: " + name);
            }
        }

        // Manages indenting
        static void WriteSpaces(bool up)
        {
            if (up)
            {
                numberOfSpaces++; // Manages Indenting
                for (int i = 0; i < numberOfSpaces; i++)
                {
                    Console.Write(" ");
                }
            }
            else
            {
                for (int i = 0; i < numberOfSpaces; i++)
                {
                    Console.Write(" ");
                }
                numberOfSpaces--;  // Managed Un-Indenting
            }
        }

        // Used to ensure that label declarations have the proper "kind" value
        static void SetToLabel()
        {
            LexicalAnalyzer.symbolTable.GetSymbol(
                LexicalAnalyzer.symbolTable.LookupSymbol(
                    LexicalAnalyzer.nextToken)).SetKind(0);
            LexicalAnalyzer.symbolTable.GetSymbol(
                LexicalAnalyzer.symbolTable.LookupSymbol(
                    LexicalAnalyzer.nextToken)).SetDataType(0);
        }

        // Determines if the current token is defined as a variable in the symbol table
        static bool CheckForVariable()
        {
            int test2 = -1;
            Symbol test = LexicalAnalyzer.symbolTable.GetSymbol(
                LexicalAnalyzer.symbolTable.LookupSymbol
                (LexicalAnalyzer.nextToken));

            if (test != null)
            {
                test2 = test.GetKind();
            }

            if (test2 == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Checks if the current token is defined as a label in the symbol table
        static bool CheckForLabel()
        {
            int test2 = -1;
            Symbol test = LexicalAnalyzer.symbolTable.GetSymbol(
                LexicalAnalyzer.symbolTable.LookupSymbol
                (LexicalAnalyzer.nextToken));

            if (test != null)
            {
                test2 = test.GetKind();
            }

            if (test2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Initializes all data structures used for declaration/use/unuse
        static void InitializeLists()
        {
            programQuadTable.Initialize(1000);

            indexesToUpdateType = new List<int>();

            declaredLables = new List<string>();
            declaredVariables = new List<string>();

            usedLabels = new List<string>();
            usedVariables = new List<string>();

            unusedLabels = new List<string>();
            unusedVariables = new List<string>();
        }

        // Returns next valid token from the lexical analyzer from the input file
        // Verbose is used to display each lexeme and mnemonic
        // Extraverbose is used to display source lines
        static void GetAndValidateNextToken()
        {
            LexicalAnalyzer.nextToken = LexicalAnalyzer.GetNextToken(extraverbose);

            // On invalid token, continue to try to get next token
            // Will not infinite loop as lexical analyzer will eventually reach End Of File
            while ((LexicalAnalyzer.nextToken == null ||
                LexicalAnalyzer.tokenCode == 999) &&
                !LexicalAnalyzer.EOF)
            {
                LexicalAnalyzer.nextToken = LexicalAnalyzer.GetNextToken(extraverbose);
                if (LexicalAnalyzer.nextToken != null && LexicalAnalyzer.tokenCode != 999)
                {
                    break;
                }
            }

            // Display token and lexeme information
            if (LexicalAnalyzer.nextToken != null && verbose == true)
            {
                LexicalAnalyzer.PrintToken
                    (LexicalAnalyzer.nextToken, LexicalAnalyzer.tokenCode);
            }
        }
    }
}
