using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymanticAnalyzer
{
    // Represents a table of Quad objects
    class QuadTable
    {
        // The QuadTable is implemented as a list to prevent need for resizing manually
        private List<Quad> _quadTable = new List<Quad>();
        // Each QuadTable possesses its own reserve table to look up opcodes
        private ReserveTable _reserveTable;
        // Next Quad points to the next open index in the list
        private int nextQuad;

        private SymbolTable _symbolTable;

        // Sets the QuadTable's max size and what reserve table its using
        // This does not however put 'size' many objects into the structure
        public void Initialize(int size)
        {
            _quadTable = new List<Quad>(size);
            _reserveTable = BuildReserveTable(17);
        }

        public void SetSymbolTable(SymbolTable symbolTable)
        {
            this._symbolTable = symbolTable;
        }

        static ReserveTable BuildReserveTable(int size)
        {
            // Create a ReserveTable to store ReserveRow objects
            ReserveTable returnReserveTable = new ReserveTable(size);

            // Add each reserve row individually in the form name, code
            returnReserveTable.Add("STOP", 0);
            returnReserveTable.Add("DIV", 1);
            returnReserveTable.Add("MUL", 2);
            returnReserveTable.Add("SUB", 3);
            returnReserveTable.Add("ADD", 4);
            returnReserveTable.Add("MOV", 5);
            returnReserveTable.Add("STI", 6);
            returnReserveTable.Add("LDI", 7);
            returnReserveTable.Add("BNZ", 8);
            returnReserveTable.Add("BNP", 9);
            returnReserveTable.Add("BNN", 10);
            returnReserveTable.Add("BZ", 11);
            returnReserveTable.Add("BP", 12);
            returnReserveTable.Add("BN", 13);
            returnReserveTable.Add("BR", 14);
            returnReserveTable.Add("BINDR", 15);
            returnReserveTable.Add("PRINT", 16);

            // Return the reserve table
            return returnReserveTable;
        }

        // Returns the next available space in the table
        public int NextQuad()
        {
            return nextQuad;
        }

        // Adds a new fully formed Quad object into the table at the next index
        public int AddQuad(int opcode, int op1, int op2, int op3)
        {
            Quad tempQuad = new Quad(opcode, op1, op2, op3);
            _quadTable.Add(tempQuad);
            // Ensures that the next open space is properly indicated
            nextQuad++;
            return nextQuad - 1;
        }

        // Returns a quad at the specified index
        public Quad GetQuad(int index)
        {
            return _quadTable[index];
        }

        // Sets the values of a quad at a desired index to a set of new values
        public void SetQuad(int index, int opcode, int op1, int op2, int op3)
        {
            Quad tempQuad = _quadTable[index];
            tempQuad.SetOpcode(opcode);
            tempQuad.SetOp1(op1);
            tempQuad.SetOp2(op2);
            tempQuad.SetOp3(op3);
            _quadTable[index] = tempQuad;
        }

        public void SetQuadOp3(int index, int op3)
        {
            Quad tempQuad = _quadTable[index];
            tempQuad.SetOp3(op3);
            _quadTable[index] = tempQuad;
        }

        // Returns the 'name' associated with an opcode from the reserve table
        public string GetMnemonic(int opcode)
        {
            return _reserveTable.LookupCode(opcode);
        }

        // Prints the reserve table attached to the quad table
        public void PrintReserveTable()
        {
            _reserveTable.PrintReserveTable();
        }

        // Prints the values of the quad table
        public void PrintQuadTable()
        {
            Console.WriteLine("Printing Quad Table: ");
            Console.WriteLine("--------------------------------------------------------");
            int counter = 0;
            foreach (Quad entry in _quadTable)
            {
                Console.Write(counter.ToString().PadLeft(4, '0') + ": ");
                counter++;
                Console.Write(GetMnemonic(entry.GetOpcode()).PadLeft(10, ' ') + "|");
                Console.Write(_symbolTable.GetSymbol(entry.GetOp1()).GetName().PadLeft(20, ' ')
                    + " <" + entry.GetOp1() + ">" + "|".PadLeft(5, ' '));
                Console.Write(_symbolTable.GetSymbol(entry.GetOp2()).GetName().PadLeft(10, ' ')
                    + " <" + entry.GetOp2() + ">" + "|".PadLeft(5, ' '));

                if (entry.GetOpcode() >= 8 && entry.GetOpcode() <= 15)
                {
                    Console.Write(" <" + entry.GetOp3() + ">\n");
                }
                else
                {
                    Console.Write(_symbolTable.GetSymbol(entry.GetOp3()).GetName().PadLeft(10, ' ')
                        + " <" + entry.GetOp3() + ">\n");
                }
            }
            Console.WriteLine("--------------------------------------------------------");
        }
    }
}
