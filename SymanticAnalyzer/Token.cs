using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymanticAnalyzer
{
    class Token
    {
        public string _lexeme;
        public int _tokenCode;
        public string _mnemonic;
        public int _symbolTableIndex;
        public bool _isIDorLiterals;

        public Token()
        {

        }

        public Token(string lexeme, int tokenCode, string mnemonic, int symbolTableIndex, bool isIDorLiteral)
        {
            _lexeme = lexeme;
            _tokenCode = tokenCode;
            _mnemonic = mnemonic;
            _symbolTableIndex = symbolTableIndex;
            _isIDorLiterals = isIDorLiteral;
        }

        public Token(string lexeme, int tokenCode, string mnemonic)
        {
            _lexeme = lexeme;
            _tokenCode = tokenCode;
            _mnemonic = mnemonic;
            _symbolTableIndex = -1;
            _isIDorLiterals = false;
        }
    }
}
