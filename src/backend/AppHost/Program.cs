var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var db = postgres.AddDatabase("vetolibdb");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithEndpoint(targetPort: 1025, scheme: "tcp", name: "smtp")
    .WithHttpEndpoint(targetPort: 8025, name: "ui");

var keycloak = builder.AddKeycloak("keycloak")
    .WithDataVolume("keycloak-data")
    .WithRealmImport("../../infra/keycloak")
    .WithEnvironment("KC_FEATURES", "organization")
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_METRICS_ENABLED", "true")
    .WithEnvironment("KC_TRACING_ENABLED", "true")
    .WithEnvironment("OTEL_SERVICE_NAME", "keycloak")
    .WithEnvironment("OTEL_TRACES_EXPORTER", "otlp")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://host.docker.internal:18889")
    .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf");

builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithEnvironment("Email__Provider", "smtp")
    .WithEnvironment("Email__Smtp__Host", mailhog.GetEndpoint("smtp"))
    .WithEnvironment("Email__Smtp__Port", "1025")
    .WithEnvironment("Email__Smtp__EnableSsl", "false");

builder.Build().Run();
