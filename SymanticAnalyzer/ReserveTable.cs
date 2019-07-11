using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymanticAnalyzer
{
    // Represents a table of reserve rows
    class ReserveTable
    {
        // The reserve table is implemented as a list to prevent the need to manually resize
        private List<ReserveRow> _reserveTable;
        // The table keeps track of how many things it can possibly hold and how many it currently holds
        private int _numUsed, _maxCount;

        // Constructor used to generate a new reserve table of max size 'size'
        public ReserveTable(int size)
        {
            _reserveTable = new List<ReserveRow>(size);
            _maxCount = size;
            _numUsed = 0;
        }

        // Used to add a new reserve row into the table as long as there is room
        public void Add(string name, int value)
        {
            if (_numUsed < _maxCount)
            {
                ReserveRow tempReserveRow = new ReserveRow(name, value);
                _reserveTable.Add(tempReserveRow);
                _numUsed++;
            }
        }

        // Used to lookup an opcode, returns the name associated with that opcode or the
        // blank string if no opcode is found
        public string LookupCode(int code)
        {
            int counter = 0;
            foreach (ReserveRow entry in _reserveTable)
            {
                if (entry.GetCode() == code)
                {
                    return _reserveTable[counter].GetName();
                }
                counter++;
            }
            return "";
        }

        // Used to lookup a name, returns the code associated with that name or
        // -1 if that name is not found
        public int LookupName(string name)
        {
            int counter = 0;
            foreach (ReserveRow entry in _reserveTable)
            {
                if (entry.GetName() == name)
                {
                    return _reserveTable[counter].GetCode();
                }
                counter++;
            }
            return -1;
        }

        // Prints the reserve table
        public void PrintReserveTable()
        {
            Console.WriteLine("Printing Reserve Table: ");
            Console.WriteLine("----------------------");
            foreach (ReserveRow entry in _reserveTable)
            {
                Console.Write("Name: " + entry.GetName());
                Console.Write(" " + "Code: " + entry.GetCode() + "\n");
            }
            Console.WriteLine("----------------------");
        }
    }
}
