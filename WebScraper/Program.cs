using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.IO;
using System.Globalization;
using CsvHelper;
using System.Linq;

namespace WebScraper
{
    public static class Program
    {
        static string _site = "https://g1.globo.com/";
        static ScrapingBrowser _browser = new ScrapingBrowser();

        static void Main(string[] args)
        {
            Console.WriteLine("Please enter a search term:");
            var searchTerm = Console.ReadLine();

            //getting news from site url
            Console.WriteLine("Searching for news, please wait...");
            var mainPageLinks = GetMainPageLinks(_site);

            //filtering desired news with the inputed keyword
            var news = GetPageDetails(mainPageLinks, searchTerm);

            //Export news to CSV
            ExportGigsToCSV(news, searchTerm);

        }


        static List<string> GetMainPageLinks(string url)
        {
            var homePageLinks = new List<string>();
            var html = GetHtml(url);
            var links = html.CssSelect("a");


            foreach (var link in links)
            {

                try
                {
                    //if (link.Attributes["class"].Value.Contains("feed-post-link"))
                    //{
                    //    homePageLinks.Add(link.Attributes["href"].Value);
                    //}


                    if (link.Attributes["href"].Value.Contains("/noticia"))
                    {
                        homePageLinks.Add(link.Attributes["href"].Value);

                    }

                }
                catch
                {
                    continue;
                }

            }

            return homePageLinks.Distinct().ToList();
        }

        static List<PageDetails> GetPageDetails(List<string> urls, string searchTerm)
        {
            var lstPageDetails = new List<PageDetails>();
            foreach (var url in urls)
            {
                var htmlNode = GetHtml(url);


                if (htmlNode != null)
                {

                    var pageDetails = new PageDetails();

                    pageDetails.title = htmlNode.OwnerDocument.DocumentNode.SelectNodes("//html/head/title")[0].InnerText;



                    try
                    {
                        //not working, need to follow DOM to find correct description
                        pageDetails.description = htmlNode.OwnerDocument.DocumentNode.SelectNodes("//html/head/summary")[0].InnerText;

                        //this may help
                        //(new System.Linq.SystemCore_EnumerableDebugView<HtmlAgilityPack.HtmlNode>((new System.Linq.SystemCore_EnumerableDebugView<HtmlAgilityPack.HtmlNode>((new System.Linq.SystemCore_EnumerableDebugView<HtmlAgilityPack.HtmlNode>((new System.Linq.SystemCore_EnumerableDebugView<HtmlAgilityPack.HtmlNode>((new System.Linq.SystemCore_EnumerableDebugView<HtmlAgilityPack.HtmlNode>(htmlNode.OwnerDocument.DocumentNode.ChildNodes).Items[2]).ChildNodes).Items[3]).ChildNodes).Items[1]).ChildNodes).Items[17]).ChildNodes).Items[7]).InnerText

                    }
                    catch
                    {
                        pageDetails.description = "No description";
                    }

                    pageDetails.url = url;

                    var searchTermInTitle = pageDetails.title.ToLower().Contains(searchTerm.ToLower());
                    var searchTermInDescription = pageDetails.description.ToLower().Contains(searchTerm.ToLower());

                    if (searchTermInTitle || searchTermInDescription)
                    {
                        lstPageDetails.Add(pageDetails);
                    }

                }
                else
                {
                    continue;
                }
            }

            return lstPageDetails;
        }

        static HtmlNode GetHtml(string url)
        {
            WebPage webpage;
            try
            {

                webpage = _browser.NavigateToPage(new Uri(url));

            }
            catch
            {


                Console.WriteLine("Page not found");
                return null;

            }


            return webpage.Html;


        }

        static void ExportGigsToCSV(List<PageDetails> lstPageDetails, string searchTerm)
        {
            //select output folder for csvs
            Console.WriteLine("Please select folder to export CSV:");
            var export_path = Console.ReadLine();

            Console.WriteLine("Exporting CSV, please wait");
            using (var writer = new StreamWriter(export_path + $"/{searchTerm}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(lstPageDetails);
            }
        }


    }
}
