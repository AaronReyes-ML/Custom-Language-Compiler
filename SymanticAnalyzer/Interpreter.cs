using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymanticAnalyzer
{
    class Interpreter
    {
        // Used to interpret QuadTable data.  Requires access to a SymbolTable
        // Use TraceOn to see current PC state and instruction details
        public static void IntepretQuads(QuadTable Q, SymbolTable S, bool TraceOn)
        {

            Console.WriteLine("---------------------------EXECUTING---------------------------\n");
            // Create a pc, opcode, op1, op2 and op3 variable to store
            // information about the current instruction
            int pc = 0;
            int opcode, op1, op2, op3;
            // Set the max amount of Quads, and therefore the max pc
            int MAXQUAD = 1000;

            // If in tracemode, the interpreter output will include pc and instruction details
            if (TraceOn)
            {
                Console.WriteLine("Trace On");
            }

            // While there are still quads to execute
            while (pc < MAXQUAD)
            {
                //SyntaxAnalyzer.programQuadTable.PrintQuadTable();
                //LexicalAnalyzer.symbolTable.PrintSymbolTable();
                // Get the next quad to evaluate by checking the QuadTable for index pc
                Quad nextQuad = Q.GetQuad(pc);
                // Parse out the data from the quad and store it in the variables defined earlier
                opcode = nextQuad.GetOpcode();
                op1 = nextQuad.GetOp1();
                op2 = nextQuad.GetOp2();
                op3 = nextQuad.GetOp3();

                // Print out pc and instruction details
                if (TraceOn)
                {
                    // Print out the name of the opcode rather than its associated code
                    Console.WriteLine("PC = " + pc.ToString().PadLeft(4, '0')
                        + ":" + Q.GetMnemonic(opcode).PadLeft(5, ' ') + " " + op1.ToString().PadLeft(5, ' ') 
                        + op2.ToString().PadLeft(5, ' ') + op3.ToString().PadLeft(5, ' '));
                }

                // Check to see if the opcode corresponds to a valid reserve word in the reserve table
                // to make sure that the case statement can parse it
                if (Q.GetMnemonic(opcode) != "")
                {
                    // Switch statement that determines which process is executed based on the opcode
                    switch (opcode)
                    {
                        case 0: // Stop
                            Console.WriteLine("Execution terminated by program stop.");
                            pc = MAXQUAD;
                            break;
                        case 1:  //DIV auto promote
                            int anyFloat = 0;

                            if (S.GetSymbol(op1).GetDataType() == 1 ||
                                S.GetSymbol(op2).GetDataType() == 1 ||
                                S.GetSymbol(op3).GetDataType() == 1)
                            {
                                anyFloat = 1;
                            }
                            else if (S.GetSymbol(op1).GetDataType() != 0 &&
                                S.GetSymbol(op2).GetDataType() != 0 &&
                                S.GetSymbol(op3).GetDataType() != 0)
                            {
                                anyFloat = -1;
                            }

                            switch (anyFloat)
                            {
                                case 0: // Int type
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetIntVal() 
                                        / S.GetSymbol(op2).GetIntVal());
                                    pc += 1;
                                    break;
                                case 1: // Double type
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetDoubleVal() 
                                        / S.GetSymbol(op2).GetDoubleVal());
                                    pc += 1;
                                    break;
                                default: //  If neither of the types just go on to the next instruction
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 2: //MUL
                            switch (S.GetSymbol(op3).GetDataType())
                            {
                                case 0: // int
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetIntVal() 
                                        * S.GetSymbol(op2).GetIntVal());
                                    pc += 1;
                                    break;
                                case 1:  // double
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetDoubleVal() 
                                        * S.GetSymbol(op2).GetDoubleVal());
                                    pc += 1;
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 3: //SUB
                            switch (S.GetSymbol(op3).GetDataType())
                            {
                                case 0:
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetIntVal() 
                                        - S.GetSymbol(op2).GetIntVal());
                                    pc += 1;
                                    break;
                                case 1:
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetDoubleVal() 
                                        - S.GetSymbol(op2).GetDoubleVal());
                                    pc += 1;
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 4: //ADD
                            switch (S.GetSymbol(op3).GetDataType())
                            {
                                case 0:
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetIntVal() + S.GetSymbol(op2).GetIntVal());
                                    pc += 1;
                                    break;
                                case 1:
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetDoubleVal() + S.GetSymbol(op2).GetDoubleVal());
                                    pc += 1;
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 5: //MOV
                            switch (S.GetSymbol(op3).GetDataType())
                            {
                                case 0:
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetIntVal());
                                    pc += 1;
                                    break;
                                case 1:
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetDoubleVal());
                                    pc += 1;
                                    break;
                                case 2: // String type
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetStringVal());
                                    pc += 1;
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 6: //STI
                            switch (S.GetSymbol(op2).GetDataType())
                            {
                                case 0:
                                    S.UpdateSymbol(op2, S.GetSymbol(op2).GetKind(),
                                        S.GetSymbol(op1).GetIntVal() 
                                        + S.GetSymbol(op3).GetIntVal());
                                    pc += 1;
                                    break;
                                case 1:
                                    S.UpdateSymbol(op2, S.GetSymbol(op2).GetKind(), 
                                        S.GetSymbol(op1).GetDoubleVal() 
                                        + S.GetSymbol(op3).GetDoubleVal());
                                    pc += 1;
                                    break;
                                case 2:
                                    S.UpdateSymbol(op3, S.GetSymbol(op2).GetKind(), 
                                        S.GetSymbol(op1).GetStringVal() 
                                        + S.GetSymbol(op3).GetStringVal());
                                    pc += 1;
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 7: //LDI
                            switch (S.GetSymbol(op3).GetDataType())
                            {
                                case 0:
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetIntVal() 
                                        + S.GetSymbol(op2).GetIntVal());
                                    pc += 1;
                                    break;
                                case 1:
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetDoubleVal() 
                                        + S.GetSymbol(op2).GetDoubleVal());
                                    pc += 1;
                                    break;
                                case 2:
                                    S.UpdateSymbol(op3, S.GetSymbol(op3).GetKind(), 
                                        S.GetSymbol(op1).GetStringVal() 
                                        + S.GetSymbol(op2).GetStringVal());
                                    pc += 1;
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 8: //BNZ
                            // Branch instructions modify the program counter
                            switch (S.GetSymbol(op1).GetDataType())
                            {
                                case 0:
                                    if (S.GetSymbol(op1).GetIntVal() != 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                case 1:
                                    if (S.GetSymbol(op1).GetDoubleVal() != 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 9: //BNP
                            switch (S.GetSymbol(op1).GetDataType())
                            {
                                case 0:
                                    if (S.GetSymbol(op1).GetIntVal() <= 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                case 1:
                                    if (S.GetSymbol(op1).GetDoubleVal() <= 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 10: //BNN
                            switch (S.GetSymbol(op1).GetDataType())
                            {
                                case 0:
                                    if (S.GetSymbol(op1).GetIntVal() >= 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                case 1:
                                    if (S.GetSymbol(op1).GetDoubleVal() >= 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 11: //BZ
                            switch (S.GetSymbol(op1).GetDataType())
                            {
                                case 0:
                                    if (S.GetSymbol(op1).GetIntVal() == 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                case 1:
                                    if (S.GetSymbol(op1).GetDoubleVal() == 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 12: //BP
                            switch (S.GetSymbol(op1).GetDataType())
                            {
                                case 0:
                                    if (S.GetSymbol(op1).GetIntVal() > 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                case 1:
                                    if (S.GetSymbol(op1).GetDoubleVal() > 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 13: //BN
                            switch (S.GetSymbol(op1).GetDataType())
                            {
                                case 0:
                                    if (S.GetSymbol(op1).GetIntVal() < 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                case 1:
                                    if (S.GetSymbol(op1).GetDoubleVal() < 0)
                                    {
                                        pc = op3;
                                    }
                                    else
                                    {
                                        pc += 1;
                                    }
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 14: //BR
                            pc = op3;
                            break;
                        case 15: //BINDR
                            switch (S.GetSymbol(op3).GetDataType())
                            {
                                case 0:
                                    pc = S.GetSymbol(op3).GetIntVal();
                                    break;
                                case 1:
                                    pc = (int)S.GetSymbol(op3).GetDoubleVal();
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                        case 16: //PRINT
                            switch (S.GetSymbol(op1).GetDataType())
                            {
                                case 0:
                                    Console.WriteLine(S.GetSymbol(op1).GetName().PadLeft(50, ' ') 
                                        + " =    " + S.GetSymbol(op1).GetIntVal());
                                    pc += 1;
                                    break;
                                case 1:
                                    Console.WriteLine(S.GetSymbol(op1).GetName().PadLeft(50, ' ') 
                                        + " =    " + S.GetSymbol(op1).GetDoubleVal());
                                    pc += 1;
                                    break;
                                case 2:
                                    Console.WriteLine(S.GetSymbol(op1).GetName().PadLeft(50, ' ') 
                                        + " =    " + S.GetSymbol(op1).GetStringVal());
                                    pc += 1;
                                    break;
                                default:
                                    pc += 1;
                                    break;
                            }
                            break;
                    }
                }
            }
        }
    }
}
