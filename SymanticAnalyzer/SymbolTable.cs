using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymanticAnalyzer
{
    // Represents a table of Symbol objects
    class SymbolTable
    {
        // The Symbol talbe is implemented as a list to avoid the need to manually resize
        // The symbol table is given a max size of 100 by default
        private List<Symbol> _symbolTable = new List<Symbol>(100);
        private int _entriesUsed = 0;

        // Returns the index of the next open space
        public int GetEntriesUsed()
        {
            return _entriesUsed;
        }

        // Used to add a new fully formed symbol into the table (int)
        public int AddSymbol(string symbol, int kind, int value)
        {
            //Look for an instance of a symbol with the desired name
            int entryCounter = 0;
            foreach (Symbol entry in _symbolTable)
            {
                // If there is a symbol of the same name do not add it
                if (entry.GetName() == symbol)
                {
                    // Inform that the symbol was not added
                    //Console.WriteLine("Duplicate entry " + "\"" + symbol + "\" " + "found");
                    //Return the index of the duplicate entry
                    return entryCounter;
                }
                entryCounter++;
            }

            //Else add the symbol as normal
            Symbol tempSymbol = new Symbol(symbol, kind, value);
            //Put the symbol in the next available spot
            _symbolTable.Add(tempSymbol);
            //Set the next available spot to the next index
            _entriesUsed++;
            return _entriesUsed - 1;
        }

        public void UpdateSymbolDataType(int index, int dataType)
        {
            Symbol tempSymbol = _symbolTable[index];

            tempSymbol.SetDataType(dataType);

            _symbolTable[index] = tempSymbol;

        }

        // Used to add a fully formed symbol into the table (Double)
        public int AddSymbol(string symbol, int kind, double value)
        {
            int entryCounter = 0;
            // Do not add duplicate symbols
            foreach (Symbol entry in _symbolTable)
            {
                if (entry.GetName() == symbol)
                {
                    //Console.WriteLine("Duplicate entry " + "\"" + symbol + "\" " + "found");
                    return entryCounter;
                }
                entryCounter++;
            }

            Symbol tempSymbol = new Symbol(symbol, kind, value);
            _symbolTable.Add(tempSymbol);
            _entriesUsed++;
            return _entriesUsed - 1;
        }

        // Used to add a fully formed symbol into the table (String)
        public int AddSymbol(string symbol, int kind, string value)
        {
            int entryCounter = 0;
            // Do not add duplicate symbols
            foreach (Symbol entry in _symbolTable)
            {
                if (entry.GetName() == symbol)
                {
                    //Console.WriteLine("Duplicate entry " + "\"" + symbol + "\" " + "found");
                    return entryCounter;
                }
                entryCounter++;
            }

            Symbol tempSymbol = new Symbol(symbol, kind, value);
            _symbolTable.Add(tempSymbol);
            _entriesUsed++;
            return _entriesUsed - 1;
        }

        // Used to lookup the index of a symbol, returns the index or -1 if not found
        public int LookupSymbol(String symbol)
        {
            int entryCounter = 0;
            // look through all entries to find the symbol with the desired name
            foreach (Symbol entry in _symbolTable)
            {
                if (entry.GetName() == symbol)
                {
                    return entryCounter;
                }
                entryCounter++;
            }
            // if not found return -1
            return -1;
        }

        // Returns the symbol at a desired index in the list
        public Symbol GetSymbol(int index)
        {
            if (index >= 0 && index < _symbolTable.Count)
            {
                return _symbolTable[index];
            }
            else
            {
                return null;
            }
        }

        public void UpdateSymbolIntvalue(int index, int value)
        {
            Symbol tempSymbol = _symbolTable[index];
            tempSymbol.SetIntVal(value);
            _symbolTable[index] = tempSymbol;
        }

        // Populates the symbol found at the desired index with new data (int)
        public void UpdateSymbol(int index, int kind, int value)
        {
            Symbol tempSymbol = _symbolTable[index];
            tempSymbol.SetKind(kind);
            tempSymbol.SetIntVal(value);
            _symbolTable[index] = tempSymbol;
        }

        // Populates the symbol found at the desired index with new data (double)
        public void UpdateSymbol(int index, int kind, double value)
        {
            Symbol tempSymbol = _symbolTable[index];
            tempSymbol.SetKind(kind);
            tempSymbol.SetDoubleVal(value);
            _symbolTable[index] = tempSymbol;
        }

        // Populates the symbol found at the desired index with new data (string)
        public void UpdateSymbol(int index, int kind, string value)
        {
            Symbol tempSymbol = _symbolTable[index];
            tempSymbol.SetKind(kind);
            tempSymbol.SetStringVal(value);
            _symbolTable[index] = tempSymbol;
        }

        public int GetSymbolValue(int index)
        {
            return _symbolTable[index].GetIntVal();
        }

        // Print the SymbolTable
        public void PrintSymbolTable()
        {
            Console.WriteLine("\n\nPrinting Symbol Table: ");
            Console.WriteLine("-------------------------------------------------------------------");
            int counter = 0;
            foreach (Symbol entry in _symbolTable)
            {
                Console.Write(counter.ToString().PadLeft(3, '0') + ": ");
                counter++;
                Console.Write(entry.GetName().PadRight(35));
                switch (entry.GetKind())
                {
                    case 0:
                        Console.Write(" " + "label".PadRight(10));
                        break;
                    case 1:
                        Console.Write(" " + "variable".PadRight(10));
                        break;
                    case 2:
                        Console.Write(" " + "constant".PadRight(10));
                        break;
                }
                switch (entry.GetDataType())
                {
                    case 0:
                        Console.Write(" int     " + entry.GetIntVal() + "\n");
                        break;
                    case 1:
                        Console.Write(" double      " + entry.GetDoubleVal() + "\n");
                        break;
                    case 2:
                        Console.Write(" string  " + entry.GetStringVal() + "\n");
                        break;
                }
            }
            Console.WriteLine("-------------------------------------------------------------------");
        }
    }
}