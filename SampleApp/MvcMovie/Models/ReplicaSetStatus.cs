﻿namespace MvcMovie.Models
{
    using MongoDB.Bson;
    using MongoDB.Driver;

    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class ReplicaSetStatus
    {
        public string name;
        public List<ServerStatus> servers;

        private ReplicaSetStatus(string name)
        {
            this.name = name;
            servers = new List<ServerStatus>();
        }

        public static ReplicaSetStatus GetReplicaSetStatus()
        {
            ReplicaSetStatus status;
            var settings = MongoDBHelper.GetServerSettings();
            settings.SlaveOk = true;
            var server = MongoServer.Create(settings);
            try
            {
                var result = server.RunAdminCommand("replSetGetStatus");
                var response = result.Response;

                BsonValue startupStatus;
                if (response.TryGetValue("startupStatus", out startupStatus))
                {
                    status = new ReplicaSetStatus("Replica Set Initializing");
                    return status;
                }

                status = new ReplicaSetStatus((string) response.GetValue("set"));
                var value = response.GetElement("members");
                var members = value.Value.AsBsonArray;
                foreach (BsonDocument member in members)
                {
                    var node = new ServerStatus();
                    foreach (BsonElement bsonElement in member.Elements)
                    {
                        switch (bsonElement.Name)
                        {
                            case "_id":
                                node.id = bsonElement.Value.ToInt32();
                                break;
                            case "name":
                                node.name = bsonElement.Value.ToString();
                                break;
                            case "health":
                                node.health = bsonElement.Value.ToInt32() == 0 ? "DOWN" : "UP";
                                break;
                            case "state":
                                node.state = bsonElement.Value.ToInt32();
                                if (node.state == 1)
                                {
                                    node.lastHeartbeat = "Not Applicable";
                                    node.pingMS = "Not Applicable";
                                }
                                break;
                            case "stateStr":
                                node.stateStr = bsonElement.Value.ToString();
                                break;
                            case "uptime":
                                break;
                            case "lastHeartbeat":
                                var hearbeat = bsonElement.Value.AsDateTime;
                                if (hearbeat != null)
                                {
                                    node.lastHeartbeat = hearbeat.ToString("yyyy-MM-dd HH:mm tt");
                                }
                                break;
                            case "optimeDate":
                                node.optimeDate = bsonElement.Value.AsDateTime;
                                break;
                            case "pingMs":
                                Double pingTime = bsonElement.Value.AsInt32;
                                node.pingMS = pingTime.ToString(CultureInfo.InvariantCulture);
                                break;
                        }
                    }
                    status.servers.Add(node);
                }
            }
            catch
            {
                status = new ReplicaSetStatus("Replica Set Unavailable");
            }
            return status;
        }
    }

    public class ServerStatus
    {
        public Int32 id { get; set; }
        public string name { get; set; }
        public string health { get; set; }
        public Int32 state { get; set; }
        public string stateStr { get; set; }
        public string lastHeartbeat { get; set; }
        public DateTime optimeDate { get; set; }
        public string pingMS { get; set; }
    }
}