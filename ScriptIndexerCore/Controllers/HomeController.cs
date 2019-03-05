using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
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
                Mongodb_port = doc.DocumentElement.SelectSingleNode("/settings/mongodb_port")?.InnerText
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

        public string SaveSettings(string mongoPath, string mongoPort)
        {
            String Ret = "OK";

            XmlDocument doc = new XmlDocument();
            doc.Load("d:\\sandbox\\ScriptIndexerCore\\ScriptIndexerCore\\Data\\SiteSettings.xml");
            doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText = mongoPath;
            doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText = mongoPort;
            doc.Save("d:\\sandbox\\ScriptIndexerCore\\ScriptIndexerCore\\Data\\SiteSettings.xml");

            return Ret;
        }
    }
}
