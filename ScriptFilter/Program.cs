// See https://aka.ms/new-console-template for more information
using ScriptFilter;
using System.Text;

Console.WriteLine("Hello, World!");


string caminhoOriginal = "C:\\Users\\samun\\Downloads\\SQLQuery2.sql";
string caminhoNovo = "C:\\Users\\samun\\Downloads\\SQLQuery3.sql";
string blackList = "C:\\Users\\samun\\Downloads\\BlackList.txt";
string caminhoFiltrado  = "C:\\Users\\samun\\Downloads\\SQLQuery4.sql";
string usedDatabase = "[$(DatabaseName)]";

var scripts = SqlWriter.LerScriptsSQL(caminhoOriginal);

SqlWriter.EscreverScriptsSQL(caminhoNovo, scripts, usedDatabase);

List<string> blacklist = SqlWriter.LerBlacklist(blackList);

SqlWriter.FiltrarArquivo(caminhoNovo, caminhoFiltrado, blacklist);