using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymanticAnalyzer
{
    // Represents a Symbol object to be used in a Symbol Table
    class Symbol
    {
        // Private fields for Symbol data
        private string _name;
        private int _kind;
        private int _data_type;
        private int _intVal;
        private double _doubleVal;
        private string _stringVal;

        // Provides an empty constructor for creating a new symbol with no data
        public Symbol()
        {

        }

        // Provides a constructor for creating a new symbol that is fully populated
        public Symbol(string name, int kind, int value)
        {
            _name = name;
            _kind = kind;
            // Automatically set the data type depending on the type of value
            _data_type = 0; //int
            _intVal = value;
        }

        public Symbol(string name, int kind, double value)
        {
            _name = name;
            _kind = kind;
            _data_type = 1; // double
            _doubleVal = value;
        }

        public Symbol(string name, int kind, string value)
        {
            _name = name;
            _kind = kind;
            _data_type = 2; // string
            _stringVal = value;
        }

        // Accessor and Mutator methods for changing private data
        public string GetName()
        {
            return _name;
        }

        public void SetName(string name)
        {
            _name = name;
        }

        public int GetKind()
        {
            return _kind;
        }

        public void SetKind(int kind)
        {
            _kind = kind;
        }

        public int GetDataType()
        {
            return _data_type;
        }

        public void SetDataType(int dataType)
        {
            _data_type = dataType;
        }

        public int GetIntVal()
        {
            return _intVal;
        }

        public void SetIntVal(int val)
        {
            _intVal = val;
            // If we have set the int val and used to default constructor
            // we need to ensure that the proper data type field is also indicated
            SetDataType(0);
        }

        public double GetDoubleVal()
        {
            return _doubleVal;
        }

        public void SetDoubleVal(double val)
        {
            _doubleVal = val;
            SetDataType(1);
        }

        public string GetStringVal()
        {
            return _stringVal;
        }

        public void SetStringVal(string val)
        {
            _stringVal = val;
            SetDataType(2);
        }
    }
}
