using Serilog;

namespace HeelnetTests.Utils
{
    public static class Log
    {
        public static ILogger Logger { get; }

        /// <summary>Eén logbestand per testrun, in de map van die run.</summary>
        public static string LogFilePath { get; }

        static Log()
        {
            LogFilePath = Path.Combine(TestConfig.RunDirectory, "TestExecutionLog.txt");

            // Naar zowel de console als een bestand loggen; {TestCase} komt uit BaseTest
            Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {TestCase} {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(LogFilePath,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {TestCase} {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
    }
}
