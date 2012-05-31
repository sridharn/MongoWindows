namespace MvcMovie
{
    using System;
    using System.Collections.Generic;

    using MongoDB.Driver;

    public static class MongoDBHelper
    {
        public static MongoServerSettings GetServerSettings()
        {
            var settings = new MongoServerSettings();
            // Uncomment the following 2 lines if using RS
            // settings.ReplicaSetName = "rs";
            // settings.ConnectionMode = ConnectionMode.ReplicaSet;

            var servers = new List<MongoServerAddress>();
            servers.Add(new MongoServerAddress("localhost",27017));
            settings.Servers = servers;
            settings.ConnectionMode = ConnectionMode.ReplicaSet;
            return settings;
        }
    }
}