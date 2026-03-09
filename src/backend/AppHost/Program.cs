var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var db = postgres.AddDatabase("vetolibdb");

builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();
