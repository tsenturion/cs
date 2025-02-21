using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;

namespace TestVSCode
{
    class Program
    {     
        static void Main(string[] args)
        {
            string url = "https://www.gutenberg.org/";
            string searchText = "book";

            try
            {
                WebRequest request = WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string html = reader.ReadToEnd();
                if (html.Contains(searchText))
                {
                    Console.WriteLine("Text found on the page.");
                    int index = html.IndexOf(searchText);
                    int fragmentStart = Math.Max(0, index - 50);
                    int fragmentEnd = Math.Min(100, html.Length - fragmentStart);
                    string fragment = html.Substring(fragmentStart, fragmentEnd);
                    Console.WriteLine($"Fragment: {fragment}");
                }
                else
                {
                    Console.WriteLine("Text not found on the page.");
                }
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }
}
