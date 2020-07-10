using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using mshtml;
using System.Diagnostics;

namespace DicoDef
{
    class Program
    {
        static void Main(string[] args)
        {
            creerDictionnaire("H:/Mes Fichiers/Ressources Programmation/lexique/dictEtDef.txt", "H:/Mes Fichiers/Ressources Programmation/lexique/liste_francais_reduite.txt");
        }
        public static string telechargerWebWC(string URL)
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            Stream data = client.OpenRead(new Uri(URL)) ;
            var htmldoc2 = (IHTMLDocument2)new HTMLDocument();
            StreamReader sr = new StreamReader(data,Encoding.UTF8);
            string txt = sr.ReadToEnd();
            htmldoc2.write(txt);
            string plainText = htmldoc2.body.outerText;
            return plainText;
        }
        public static async Task<string> telechargerWeb(string URL)
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromTicks((int)(TimeSpan.TicksPerSecond*4));
            HttpResponseMessage response = await client.GetAsync(new Uri(URL));
            response.EnsureSuccessStatusCode();
            Stream data = await response.Content.ReadAsStreamAsync();
            var htmldoc2 = (IHTMLDocument2)new HTMLDocument();
            StreamReader sr = new StreamReader(data, Encoding.UTF8);
            string txt = sr.ReadToEnd();
            htmldoc2.write(txt);
            string plainText = htmldoc2.body.outerText;
            return plainText;
        }
        public static string extraireDef(string Text)
        {
            string[] lignes = Text.Split('\n');
            string retour = "";
            bool ecrire=false;
            for (int i = 0; i < lignes.Length; i++)
            {
                string l = "";
                if (!ecrire && lignes[i].Length >= 15)
                {
                    l=(lignes[i].Substring(0, 15));
                }
                if (!ecrire && lignes[i].Length>=15 && l== "Définitions de ")
                {
                    ecrire = true;
                    i++;
                }
                if(lignes[i].Contains("VOUS CHERCHEZ PEUT-ÊTRE"))
                {
                    break;
                }
                if(ecrire && lignes[i] != "\r")
                {
                    retour += lignes[i].Replace("\r","") + " ";
                }
            }
            return retour;
        }
        public static string telechargerDef(string mot)
        {
            return extraireDef(telechargerWeb("https://www.larousse.fr/dictionnaires/francais/" + mot).Result);
        }

        public static List<string> listeMotsDef(string mot)
        {
            string st = TrimPunctuation(telechargerDef(mot));
            string[] mots = st.Split();
            List<string> ret = new List<string>();
            foreach(string s in mots)
            {
                if(!ret.Contains(s))
                {
                    ret.Add(s);
                }
            }
            return ret;
        }

        static string TrimPunctuation(string value)
        {
            string retour = "";
            foreach (char c in value)
            {
                if (!char.IsPunctuation(c))
                {
                    retour += c;
                }
            }
            return retour;
        }
        public static void creerDictionnaire(string Path, string PathListeMots)
        {
            List<string> Mots = System.IO.File.ReadAllLines(PathListeMots, Encoding.GetEncoding("iso-8859-1")).ToList<string>();
            int nb = Mots.Count;
            Console.WriteLine(nb);
            int n = 0;
            Stopwatch sw = new Stopwatch();
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@Path))
            {
                sw.Start();
                foreach (string mot in Mots)
                {
                    n++;
                    Console.WriteLine("Requete");
                    string def = "";
                    try
                    {
                        def = telechargerDef(mot);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    if (def != "")
                    {
                        file.WriteLine(mot + " = " + def + '\n');
                        long ms = sw.ElapsedMilliseconds;
                        double tpm = (double)ms / (double)n;
                        double msRestant = (tpm * (nb - n));
                        TimeSpan restant = TimeSpan.FromMilliseconds(msRestant);
                        Console.WriteLine("Restent : " + restant.ToString());
                    }
                    Console.WriteLine(n);
                }
            }
        }
    }
}
