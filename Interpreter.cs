using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Compiler
{
    public class Interpreter
    {
        private List<string> C { get; set; }
        private List<float> D { get; set; }
        private int i { get; set; }
        private int s { get; set; }

        public Interpreter(string path)
        {
            C = File.ReadLines(path).ToList();
            D = new List<float>();
            i = 0;
        }

        public void execute()
        {
            while (C.Count > i)
            {
                string[] terms = C[i].Split(' ');
                string function = terms[0];

                MethodInfo method = GetType().GetMethod(function);
                if (terms.Length == 2)
                {
                    string parameter = terms[1];
                    method?.Invoke(this, new object[]{parameter});
                }
                else
                {
                    method?.Invoke(this, new object[]{});
                }

                i++;
            }
        }

        // Carrega constante k no topo da pilha D
        public void CRCT(string k)
        {
            D = D.Append(float.Parse(k, CultureInfo.InvariantCulture)).ToList();
            s++;
        }
        
        // Carrega o valor de endereço n no topo da pilha D
        public void CRVL(string n)
        {
            D = D.Append(D[int.Parse(n)]).ToList();
            s++;
        }

        // Soma o elemento antecessor com o topo da pilha
        public void SOMA()
        {
            D[s - 1] += D[s];
            D.RemoveAt(s--);
        }

        // Subtrai o antecessor pelo elemento do topo
        public void SUBT()
        {
            D[s - 1] -= D[s];
            D.RemoveAt(s--);
        }

        // Multiplica o elemento antecessor pelo elemento do topo
        public void MULT()
        {
            D[s - 1] *= D[s];
            D.RemoveAt(s--);
        }

        // Divide o elemento antecessor pelo elemento do topo
        public void DIVI()
        {
            D[s - 1] /= D[s];
            D.RemoveAt(s--);
        }
        
        // Inverte o sinal do topo
        public void INVE()
        {
            D[s] *= -1;
        }
        
        // Conjunção de valores lógicos. F = 0; V = 1
        public void CONJ()
        {
            if ((int) D[s - 1] == 1 && (int) D[s] == 1)
            {
                D[s - 1] = 1;
            }
            else
            {
                D[s - 1] = 0;
            }
            
            D.RemoveAt(s--);
        }
        
        // Disjunção de valores lógicos
        public void DISJ()
        {
            if ((int) D[s - 1] == 1 || (int) D[s] == 1)
            {
                D[s - 1] = 1;
            }
            else
            {
                D[s - 1] = 0;
            }
            D.RemoveAt(s--);
        }
        
        // Negação lógica
        public void NEGA()
        {
            D[s] = 1 - D[s];
        }
        
        // Comparação de menor entre o antecessor e o topo
        public void CPME()
        {
            if (D[s - 1] < D[s])
            {
                D[s - 1] = 1;
            }
            else
            {
                D[s - 1] = 0;
            }
            D.RemoveAt(s--);
        }
        
        // Comparação de maior
        public void CPMA()
        {
            if (D[s - 1] > D[s])
            {
                D[s - 1] = 1;
            }
            else
            {
                D[s - 1] = 0;
            }
            D.RemoveAt(s--);
        }

        // Comparação de igualdade
        public void CPIG()
        {
            if (D[s - 1] == D[s])
            {
                D[s - 1] = 1;
            }
            else
            {
                D[s - 1] = 0;
            }
            D.RemoveAt(s--);
        }
        
        // Comparação de desigualdade
        public void CDES()
        {
            if (D[s - 1] != D[s])
            {
                D[s - 1] = 1;
            }
            else
            {
                D[s - 1] = 0;
            }
            D.RemoveAt(s--);
        }
        
        // Comparação menor-igual
        public void CPMI()
        {
            if (D[s - 1] <= D[s])
            {
                D[s - 1] = 1;
            }
            else
            {
                D[s - 1] = 0;
            }
            D.RemoveAt(s--);
        }
        
        // Comparação maior-igual
        public void CMAI()
        {
            if (D[s - 1] >= D[s])
            {
                D[s - 1] = 1;
            }
            else
            {
                D[s - 1] = 0;
            }
            D.RemoveAt(s--);
        }

        // Armazena o topo da pilha no endereço n de D
        public void ARMZ(string n)
        {
            D[int.Parse(n)] = D[s];
            D.RemoveAt(s--);
        }
        
        // Desvio incodicional para a instrução de endereço p
        public void DSVI(string p)
        {
            i = int.Parse(p);
            // TODO verificar
        }
        
        // Desvio condicional para a intrução de endereço p; o
        // desvio será executado caso a condição resultante seja
        // falsa; o valor da condição estará no topo
        public void DSVF(string p)
        {
            if (D[s] == 0)
            {
                // TODO verificar
                i = int.Parse(p);
            }
            D.RemoveAt(s--);
        }
        
        // Lê um dado de entrada para o topo da pilha
        public void LEIT()
        {
            s++;
            D = D.Append(float.Parse(Console.ReadLine(), CultureInfo.InvariantCulture)).ToList();
        }
        
        // Imprime o valor do topo da pilha na saída
        public void IMPR()
        {
            Console.WriteLine(D[s].ToString(CultureInfo.InvariantCulture));
            D.RemoveAt(s--);
        }
        
        // Reserva m posições na pilha D; m depende do tipo da variável
        public void ALME(string m)
        {
            D = D.Append(0).ToList();
            s += int.Parse(m);
        }
        
        // Inicia o programa - será sempre a 1ª instrução
        public void INPP()
        {
            s = -1;
        }
        
        // Termina a execução do programa
        public void PARA()
        {
        }
        
    }
}