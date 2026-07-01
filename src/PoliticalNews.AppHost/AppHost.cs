using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Toggle where Mongo comes from without touching any downstream project:
//   true  (default) - spins up a local Mongo container, zero credentials required.
//   false            - reads a real connection string named "mongodb" from this AppHost's own
//                       configuration (user-secrets/env var), e.g. an existing Atlas cluster.
// Set via appsettings.json/user-secrets/env var: "UseLocalMongo": false.
var useLocalMongo = builder.Configuration.GetValue("UseLocalMongo", true);

var mongoConnectionString = useLocalMongo
    ? builder.AddMongoDB("mongo").WithLifetime(ContainerLifetime.Persistent).AddDatabase("mongodb")
    : builder.AddConnectionString("mongodb");

builder.AddProject<Projects.PoliticalNews_Web>("web")
    .WithReference(mongoConnectionString)
    .WaitFor(mongoConnectionString)
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.PoliticalNews_Worker>("worker")
    .WithReference(mongoConnectionString)
    .WaitFor(mongoConnectionString);

builder.Build().Run();
