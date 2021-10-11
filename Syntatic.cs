using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Compiler
{
    class Syntatic
    {
        private LexScanner lexScanner;
        private Token token;
        private EnumToken? tipo;
        private Dictionary<String, Symbol> SymbolTable = new Dictionary<string, Symbol>();
        private List<string> C = new List<string>{};
        private int s = -1;
        private string output;

        public Syntatic(string input, string output)
        {
            lexScanner = new LexScanner(input);
            this.output = output;
        }

        public void analysis()
        {
            getToken();
            programa();
            if (token == null)
            {
                File.WriteAllLines(output, C.ToArray());
            }
            else
            {
                throw new Exception(
                    $"Erro sintático, era esperado um fim de cadeia, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");
            }
        }
        
        private void getToken()
        {
            token = lexScanner.NextToken();

            while(verifyTokenValue("{", "/*"))
            {
                if (verifyTokenValue("{"))
                {
                    while (!verifyTokenValue("}"))
                    {
                        token = lexScanner.NextToken();
                    }
                    token = lexScanner.NextToken();
                }

                if (verifyTokenValue("/*"))
                {
                    while (!verifyTokenValue("*/"))
                    {
                        token = lexScanner.NextToken();
                    }
                    token = lexScanner.NextToken();
                }
            }
        }

        private bool verifyTokenValue(params string[] terms)
        {
            return terms.Any(t => token != null && token.value.Equals(t));
        }

        private bool verifyTokenType(params EnumToken[] enums)
        {
            return enums.Any(e => token != null && token.type.Equals(e));
        }

        // <programa> -> program ident <corpo> .
        private void programa()
        {
            if (verifyTokenValue("program"))
            {
                getToken();
                if (verifyTokenType(EnumToken.IDENTIFIER))
                {
                    C = C.Append("INPP").ToList();
                    corpo();
                    getToken();
                    if (!verifyTokenValue("."))
                    {
                        throw new Exception($"Erro sintático, '.' era esperado, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");
                    }

                    C = C.Append("PARA").ToList();
                    getToken();
                }
                else
                {
                    throw new Exception($"Erro sintático, identificador era esperado, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");    
                }
            }
            else
            {
                throw new Exception($"Erro sintático, 'program' era esperado, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");
            }
        }

        // <corpo> -> <dc> begin <comandos> end
        private void corpo()
        {
            dc();
            if (verifyTokenValue("begin"))
            {
                comandos();
                if (!verifyTokenValue("end"))
                {
                    throw new Exception($"Erro sintático, 'end' ou ';' era esperado, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");
                }
            }
            else
            {
                throw new Exception($"Erro sintático, 'begin' ou ';' era esperado, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");
            }
        }
        
        // <dc> -> <dc_v> <mais_dc>  | λ
        private void dc()
        {
            getToken();
            if (!verifyTokenValue("begin"))
            {
                dc_v();
                mais_dc();
            }
        }

        // <dc_v> ->  <tipo_var> : <variaveis>
        private void dc_v()
        {
            var tipoVarDir = tipo_var();
            getToken();
            if (verifyTokenValue(":"))
            {
                variaveis(tipoVarDir);
            }
            else
            {
                throw new Exception($"Erro sintático, ':' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
            }
        }

        // <tipo_var> -> real | integer
        private EnumToken tipo_var()
        {
            if (token.value == "real")
            {
                return EnumToken.REAL;
            }
            else if (token.value == "integer")
            {
                return EnumToken.INTEGER;
            }
            
            throw new Exception($"Erro sintático, 'real', 'integer' ou 'begin' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
        }

        // <variaveis> -> ident <mais_var>
        private void variaveis(EnumToken variaveisEsq)
        {
            getToken();
            if (verifyTokenType(EnumToken.IDENTIFIER))
            {
                if (SymbolTable.ContainsKey(token.value))
                {
                    throw new Exception($"Erro semântico, o identificador '{token.value}' já foi declarado.");
                }
                C = C.Append("ALME 1").ToList();
                SymbolTable.Add(token.value, new Symbol(variaveisEsq ,token.value, ++s));
                mais_var(variaveisEsq);
            }
            else
            {
                throw new Exception($"Erro sintático, identificador era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
            }
        }
        
        // <mais_var> -> , <variaveis> | λ
        private void mais_var(EnumToken maisVarEsq)
        {
            getToken();
            if (verifyTokenValue(","))
            {
                variaveis(maisVarEsq);
            }
        }
        
        // <mais_dc> -> ; <dc> | λ
        private void mais_dc()
        {
            if (verifyTokenValue(";"))
            {
                dc();
            }
        }
        
        // <comandos> -> <comando> <mais_comandos>
        private void comandos()
        {
            comando();
            mais_comandos();
        }

        /*
         * <comando> -> read (ident)
		 * <comando> ->	write (ident)
		 * <comando> ->	ident := <expressao>
		 * <comando> ->	if <condicao> then <comandos> <pfalsa> $
         */
        private void comando()
        {
            getToken();
            if (verifyTokenValue("read", "write"))
            {
                var opCode = token.value;
                getToken();
                if (verifyTokenValue("("))
                {
                    getToken();
                    if (verifyTokenType(EnumToken.IDENTIFIER))
                    {
                        var ident = token.value;
                        if (!SymbolTable.ContainsKey(ident))
                        {
                            throw new Exception($"Erro semântico, o identificador '{ident}' não foi declarado.");
                        }
                        
                        getToken();
                        if (!verifyTokenValue(")"))
                        {
                            throw new Exception($"Erro sintático, ')' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
                        }

                        s++;
                        if (opCode == "read")
                        {
                            C = C.Append("LEIT").ToList();
                            C = C.Append($"ARMZ {SymbolTable[ident].endRel}").ToList();
                        }
                        else
                        {
                            C = C.Append($"CRVL {SymbolTable[ident].endRel}").ToList();
                            C = C.Append("IMPR").ToList();
                        }
                        
                        getToken();
                    }
                    else
                    {
                        throw new Exception($"Erro sintático, identificador era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");    
                    }
                }
                else
                {
                    throw new Exception($"Erro sintático, '(' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
                }
            }
            else if (verifyTokenType(EnumToken.IDENTIFIER))
            {
                var ident = token.value;
                if (!SymbolTable.ContainsKey(ident))
                {
                    throw new Exception($"Erro semântico, o identificador '{ident}' não foi declarado.");
                }
                getToken();
                if (verifyTokenValue(":="))
                {
                    tipo = SymbolTable[ident].type;
                    expressao();
                    tipo = null;
                    C = C.Append($"ARMZ {SymbolTable[ident].endRel}").ToList();
                }
                else
                {
                    throw new Exception($"Erro sintático, ':=' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
                }
            }
            else if (verifyTokenValue("if"))
            {   
                condicao();
                if (verifyTokenValue("then"))
                {
                    var DSVFLineToReplace = C.Count;
                    C = C.Append("DSVF DSVFLineToReplace").ToList();

                    comandos();

                    var DSVILineToReplace = C.Count;
                    C = C.Append("DSVI DSVILineToReplace").ToList();

                    var ElseLine = C.Count;
                    pfalsa();

                    // não tem else
                    if (C.Count == ElseLine)
                    {
                        C.RemoveAt(C.Count - 1);
                        C[DSVFLineToReplace] = C[DSVFLineToReplace].Replace("DSVFLineToReplace", C.Count.ToString());
                    }
                
                    if (C.Count > ElseLine)
                    {
                        C[DSVFLineToReplace] = C[DSVFLineToReplace].Replace("DSVFLineToReplace", ElseLine.ToString());
                        C[DSVILineToReplace] = C[DSVILineToReplace].Replace("DSVILineToReplace", C.Count.ToString());
                    }
                
                    if (!verifyTokenValue("$"))
                    {
                        throw new Exception($"Erro sintático, '$' ou ';' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
                    }
                    getToken();
                }
                else
                {
                    throw new Exception($"Erro sintático, 'then' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
                }
            }
            else if (verifyTokenValue("while"))
            {
                var ConditionLineToReplace = C.Count; 
                condicao();

                var DSVFLineToReplace = C.Count;
                C = C.Append("DSVF DSVFLineToReplace").ToList();

                if (verifyTokenValue("do"))
                {
                    comandos();

                    C = C.Append($"DSVI {ConditionLineToReplace}").ToList();

                    C[DSVFLineToReplace] = C[DSVFLineToReplace].Replace("DSVFLineToReplace", C.Count.ToString());
                
                    if (!verifyTokenValue("$"))
                    {
                        throw new Exception(
                            $"Erro sintático, '$' ou ';' era esperado, mas foi encontrado: '{(token == null ? "NULL" : token.value)}'.");
                    }
                    getToken();
                }
                else
                {
                    throw new Exception($"Erro sintático, 'do' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
                }
            
            }
            else
            {
                throw new Exception($"Erro sintático, 'read', 'write', 'if' ou identificador era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
            }
        }
        
        // <mais_comandos> -> ; <comandos> | λ 
        private void mais_comandos()
        {
            if (verifyTokenValue(";"))
            {
                comandos();
            }
        }

        // <expressao> -> <termo> <outros_termos>
        private void expressao()
        {
            termo();
            outros_termos();
        }

        // <outros_termos> -> <op_ad> <termo> <outros_termos> | λ
        private void outros_termos()
        {
            if (verifyTokenValue("+", "-"))
            {
                var opAdDir = op_ad();
                getToken();
                termo();

                if (opAdDir is "+")
                {
                    C = C.Append("SOMA").ToList();
                }
                else
                {
                    C = C.Append("SUBT").ToList();
                }
                
                outros_termos();
            }
        }

        // <op_ad> -> + | -
        private string op_ad()
        {
            return token.value;
        }

        // <condicao> -> <expressao> <relacao> <expressao>  
        private void condicao()
        {
            expressao();
            var relacaoDir = relacao();
            expressao();

            switch (relacaoDir)
            {
                case "=":
                    C = C.Append("CPIG").ToList();
                    break;
                case "<>":
                    C = C.Append("CDES").ToList();
                    break;
                case "<":
                    C = C.Append("CPME").ToList();
                    break;
                case ">":
                    C = C.Append("CPMA").ToList();
                    break;
                case "<=":
                    C = C.Append("CPMI").ToList();
                    break;
                case ">=":
                    C = C.Append("CMAI").ToList();
                    break;
            }
        }

        // <pfalsa> -> else <comandos> | λ  
        private void pfalsa()
        {
            if (verifyTokenValue("else"))
            {
                comandos();
            }
        }

        // <relacao> -> = | <> | >= | <= | > | <  
        private string relacao()
        {
            if (!verifyTokenValue("=", "<>", "<>", ">=", "<=", ">", "<"))
            {
                throw new Exception($"Erro sintático, '=', '<>', '>=', '<=', '>' ou '<' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
            }

            return token.value;
        }

        // <termo> -> <op_un> <fator> <mais_fatores> 
        private void termo()
        {
            var opUnDir = op_un();
            fator(opUnDir);
            mais_fatores();
        }

        // <op_un> -> - | λ
        private string op_un()
        {
            if (!verifyTokenType(EnumToken.IDENTIFIER, EnumToken.REAL, EnumToken.INTEGER))
            {
                getToken();
                if (verifyTokenValue("-"))
                {
                    getToken();
                    return token.value;
                }
            }
            return "";
        }

        // <fator> -> ident | numero_int | numero_real | (<expressao>)   
        private void fator(string fatorEsq)
        {
            if (verifyTokenValue("("))
            {
                expressao();
                
                getToken();
                if (!verifyTokenValue(")"))
                {
                    throw new Exception($"Erro sintático, ')' esperado, mas foi recebido: '{(token == null ? "NULL": token.value)}'.");
                }
                
                if (fatorEsq == "-")
                {
                    C = C.Append("INVE").ToList();
                }
                return;

            }
            else if (verifyTokenType(EnumToken.INTEGER, EnumToken.REAL))
            {
                var number = token.value;

                C = C.Append($"CRCT {number}").ToList();
                if (fatorEsq == "-")
                {
                    C = C.Append("INVE").ToList();
                }

                return;
            } 
            else if (verifyTokenType(EnumToken.IDENTIFIER))
            {
                var ident = token.value;
                
                if (!SymbolTable.ContainsKey(ident))
                {
                    throw new Exception($"Erro semântico, o identificador '{ident}' não foi declarado.");
                }

                C = C.Append($"CRVL {SymbolTable[ident].endRel}").ToList();
                if (fatorEsq == "-")
                {
                    C = C.Append("INVE").ToList();
                }
                
                return;
            }
            else if(verifyTokenValue("$"))
            {
                return;
            }
            throw new Exception($"Erro sintático, identificador, número inteiro, número real ou '(' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
        }

        // <mais_fatores> -> <op_mul> <fator> <mais_fatores> | λ  
        private void mais_fatores()
        {
            getToken();
            if (verifyTokenValue("*", "/"))
            {
                var opMulDir = op_mul();
                getToken();
                fator(opMulDir);

                if (opMulDir == "*")
                {
                    C = C.Append("MULT").ToList();
                }
                else
                {
                    C = C.Append("DIVI").ToList();
                }
                
                mais_fatores();
            }
        }

        // <op_mul> -> * | / 
        private string op_mul()
        {
            return token.value;
        }
    }
}
