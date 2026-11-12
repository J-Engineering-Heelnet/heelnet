using HeelnetTests.Utils;

namespace HeelnetTests.Tests
{
    [TestClass]
    public static class AssemblySetup
    {
        /// <summary>Is de applicatie bereikbaar? Zo niet, dan worden de UI-testen overgeslagen (Inconclusive).</summary>
        public static bool AppIsReachable { get; private set; }

        /// <summary>
        /// Start van de testrun: legt vast welke versie getest wordt en controleert dat
        /// de applicatie draait en (indien ingesteld) de database bereikbaar is.
        /// </summary>
        [AssemblyInitialize]
        public static void CheckEnvironment(TestContext _)
        {
            Log.Logger.Information("==== TESTRUN {Started:yyyy-MM-dd HH:mm:ss} ====", TestConfig.RunStarted);
            Log.Logger.Information("Geteste versie     : {Version}", AppVersionInfo.Current);
            Log.Logger.Information("Applicatie-URL     : {BaseUrl}", TestConfig.BaseUrl);
            Log.Logger.Information("API-URL            : {ApiUrl}", TestConfig.ApiUrl);
            Log.Logger.Information("Resultaten van deze run: {RunDirectory}", TestConfig.RunDirectory);

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            try
            {
                http.GetAsync(TestConfig.BaseUrl).GetAwaiter().GetResult();
                AppIsReachable = true;
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal("HeelNet is niet bereikbaar op {BaseUrl}. Start de app eerst met " +
                                 "'docker compose -f docker-compose.dev.yml up -d --build'. Fout: {Message}",
                                 TestConfig.BaseUrl, ex.Message);
            }

            if (TestConfig.HasDatabase)
            {
                new MsSqlDatabaseHelper().ExecuteQuery("SELECT 1");
                Log.Logger.Information("Database           : bereikbaar");
            }
            else
            {
                Log.Logger.Warning("Database           : niet ingesteld (geen SA_PASSWORD in .env); testdata wordt niet geseed");
            }
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            Log.Logger.Information("==== TESTRUN AFGEROND ({Version}) ====", AppVersionInfo.Current);
            (Log.Logger as IDisposable)?.Dispose();
        }
    }
}
