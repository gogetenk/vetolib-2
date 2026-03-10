using Sentry;

SentrySdk.Init(options =>
{
    options.Dsn = "https://50f2107bc718f33d8498bb7dc53d4555@o4511020358434816.ingest.de.sentry.io/4511020363481168";
    options.Debug = true;
    options.TracesSampleRate = 1.0;
    options.Environment = "local-test";
});

// Test 1: Capture a message
SentrySdk.CaptureMessage("Hello Sentry from Vetolib local test!");

// Test 2: Capture an exception
try
{
    throw new InvalidOperationException("Test exception from Vetolib — SRE agent demo validation");
}
catch (Exception ex)
{
    SentrySdk.CaptureException(ex);
    Console.WriteLine($"Exception captured: {ex.Message}");
}

Console.WriteLine("Events sent to Sentry. Flushing...");

// Flush to ensure events are sent before exit
await SentrySdk.FlushAsync(TimeSpan.FromSeconds(5));

Console.WriteLine("Done! Check your Sentry dashboard.");
