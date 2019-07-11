using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymanticAnalyzer
{
    // Represents Quad data to be stored in a QuadTable
    class Quad
    {
        // Private variables to store data
        private int _opcode;
        private int _op1;
        private int _op2;
        private int _op3;

        // Provides a general constructor that does not populate anything
        public Quad()
        {

        }

        // Provides an alternate constructor allowing for creation of a fully
        // populated Quad
        public Quad(int opcode, int op1, int op2, int op3)
        {
            _opcode = opcode;
            _op1 = op1;
            _op2 = op2;
            _op3 = op3;
        }

        // Accessor and Mutator methods for accessing private data
        public int GetOpcode()
        {
            return _opcode;
        }

        public void SetOpcode(int opcode)
        {
            _opcode = opcode;
        }

        public int GetOp1()
        {
            return _op1;
        }

        public void SetOp1(int op1)
        {
            _op1 = op1;
        }

        public int GetOp2()
        {
            return _op2;
        }

        public void SetOp2(int op2)
        {
            _op2 = op2;
        }

        public int GetOp3()
        {
            return _op3;
        }

        public void SetOp3(int op3)
        {
            _op3 = op3;
        }
    }
}
