# compiler
This project was developed for Compilers 1 subject. The implementation is just the frontend of a compiler, thus it analysis is just the syntactic and semantic. 

To run the program use ```dotnet run``` or ```dotnet run file```.

Automaton:
![alt text](https://github.com/Hiago-Patricio/compiler2/blob/main/Automaton.png)

Syntactic grammar:

```
LALG Language Syntax Specification
- Comments on LALG: between { } or /* */


<programa> -> program ident <corpo> .
<corpo> -> <dc> begin <comandos> end
<dc> -> <dc_v> <mais_dc>  | λ
<mais_dc> -> ; <dc> | λ
<dc_v> ->  <tipo_var> : <variaveis>
<tipo_var> -> real | integer
<variaveis> -> ident <mais_var>
<mais_var> -> , <variaveis> | λ
<comandos> -> <comando> <mais_comandos>
<mais_comandos> -> ; <comandos> | λ

<comando> -> read (ident) |
             write (ident) |
             ident := <expressao> |
             if <condicao> then <comandos> <pfalsa> $ |
             while <condicao> do <comandos> $
							
<condicao> -> <expressao> <relacao> <expressao>
<relacao> -> = | <> | >= | <= | > | <
<expressao> -> <termo> <outros_termos>
<termo> -> <op_un> <fator> <mais_fatores>
<op_un> -> - | λ
<fator> -> ident | numero_int | numero_real | (<expressao>)
<outros_termos> -> <op_ad> <termo> <outros_termos> | λ
<op_ad> -> + | -
<mais_fatores> -> <op_mul> <fator> <mais_fatores> | λ
<op_mul> -> * | /
<pfalsa> -> else <comandos> | λ
```