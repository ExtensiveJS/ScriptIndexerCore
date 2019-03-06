using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ScriptIndexerCore.Models;

namespace ScriptIndexerCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SiteSettings()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("d:\\sandbox\\ScriptIndexerCore\\ScriptIndexerCore\\Data\\SiteSettings.xml");
            HomeModel model = new HomeModel
            {
                Mongodb_path = doc.DocumentElement.SelectSingleNode("/settings/mongodb_path")?.InnerText,
                Mongodb_port = doc.DocumentElement.SelectSingleNode("/settings/mongodb_port")?.InnerText,
                Database_name = doc.DocumentElement.SelectSingleNode("/settings/database_name")?.InnerText,
                Movie_collection_name = doc.DocumentElement.SelectSingleNode("/settings/movie_collection_name")?.InnerText,
                Show_collection_name = doc.DocumentElement.SelectSingleNode("/settings/show_collection_name")?.InnerText,
                Misc_collection_name = doc.DocumentElement.SelectSingleNode("/settings/misc_collection_name")?.InnerText
            };
            return View(model);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public String RunTest()
        {
            return "OK";
        }

        public String Search()
        {
            return "OK";
        }

        public string SaveSettings(string mongoPath, string mongoPort, string databaseName, string movieCollectionName, string showCollectionName, string miscCollectionName)
        {
            String Ret = "OK";

            XmlDocument doc = new XmlDocument();
            doc.Load("d:\\sandbox\\ScriptIndexerCore\\ScriptIndexerCore\\Data\\SiteSettings.xml");
            doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText = mongoPath;
            doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText = mongoPort;
            doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText = databaseName;
            doc.DocumentElement.SelectSingleNode("/settings/movie_collection_name").InnerText = movieCollectionName;
            doc.DocumentElement.SelectSingleNode("/settings/show_collection_name").InnerText = showCollectionName;
            doc.DocumentElement.SelectSingleNode("/settings/misc_collection_name").InnerText = miscCollectionName;
            doc.Save("d:\\sandbox\\ScriptIndexerCore\\ScriptIndexerCore\\Data\\SiteSettings.xml");

            return Ret;
        }

        public ActionResult TestMongo()
        {
            bool Ret = true;
            String msg = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("d:\\sandbox\\ScriptIndexerCore\\ScriptIndexerCore\\Data\\SiteSettings.xml");

                MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
                IMongoDatabase db = dbClient.GetDatabase(doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText);
                var collList = db.ListCollections().ToList();
                Console.WriteLine("The list of collections are :");
                foreach (var item in collList)
                {
                    Console.WriteLine(item);
                }
            }
            catch (Exception e)
            {
                Ret = false;
                msg = e.Message;
            }
            
            if (Ret) {
                return new StatusCodeResult(200);
            }
            else
            {
                return new StatusCodeResult(400);
            }
            
        }
    }
}
