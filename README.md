## Projeto FileCompresserEcc ##

### T2 - Teoria da informação ###

- Alunos: Felipe Silva, Gabriel Wottawa e Gabriel Konrath

#Programas necessários para compilar o projeto:
- Visual Studio 2019;

#Intruções para compilar:
- Clonar o projeto usando Git ou fazer um download da pasta;
- Abrir o arquivo "FileCompresserEcc.sln";
- Abrir o projeto no Visual Studio;
- Após o projeto estar aberto, clicar em "F5" para o mesmo ser iniciado;

#Observações:
- Necessário que o arquivo a ser lido pelo programa esteja na pasta "C:\Temp";
  - Ex.: "C:\Temp\alice29.txt"
- Após o programa ser executado será criado outros 2 arquivos na pasta "C:\Temp";
  - Um arquivo codificado com a extensão ".cod";
    - Ex.: "C:\Temp\alice29.cod"
  - Um arquivo decodificado com a extensão ".dec";
    - Ex.: "C:\Temp\alice29.dec"

#Funcionamento do projeto:
  * Codificação:
    - O arquivo será lido e codificado de acordo com a codificação que você escolher, gerando assim um arquivo.cod;
    - Depois, será gerado um arquivo.ecc, com a adição da codificação hamming(7, 4), a partir do arquivo.cod gerado anteriormente;
  * Decodifição:
    - Primeiro o arquivo.ecc será lido e acontecerá a decodificação do hamming(7, 4), gerando novamente o .cod, idêntico ao gerado na codificação;
    - Depois, esse arquivo.cod será lido e decodificado de acordo com a codificação que você escolheu no começo.

#Limitações:
- Não foi possível em algumas codificações chegar a um valor de tamanho do arquivo menor do que o original.
