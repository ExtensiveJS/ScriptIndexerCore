﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ScriptIndexerCore.Models;
using System.IO;
using System.Threading;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace ScriptIndexerCore.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
            string settingsFileName = @"\Data\SiteSettings.xml";
            SiteSettingsLocation = new DirectoryInfo(Environment.CurrentDirectory).FullName + settingsFileName;
        }

        public string SiteSettingsLocation { get; set; }

        [HttpGet]
        public IActionResult Index()
        {
            HomeModel model = new HomeModel();
            return View(model);
        }


        /// <summary>
        /// This is the SiteSettings page load method and populates all the Model values for display on the page.
        /// </summary>
        /// <returns></returns>
        public IActionResult SiteSettings()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(SiteSettingsLocation);
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

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public string SaveSettings(string mongoPath, string mongoPort, string databaseName, string movieCollectionName, string showCollectionName, string miscCollectionName)
        {
            String Ret = "OK";

            XmlDocument doc = new XmlDocument();
            doc.Load(SiteSettingsLocation);
            doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText = mongoPath;
            doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText = mongoPort;
            doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText = databaseName;
            doc.DocumentElement.SelectSingleNode("/settings/movie_collection_name").InnerText = movieCollectionName;
            doc.DocumentElement.SelectSingleNode("/settings/show_collection_name").InnerText = showCollectionName;
            doc.DocumentElement.SelectSingleNode("/settings/misc_collection_name").InnerText = miscCollectionName;
            doc.Save(SiteSettingsLocation);

            return Ret;
        }

        /// <summary>
        /// This method lets us run the TEST CONNECTION from the Settings UI.
        /// </summary>
        /// <returns></returns>
        public ActionResult TestMongo()
        {
            bool Ret = true;
            String msg = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(SiteSettingsLocation);

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

        public string MongoLoader(string fldrName, string collName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(SiteSettingsLocation);
            MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
            IMongoDatabase db = dbClient.GetDatabase(doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText);
            
            switch (collName)
            {
                case "Movies":
                    var movieCollection = db.GetCollection<BsonDocument>(doc.DocumentElement.SelectSingleNode("/settings/movie_collection_name").InnerText);
                    foreach (string file in Directory.GetFiles(fldrName))
                    {
                        string contents = System.IO.File.ReadAllText(file);
                        string strippedContent = Regex.Replace(contents, @"\t|\n|\r|[^\w]", "");
                        string strippedFileName = file.Substring(file.LastIndexOf("\\") + 1);
                        BsonDocument movieDoc = new BsonDocument();
                        BsonElement scriptTitle = new BsonElement("FileName", strippedFileName);
                        movieDoc.Add(scriptTitle);
                        BsonElement scriptContents = new BsonElement("Contents", contents);
                        movieDoc.Add(scriptContents);
                        BsonElement scriptStrippedContents = new BsonElement("StrippedContents", strippedContent);
                        movieDoc.Add(scriptStrippedContents);
                        BsonElement scriptType = new BsonElement("Category", "Movie");
                        movieDoc.Add(scriptType);
                        movieCollection.InsertOne(movieDoc);
                    }
                    break;
                case "Shows":
                    var showCollection = db.GetCollection<BsonDocument>(doc.DocumentElement.SelectSingleNode("/settings/show_collection_name").InnerText);
                    foreach (string file in Directory.GetFiles(fldrName))
                    {
                        string contents = System.IO.File.ReadAllText(file);
                        string strippedContent = Regex.Replace(contents, @"\t|\n|\r|[^\w]", "");
                        string strippedFileName = file.Substring(file.LastIndexOf("\\") + 1);
                        BsonDocument showDoc = new BsonDocument();
                        BsonElement scriptTitle = new BsonElement("FileName", strippedFileName);
                        showDoc.Add(scriptTitle);
                        BsonElement scriptContents = new BsonElement("Contents", contents);
                        showDoc.Add(scriptContents);
                        BsonElement scriptStrippedContents = new BsonElement("StrippedContents", strippedContent);
                        showDoc.Add(scriptStrippedContents);
                        BsonElement scriptType = new BsonElement("Category", "Show");
                        showDoc.Add(scriptType);
                        showCollection.InsertOne(showDoc);
                    }
                    break;
                case "Misc":
                    var miscCollection = db.GetCollection<BsonDocument>(doc.DocumentElement.SelectSingleNode("/settings/misc_collection_name").InnerText);
                    foreach (string file in Directory.GetFiles(fldrName))
                    {
                        string contents = System.IO.File.ReadAllText(file);
                        string strippedContent = Regex.Replace(contents, @"\t|\n|\r|[^\w]", "");
                        string strippedFileName = file.Substring(file.LastIndexOf("\\") + 1);
                        BsonDocument miscDoc = new BsonDocument();
                        BsonElement scriptTitle = new BsonElement("FileName", strippedFileName);
                        miscDoc.Add(scriptTitle);
                        BsonElement scriptContents = new BsonElement("Contents", contents);
                        miscDoc.Add(scriptContents);
                        BsonElement scriptStrippedContents = new BsonElement("StrippedContents", strippedContent);
                        miscDoc.Add(scriptStrippedContents);
                        BsonElement scriptType = new BsonElement("Category", "Other");
                        miscDoc.Add(scriptType);
                        miscCollection.InsertOne(miscDoc);
                    }
                    break;
            }


            


            return "done";
        }

        public string MongoPurgeCollection(string collectionName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(SiteSettingsLocation);
            MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
            var db = dbClient.GetDatabase(doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText);

            switch (collectionName){
                case "Movies":
                    var mongoMoviesCollection = db.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/movie_collection_name").InnerText);
                    var result1 = mongoMoviesCollection.DeleteMany(Builders<searchFileByContents>.Filter.Empty);
                    break;
                case "Shows":
                    var mongoShowsCollection = db.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/show_collection_name").InnerText);
                    var result2 = mongoShowsCollection.DeleteMany(Builders<searchFileByContents>.Filter.Empty);
                    break;
                case "Misc":
                    var mongoMiscCollection = db.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/misc_collection_name").InnerText);
                    var result3 = mongoMiscCollection.DeleteMany(Builders<searchFileByContents>.Filter.Empty);
                    break;
            }

            return "OK";
        }
        public class searchFileByContents
        {
            public ObjectId Id { get; set; }
            public string FileName { get; set; }
            public string Contents { get; set; }
            public string Category { get; set; }
            public string StrippedContents { get; set; }

        }
        public class searchReturn
        {
            public ObjectId Id { get; set; }
            public string FileName { get; set; }
            public string Category { get; set; }
        }

        
        public List<searchReturn> Search(string searchText, bool searchMovies, bool searchShows, bool searchMisc, string searchType, int searchQty)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            var ret = new List<searchReturn>();
            var staging = new List<searchFileByContents>();
            XmlDocument doc = new XmlDocument();
            doc.Load(SiteSettingsLocation);
            MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
            var db = dbClient.GetDatabase(doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText);
            // set the default Filter to be TEXT (and therefore Any Word)
            var filter = Builders<searchFileByContents>.Filter.Text(searchText);

            if (searchMovies)
            {
                var movieCollection = db.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/movie_collection_name").InnerText);
                switch (searchType)
                {
                    case "All":
                        searchText = Regex.Replace(searchText, @"[^\w\s]", "");
                        string[] searchTextString = searchText.Split(" ");
                        string searchTextQuoted = string.Empty;
                        foreach(string str in searchTextString)
                        {
                            searchTextQuoted = searchTextQuoted + "\"" + str + "\" ";
                        }
                        var filter0 = Builders<searchFileByContents>.Filter.Text(searchTextQuoted);
                        staging.AddRange(movieCollection.Find(filter0).Limit(searchQty).ToList());
                        break;
                    case "Any":
                        searchText = Regex.Replace(searchText, @"[^\w\s]", "");
                        var filter1 = Builders<searchFileByContents>.Filter.Text(searchText);
                        staging.AddRange(movieCollection.Find(filter1).Limit(searchQty).ToList());
                        break;
                    case "Exact":
                        searchText = Regex.Replace(searchText, @"[^\w]|\s", "");
                        var filter2 = Builders<searchFileByContents>.Filter.Regex("StrippedContents", new BsonRegularExpression(searchText, "i"));
                        staging.AddRange(movieCollection.Find(filter2).Limit(searchQty).ToList());
                        break;
                }
            }
            if (searchShows)
            {
                var showCollection = db.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/show_collection_name").InnerText);
                switch (searchType)
                {
                    case "All":
                        searchText = Regex.Replace(searchText, @"[^\w\s]", "");
                        string[] searchTextString = searchText.Split(" ");
                        string searchTextQuoted = string.Empty;
                        foreach (string str in searchTextString)
                        {
                            searchTextQuoted = searchTextQuoted + "\"" + str + "\" ";
                        }
                        var filter0 = Builders<searchFileByContents>.Filter.Text(searchTextQuoted);
                        staging.AddRange(showCollection.Find(filter0).Limit(searchQty).ToList());
                        break;
                    case "Any":
                        searchText = Regex.Replace(searchText, @"[^\w\s]", "");
                        var filter1 = Builders<searchFileByContents>.Filter.Text(searchText);
                        staging.AddRange(showCollection.Find(filter1).Limit(searchQty).ToList());
                        break;
                    case "Exact":
                        searchText = Regex.Replace(searchText, @"[^\w]|\s", "");
                        var filter2 = Builders<searchFileByContents>.Filter.Regex("StrippedContents", new BsonRegularExpression(searchText, "i"));
                        staging.AddRange(showCollection.Find(filter2).Limit(searchQty).ToList());
                        break;
                }
            }
            if (searchMisc)
            {
                var miscCollection = db.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/misc_collection_name").InnerText);
                switch (searchType)
                {
                    case "All":
                        searchText = Regex.Replace(searchText, @"[^\w\s]", "");
                        string[] searchTextString = searchText.Split(" ");
                        string searchTextQuoted = string.Empty;
                        foreach (string str in searchTextString)
                        {
                            searchTextQuoted = searchTextQuoted + "\"" + str + "\" ";
                        }
                        var filter0 = Builders<searchFileByContents>.Filter.Text(searchTextQuoted);
                        staging.AddRange(miscCollection.Find(filter0).Limit(searchQty).ToList());
                        break;
                    case "Any":
                        searchText = Regex.Replace(searchText, @"[^\w\s]", "");
                        var filter1 = Builders<searchFileByContents>.Filter.Text(searchText);
                        staging.AddRange(miscCollection.Find(filter1).Limit(searchQty).ToList());
                        break;
                    case "Exact":
                        searchText = Regex.Replace(searchText, @"[^\w]|\s", "");
                        var filter2 = Builders<searchFileByContents>.Filter.Regex("StrippedContents", new BsonRegularExpression(searchText, "i"));
                        staging.AddRange(miscCollection.Find(filter2).Limit(searchQty).ToList());
                        break;
                }
            }
            foreach(searchFileByContents rec in staging)
            {
                ret.Add(new searchReturn() { Id = rec.Id, Category = rec.Category, FileName = rec.FileName });
            }

            //logging the search time for each query
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            StringBuilder sb = new StringBuilder();
            var path = "timelog.txt";
            var now = DateTime.Now;
            sb.Append(now.ToString() + " | " + "search took: " + elapsedMs + "ms" + " | " + "searchtext: " + searchText + " | " + "searchtype: " + searchType + Environment.NewLine);
            System.IO.File.AppendAllText(path, sb.ToString());

            return ret;
        }
        
        public List<String> GetFolders(string startDir)
        {
            List<String> Ret = new List<string>();
            foreach (string fldrName in Directory.GetDirectories(startDir))
            {
                Ret.Add(fldrName);
            }
            return Ret;
        }

        public string MongoBuildIndex(string collectionName)
        {
            CreateIndex(collectionName).GetAwaiter().GetResult();
            return "OK";
        }
        public string MongoDeleteIndex(string collectionName)
        {
            DeleteIndex(collectionName).GetAwaiter().GetResult();
            return "OK";
        }
        static async Task DeleteIndex(string collectionName)
        {
            string settingsFileName = @"\Data\SiteSettings.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(new DirectoryInfo(Environment.CurrentDirectory).FullName + settingsFileName);
            MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
            var database = dbClient.GetDatabase(doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText);


            switch (collectionName)
            {
                case "Movies":
                    var movieCollection = database.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/movie_collection_name").InnerText);
                    try
                    {
                        await movieCollection.Indexes.DropOneAsync("Contents_text");
                    }
                    catch {
                        // just eat the error
                    }
                    break;
                case "Shows":
                    var showCollection = database.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/show_collection_name").InnerText);
                    try
                    {
                        await showCollection.Indexes.DropOneAsync("Contents_text");
                    }
                    catch
                    {
                        // just eat the error
                    }
                    break;
                case "Misc":
                    var otherCollection = database.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/misc_collection_name").InnerText);
                    try
                    {
                        await otherCollection.Indexes.DropOneAsync("Contents_text");
                    }
                    catch
                    {
                        // just eat the error
                    }
                    break;
            }
        }
        static async Task CreateIndex(string collectionName)
        {
            string settingsFileName = @"\Data\SiteSettings.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(new DirectoryInfo(Environment.CurrentDirectory).FullName + settingsFileName);
            MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
            var database = dbClient.GetDatabase(doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText);


            var notificationLogBuilder = Builders<searchFileByContents>.IndexKeys;
            var indexModel = new CreateIndexModel<searchFileByContents>(notificationLogBuilder.Text(x => x.Contents));

            switch (collectionName)
            {
                case "Movies":
                    var movieCollection = database.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/movie_collection_name").InnerText);
                    await movieCollection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    break;
                case "Shows":
                    var showCollection = database.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/show_collection_name").InnerText);
                    await showCollection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    break;
                case "Misc":
                    var otherCollection = database.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/misc_collection_name").InnerText);
                    await otherCollection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);
                    break;
            }

        }

        public class mongoStats
        {
            public long MovieCount { get; set; }
            public long ShowCount { get; set; }
            public long MiscCount { get; set; }
            public string MovieIndexes { get; set; }
            public string ShowIndexes { get; set; }
            public string MiscIndexes { get; set; }
        }
        public mongoStats MongoGetStats()
        {
            mongoStats ret = new mongoStats();
            ret.MiscCount = 0;
            ret.MovieCount = 0;
            ret.ShowCount = 0;

            XmlDocument doc = new XmlDocument();
            doc.Load(SiteSettingsLocation);
            MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
            var db = dbClient.GetDatabase(doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText);
            
            var movieCollection = db.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/movie_collection_name").InnerText);
            ret.MovieCount = movieCollection.EstimatedDocumentCount();
            var movieIndexList = movieCollection.Indexes.List();
            while (movieIndexList.MoveNext())
            {
                var currentIndex = movieIndexList.Current;
                foreach (var bdoc in currentIndex)
                {
                    var docNames = bdoc.Names;
                    foreach (string name in docNames)
                    {
                        if(name == "name")
                        {
                            var value = bdoc.GetValue(name);
                            ret.MovieIndexes = ret.MovieIndexes + string.Concat(name, ": ", value) + "<br />";
                        }
                        
                    }
                }
            }

            var showCollection = db.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/show_collection_name").InnerText);
            ret.ShowCount = showCollection.EstimatedDocumentCount();
            var showIndexList = showCollection.Indexes.List();
            while (showIndexList.MoveNext())
            {
                var currentIndex = showIndexList.Current;
                foreach (var bdoc in currentIndex)
                {
                    var docNames = bdoc.Names;
                    foreach (string name in docNames)
                    {
                        if (name == "name")
                        {
                            var value = bdoc.GetValue(name);
                            ret.ShowIndexes = ret.ShowIndexes + string.Concat(name, ": ", value) + "<br />";
                        }

                    }
                }
            }

            var miscCollection = db.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/misc_collection_name").InnerText);
            ret.MiscCount = miscCollection.EstimatedDocumentCount();
            var miscIndexList = miscCollection.Indexes.List();
            while (miscIndexList.MoveNext())
            {
                var currentIndex = miscIndexList.Current;
                foreach (var bdoc in currentIndex)
                {
                    var docNames = bdoc.Names;
                    foreach (string name in docNames)
                    {
                        if (name == "name")
                        {
                            var value = bdoc.GetValue(name);
                            ret.MiscIndexes = ret.MiscIndexes + string.Concat(name, ": ", value) + "<br />";
                        }

                    }
                }
            }

            return ret;
        }

        public searchFileByContents GetDetails(string id, string collectionName)
        {
            searchFileByContents ret = new searchFileByContents();
            XmlDocument doc = new XmlDocument();
            doc.Load(SiteSettingsLocation);
            MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
            var database = dbClient.GetDatabase(doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText);
            var filter_id = Builders<searchFileByContents>.Filter.Eq("Id", ObjectId.Parse(id));
            switch (collectionName)
            {
                case "Movie":
                    var movieCollection = database.GetCollection<searchFileByContents>("ScriptIndexerMovieCollection");
                    ret = movieCollection.Find(filter_id).FirstOrDefault();
                    break;
                case "Show":
                    var showCollection = database.GetCollection<searchFileByContents>("ScriptIndexerShowCollection");
                    ret = showCollection.Find(filter_id).FirstOrDefault();
                    break;
                case "Other":
                    var otherCollection = database.GetCollection<searchFileByContents>("ScriptIndexerMiscCollection");
                    ret = otherCollection.Find(filter_id).FirstOrDefault();
                    break;
            }

            return ret;
        }

        public string CreateDb(string dbName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(SiteSettingsLocation);
            MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
            var db = dbClient.GetDatabase(dbName);
            return "OK";
        }
        public string CreateColl(string collectionName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(SiteSettingsLocation);
            MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
            var db = dbClient.GetDatabase(doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText);
            db.CreateCollection(collectionName);
            return "OK";
        }
    }
}
