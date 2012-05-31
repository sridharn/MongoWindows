
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMovie.Models;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MvcMovie.Controllers
{
    public class MoviesController : Controller
    {
        private static MongoServerSettings serverSettings= null;


        private static MongoServerSettings GetMongoServerSettings()
        {
            return GetMongoServerSettings(false);
        }

        private static MongoServerSettings GetMongoServerSettings(bool force)
        {
            if (!force && (serverSettings != null))
            {
                return serverSettings;
            }
            serverSettings = MongoDBHelper.GetServerSettings();
            return serverSettings;
        }

        private MongoCollection<Movie> GetMoviesCollection()
        {
            var settings = GetMongoServerSettings();
            settings.SlaveOk = true;
            var server = MongoServer.Create(settings);
            var database = server["movies"];
            var movieCollection = database.GetCollection<Movie>("movies");
            return movieCollection;
        }

        private MongoCollection<Movie> GetMoviesCollectionForEdit()
        {
            var settings = GetMongoServerSettings();
            var server = MongoServer.Create(settings);
            var database = server["movies"];
            var movieCollection = database.GetCollection<Movie>("movies");
            return movieCollection;
        }

        //
        // GET: /Movies/

        public ViewResult Index()
        {
            var collection = GetMoviesCollection();
            var cursor = collection.FindAll();
            try
            {
                var movieList = cursor.ToList<Movie>();
                return View(movieList);
            }
            catch
            {
                serverSettings = null;
                throw;
            }
        }

        //
        // GET: /Movies/Details/5

        public ViewResult Details(string id)
        {
            var collection = GetMoviesCollection();
            var query = Query.EQ("_id", new ObjectId(id));
            try
            {
                var movie = collection.FindOne(query);
                return View(movie);
            }
            catch
            {
                serverSettings = null;
                throw;
            }
        }

        //
        // GET: /Movies/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Movies/Create

        [HttpPost]
        public ActionResult Create([Bind(Exclude = "Id")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                var collection = GetMoviesCollectionForEdit();
                try
                {
                    collection.Insert(movie);
                    return RedirectToAction("Index");
                }
                catch
                {
                    serverSettings = null;
                    throw;
                }
            }

            return View(movie);
        }

        //
        // GET: /Movies/Edit/5

        public ActionResult Edit(string id)
        {
            var collection = GetMoviesCollectionForEdit();
            var query = Query.EQ("_id", new ObjectId(id));
            try
            {
                var movie = collection.FindOne(query);
                return View(movie);
            }
            catch
            {
                serverSettings = null;
                throw;
            }
        }

        //
        // POST: /Movies/Edit/5

        [HttpPost]
        public ActionResult Edit(Movie movie)
        {
            if (ModelState.IsValid)
            {
                var collection = GetMoviesCollectionForEdit();
                try
                {
                    collection.Save(movie);
                    return RedirectToAction("Index");
                }
                catch
                {
                    serverSettings = null;
                    throw;
                }
            }
            return View(movie);
        }

        //
        // GET: /Movies/Delete/5

        public ActionResult Delete(string id)
        {
            var collection = GetMoviesCollectionForEdit();
            var query = Query.EQ("_id", new ObjectId(id));
            try
            {
                var movie = collection.FindOne(query);
                return View(movie);
            }
            catch
            {
                serverSettings = null;
                throw;
            }
        }

        //
        // POST: /Movies/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            var collection = GetMoviesCollectionForEdit();
            var query = Query.EQ("_id", new ObjectId(id));
            try
            {
                var result = collection.Remove(query);
                return RedirectToAction("Index");
            }
            catch
            {
                serverSettings = null;
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        //
        // GET: /Movies/About

        public ActionResult About()
        {
            return View(ReplicaSetStatus.GetReplicaSetStatus());
        }

    }
}