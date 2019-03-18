using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ScriptIndexerCore.Models;
using System.IO;
using System.Threading;

namespace ScriptIndexerCore.Controllers
{
    public class HomeController : Controller
    {
        private IMongoDatabase mongoDatabase;

        //Generic method to get the mongodb database details  
        public IMongoDatabase GetMongoDatabase()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            return mongoClient.GetDatabase("scripts");
        }

        [HttpGet]
        public IActionResult Index()
        {
            //Get the database connection  
            mongoDatabase = GetMongoDatabase();

            //fetch the details from CustomerDB and pass into view  
            var result = mongoDatabase.GetCollection<Movie>("moviescripts").Find(FilterDefinition<Movie>.Empty).ToList();
            return View(result);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Movie movie)
        {
            try
            {
                //Get the database connection  
                mongoDatabase = GetMongoDatabase();
                mongoDatabase.GetCollection<Movie>("moviescripts").InsertOne(movie);
            }
            catch (Exception ex)
            {
                throw;
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(object movieID)
        {
            string currMovieId = RouteData.Values["id"].ToString();
            if (currMovieId == null)
            {
                return NotFound();
            }
            //Get the database connection  
            mongoDatabase = GetMongoDatabase();

            //fetch the details from DB and pass into view 
            IMongoCollection<Movie> collection = mongoDatabase.GetCollection<Movie>("moviescripts");

            var builder = Builders<Movie>.Filter;
            var objIdCurr = new ObjectId(currMovieId);
            var filter_id = builder.Eq("_id", objIdCurr);
            var movie = collection.Find(filter_id).FirstOrDefault();

            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //Get the database connection  
            mongoDatabase = GetMongoDatabase();
            //fetch the details from CustomerDB and pass into view  
            string idStr = id.ToString();
            Movie movie = mongoDatabase.GetCollection<Movie>("moviescripts").Find<Movie>(k => k.Id.ToString() == idStr).FirstOrDefault();
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        [HttpPost]
        public IActionResult Delete(Movie movie)
        {
            try
            {
                //Get the database connection  
                mongoDatabase = GetMongoDatabase();
                //Delete the customer record  
                var result = mongoDatabase.GetCollection<Movie>("moviescripts").DeleteOne<Movie>(k => k.Id.ToString() == movie.Id.ToString());
                if (result.IsAcknowledged == false)
                {
                    return BadRequest("Unable to Delete Movie " + movie.Id.ToString());
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //Get the database connection  
            mongoDatabase = GetMongoDatabase();
            //fetch the details from CustomerDB based on id and pass into view  
            string idStr = id.ToString();
            var movie = mongoDatabase.GetCollection<Movie>("moviescripts").Find<Movie>(k => k.Id.ToString() == idStr).FirstOrDefault();
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        [HttpPost]
        public IActionResult Edit(Movie movie)
        {
            try
            {
                //Get the database connection  
                mongoDatabase = GetMongoDatabase();
                //Build the where condition  
                var filter = Builders<Movie>.Filter.Eq("_Id", movie.Id.ToString());
                //Build the update statement   
                var updatestatement = Builders<Movie>.Update.Set("_Id", movie.Id);
                updatestatement = updatestatement.Set("filename", movie.filename);
                updatestatement = updatestatement.Set("filedate", movie.filedate);
                updatestatement = updatestatement.Set("filetype", movie.filetype);
                updatestatement = updatestatement.Set("filecontents", movie.filecontents);
                updatestatement = updatestatement.Set("filecontentsparsed", movie.filecontentsparsed);
                //fetch the details from CustomerDB based on id and pass into view  
                var result = mongoDatabase.GetCollection<Movie>("moviescripts").UpdateOne(filter, updatestatement);
                if (result.IsAcknowledged == false)
                {
                    return BadRequest("Unable to update Movie  " + movie.filename);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// This is the SiteSettings page load method and populates all the Model values for display on the page.
        /// </summary>
        /// <returns></returns>
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


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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

        public string MongoLoader(string fldrName, string collName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("d:\\sandbox\\ScriptIndexerCore\\ScriptIndexerCore\\Data\\SiteSettings.xml");
            MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
            IMongoDatabase db = dbClient.GetDatabase(doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText);
            
            switch (collName)
            {
                case "Movies":
                    var movieCollection = db.GetCollection<BsonDocument>(doc.DocumentElement.SelectSingleNode("/settings/movie_collection_name").InnerText);
                    foreach (string file in Directory.GetFiles(fldrName))
                    {
                        string contents = System.IO.File.ReadAllText(file);
                        BsonDocument movieDoc = new BsonDocument();
                        BsonElement scriptTitle = new BsonElement("FileName", file);
                        movieDoc.Add(scriptTitle);
                        BsonElement scriptContents = new BsonElement("Contents", contents);
                        movieDoc.Add(scriptContents);
                        movieCollection.InsertOne(movieDoc);
                    }
                    break;
                case "Shows":
                    var showCollection = db.GetCollection<BsonDocument>(doc.DocumentElement.SelectSingleNode("/settings/show_collection_name").InnerText);
                    foreach (string file in Directory.GetFiles(fldrName))
                    {
                        string contents = System.IO.File.ReadAllText(file);
                        BsonDocument showDoc = new BsonDocument();
                        BsonElement scriptTitle = new BsonElement("FileName", file);
                        showDoc.Add(scriptTitle);
                        BsonElement scriptContents = new BsonElement("Contents", contents);
                        showDoc.Add(scriptContents);
                        showCollection.InsertOne(showDoc);
                    }
                    break;
                case "Misc":
                    var miscCollection = db.GetCollection<BsonDocument>(doc.DocumentElement.SelectSingleNode("/settings/misc_collection_name").InnerText);
                    foreach (string file in Directory.GetFiles(fldrName))
                    {
                        string contents = System.IO.File.ReadAllText(file);
                        BsonDocument miscDoc = new BsonDocument();
                        BsonElement scriptTitle = new BsonElement("FileName", file);
                        miscDoc.Add(scriptTitle);
                        BsonElement scriptContents = new BsonElement("Contents", contents);
                        miscDoc.Add(scriptContents);
                        miscCollection.InsertOne(miscDoc);
                    }
                    break;
            }


            


            return "done";
        }

        public string MongoPurgeCollection(string collectionName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("d:\\sandbox\\ScriptIndexerCore\\ScriptIndexerCore\\Data\\SiteSettings.xml");
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
        }
        public string CurtSimpleSearch()
        {
            int Count = 0;
            XmlDocument doc = new XmlDocument();
            doc.Load("d:\\sandbox\\ScriptIndexerCore\\ScriptIndexerCore\\Data\\SiteSettings.xml");
            MongoClient dbClient = new MongoClient("mongodb://" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_path").InnerText + ":" + doc.DocumentElement.SelectSingleNode("/settings/mongodb_port").InnerText);
            var db = dbClient.GetDatabase(doc.DocumentElement.SelectSingleNode("/settings/database_name").InnerText);
            var movieCollection = db.GetCollection<searchFileByContents>(doc.DocumentElement.SelectSingleNode("/settings/movie_collection_name").InnerText);

            var filter = Builders<searchFileByContents>.Filter.Eq(x => x.FileName, "TheFileName");
            var results = movieCollection.Find(filter).ToList();
            Count = results.Count();

            return "done " + Count;
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
            //var tsk = CreateIndex();
            //var rslt = tsk.WaitAndUnwrapException();
            //var result = AsyncContext.RunTask(CreateIndex).Result;
            CreateIndex().GetAwaiter().GetResult();
            return "OK";
        }
        static async Task CreateIndex()
        {
            //var client = new MongoClient();
            //var database = client.GetDatabase("ScriptIndexer");
            //var collection = database.GetCollection<searchFileByContents>("ScriptIndexerMovieCollection");
            //await collection.Indexes.CreateOneAsync(Builders<searchFileByContents>.IndexKeys.Ascending(_ => _.FileName));

            //var client = new MongoClient("mongodb://localhost");
            //var db = client.GetDatabase("ScriptIndexer");
            //var collection = db.GetCollection<searchFileByContents>("Hamsters");
            //collection.Indexes.CreateOne(Builders<searchFileByContents>.IndexKeys.Ascending(_ => _.FileName));

            var client = new MongoClient();
            var database = client.GetDatabase("ScriptIndexer");
            var collection = database.GetCollection<searchFileByContents>("ScriptIndexerMovieCollection");
            var notificationLogBuilder = Builders<searchFileByContents>.IndexKeys;
            var indexModel = new CreateIndexModel<searchFileByContents>(notificationLogBuilder.Ascending(x => x.FileName));
            await collection.Indexes.CreateOneAsync(indexModel).ConfigureAwait(false);

        }


    }
}
