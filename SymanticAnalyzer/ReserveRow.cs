using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymanticAnalyzer
{
    // Represents a reserve row for use in a reserve table
    class ReserveRow
    {
        // Every reserve row has a name and code value
        private string _name;
        private int _code;

        // Provides a constructor for creating a new fully formed reserve row
        public ReserveRow(string name, int code)
        {
            _name = name;
            _code = code;
        }

        // Accessor and Mutator methods for private data
        public string GetName()
        {
            return _name;
        }

        public void SetName(string name)
        {
            _name = name;
        }

        public int GetCode()
        {
            return _code;
        }

        public void SetCode(int code)
        {
            _code = code;
        }
    }
}
