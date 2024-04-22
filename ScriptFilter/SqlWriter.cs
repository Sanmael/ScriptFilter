using System.Text;
using System.Text.RegularExpressions;

namespace ScriptFilter
{
    public class SqlWriter
    {
        private static string[] Methods = ["Create Table", "Create Procedure", "Alter Proceudre", "Alter Table", "Drop Procedure"];
        private static bool InTheBlock = false;
        private static bool RemoveBlock = false;
        private static List<string> AddedScripts = new List<string>();

        public static List<string> LerScriptsSQL(string caminhoArquivo)
        {
            using (StreamReader sr = new StreamReader(caminhoArquivo))
            {
                StringBuilder atualScript = new StringBuilder();

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine()!;

                    ValidateInTheBlock(atualScript, line);

                    ValidateIfBlockFinished(atualScript, line);
                }

                if (atualScript.Length > 0)
                {
                    AddedScripts.Add(atualScript.ToString());
                }
            }

            return AddedScripts;
        }

        public static void EscreverScriptsSQL(string caminhoArquivo, List<string> scripts,string dataBase)
        {
            using (StreamWriter sw = new StreamWriter(caminhoArquivo))
            {
                for (int i = 0; i < scripts.Count; i++)
                {
                    if (i == 0)
                    {
                        sw.WriteLine("GO");
                        sw.WriteLine(dataBase);
                        sw.WriteLine("");
                        sw.WriteLine("GO");
                    }

                    sw.WriteLine(scripts[i]);
                    sw.WriteLine("GO");
                }
            }
        }

        public static List<string> LerBlacklist(string caminhoArquivo)
        {
            List<string> blacklist = new List<string>();

            using (StreamReader sr = new StreamReader(caminhoArquivo))
            {
                string linha;
                while ((linha = sr.ReadLine()!) != null)
                {
                    blacklist.Add(linha.Trim());
                }
            }

            return blacklist;
        }

        public static void FiltrarArquivo(string caminhoArquivoOriginal, string caminhoArquivoFiltrado, List<string> blacklist)
        {
            using (StreamReader sr = new StreamReader(caminhoArquivoOriginal))
            using (StreamWriter sw = new StreamWriter(caminhoArquivoFiltrado))
            {
                StringBuilder atualScript = new StringBuilder();                

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine()!;

                    ValidateIsblocked(line, blacklist, atualScript); 

                    if (InTheBlock)
                    {
                        atualScript.AppendLine(line);
                    }

                    if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase) || (!line.Contains("(") && RemoveBlock))
                    {
                        if (!RemoveBlock)
                        {
                            sw.WriteLine(atualScript.ToString());
                        }

                        atualScript.Clear();
                        InTheBlock = false;
                        RemoveBlock = false;
                    }
                }
            }
        }

        private static void ValidateIsblocked(string line, List<string> blacklist, StringBuilder atualScript)
        {
            if (Regex.IsMatch(line.Trim(), @"^CREATE\s+(TABLE|PROCEDURE)", RegexOptions.IgnoreCase))
            {
                atualScript.Clear();
                InTheBlock = true;

                string nomeBloco = Regex.Match(line, @"(CREATE|ALTER|DROP)\s+(TABLE|PROCEDURE)\s+([^\s(]+)", RegexOptions.IgnoreCase).Groups[3].Value.Trim();
                nomeBloco = nomeBloco.Replace("dbo.", " ");
                nomeBloco = nomeBloco.Replace("[dbo].", " ");
                nomeBloco = nomeBloco.Replace("[", " ");
                nomeBloco = nomeBloco.Replace("]", " ").Trim();

                if (blacklist.Contains(nomeBloco))
                {
                    RemoveBlock = true;
                }
                else
                {
                    RemoveBlock = false;
                }
            }
        }

        private static void ValidateInTheBlock(StringBuilder atualScript, string line)
        {
            foreach (var method in Methods)
            {
                if (!InTheBlock)
                {
                    if (line.Trim().StartsWith(method, StringComparison.OrdinalIgnoreCase))
                    {
                        atualScript.Clear();
                        InTheBlock = true;
                    }
                }
                else
                {
                    atualScript.AppendLine(line);
                    return;
                }
            }
        }

        private static void ValidateIfBlockFinished(StringBuilder atualScript, string line)
        {
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                if (atualScript.Length > 0)
                {
                    AddedScripts.Add(atualScript.ToString());
                    atualScript.Clear();
                }
                InTheBlock = false;
            }
        }
    }
}