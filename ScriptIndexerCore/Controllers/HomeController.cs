using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using ScriptIndexerCore.Models;

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
        public IActionResult Details(string movieID)
        {
            string currMovieId = RouteData.Values["id"].ToString();
            if (currMovieId == null)
            {
                return NotFound();
            }
            //Get the database connection  
            mongoDatabase = GetMongoDatabase();

            //fetch the details from DB and pass into view 
            var collection = mongoDatabase.GetCollection<Movie>("moviescripts");

            //obtain objectid with our guid
            var filter_id = Builders<Movie>.Filter.Eq("id", ObjectId.Parse(currMovieId));
            //not finding movie
            Movie movie = collection.Find<Movie>(filter_id).FirstOrDefault();

            //var builder = Builders<Movie>.Filter;
            //var filter = builder.Eq(x => x.Id.ToString(), currMovieId);
            //var movie = collection.Find(filter).FirstOrDefault();




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
                    return BadRequest("Unable to update Customer  " + movie.filename);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return RedirectToAction("Index");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
