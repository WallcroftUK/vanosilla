﻿using System;

namespace Plugin.MongoLogs.Utils
{
    public class MongoLogsConfiguration
    {
        public MongoLogsConfiguration(string host, short port, string dbName, string username, string password)
        {
            Host = host;
            Port = port;
            DbName = dbName;
            Username = username;
            Password = password;
        }

        public string Host { get; }
        public short Port { get; }
        public string DbName { get; }
        public string Username { get; }
        public string Password { get; }

        public static MongoLogsConfiguration FromEnv() =>
            new(
                Environment.GetEnvironmentVariable("WINGSEMU_MONGO_HOST") ?? "localhost",
                short.Parse(Environment.GetEnvironmentVariable("WINGSEMU_MONGO_PORT") ?? "27017"),
                Environment.GetEnvironmentVariable("WINGSEMU_MONGO_DB") ?? "wingsemu_logs",
                Environment.GetEnvironmentVariable("WINGSEMU_MONGO_USERNAME") ?? "root",
                Environment.GetEnvironmentVariable("WINGSEMU_MONGO_PWD") ?? "root"
            );

        public override string ToString() => $"mongodb://{Username}:{Password}@{Host}:{Port}";
    }
}