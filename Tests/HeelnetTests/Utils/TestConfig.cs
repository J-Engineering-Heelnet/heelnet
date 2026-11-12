namespace HeelnetTests.Utils
{
    /// <summary>
    /// Centrale configuratie voor de testen. Alles is te overschrijven met omgevingsvariabelen
    /// (zie heelnet.runsettings), zodat dezelfde testen lokaal en op een andere omgeving kunnen draaien.
    /// </summary>
    public static class TestConfig
    {
        /// <summary>URL van de applicatie. Met docker-compose.dev.yml: http://localhost (nginx).</summary>
        public static string BaseUrl =>
            (Environment.GetEnvironmentVariable("HEELNET_BASE_URL") ?? "http://localhost").TrimEnd('/');

        /// <summary>Basis-URL van de API (nginx stuurt /api door naar de ASP.NET-backend).</summary>
        public static string ApiUrl =>
            (Environment.GetEnvironmentVariable("HEELNET_API_URL") ?? BaseUrl + "/api").TrimEnd('/');

        /// <summary>Zet HEELNET_HEADLESS=false om de browser zichtbaar mee te laten kijken.</summary>
        public static bool Headless =>
            !string.Equals(Environment.GetEnvironmentVariable("HEELNET_HEADLESS"), "false", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Connectiestring naar de database van de applicatie, of null als die niet ingesteld is.
        /// Volgorde: HEELNET_DB_CONNECTION, anders opgebouwd uit SA_PASSWORD en DATABASE_NAME in de .env
        /// in de root van de repository (de dev-compose publiceert SQL Server op localhost,1433).
        /// </summary>
        public static string? ConnectionString
        {
            get
            {
                var configured = Environment.GetEnvironmentVariable("HEELNET_DB_CONNECTION");
                if (!string.IsNullOrWhiteSpace(configured)) return configured;

                var env = DotEnv.Values;
                if (!env.TryGetValue("SA_PASSWORD", out var password) || password.Contains("{{")) return null;
                var database = env.GetValueOrDefault("DATABASE_NAME", "HeelnetDb");
                return $"Server=localhost,1433;Database={database};User=sa;Password={password};TrustServerCertificate=True;Encrypt=True";
            }
        }

        public static bool HasDatabase => ConnectionString != null;

        /// <summary>Label van de geteste oplevering in logs en in de naam van de testrun-map.</summary>
        public static string AppVersionLabel =>
            Environment.GetEnvironmentVariable("HEELNET_VERSION") is { Length: > 0 } label
                ? label
                : AppVersionInfo.Current.Branch ?? "onbekend";

        public static TimeSpan DefaultWait => TimeSpan.FromSeconds(10);

        /// <summary>Tijdstip waarop deze testrun is gestart (één keer per run vastgelegd).</summary>
        public static DateTime RunStarted { get; } = DateTime.Now;

        private static readonly Lazy<string> runDirectory = new(() =>
        {
            var safeLabel = string.Join("-", AppVersionLabel.Split(Path.GetInvalidFileNameChars()));
            var dir = Path.Combine(TestRunsDirectory, $"{RunStarted:yyyy-MM-dd_HH-mm-ss}_{safeLabel}");
            Directory.CreateDirectory(dir);
            return dir;
        });

        /// <summary>
        /// Eigen map per testrun (Tests/HeelnetTests/TestRuns/datum_tijd_versie) met log, screenshots en downloads.
        /// Staat buiten bin/, zodat resultaten van eerdere runs bewaard blijven. Staat in .gitignore.
        /// </summary>
        public static string RunDirectory => runDirectory.Value;

        public static string TestRunsDirectory => Path.Combine(ProjectDirectory, "TestRuns");

        /// <summary>Map van dit testproject (de map met HeelnetTests.csproj).</summary>
        public static string ProjectDirectory => Path.Combine(RepositoryRoot, "Tests", "HeelnetTests");

        /// <summary>Root van de repository: de map met docker-compose.dev.yml.</summary>
        public static string RepositoryRoot
        {
            get
            {
                // bin/Debug/net10.0 -> HeelnetTests -> Tests -> repository-root
                var dir = new DirectoryInfo(AppContext.BaseDirectory);
                while (dir != null && !File.Exists(Path.Combine(dir.FullName, "docker-compose.dev.yml")))
                {
                    dir = dir.Parent;
                }
                return dir?.FullName ?? AppContext.BaseDirectory;
            }
        }
    }

    /// <summary>Leest de (niet in git opgenomen) .env in de root van de repository.</summary>
    internal static class DotEnv
    {
        private static readonly Lazy<Dictionary<string, string>> values = new(() =>
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var file = Path.Combine(TestConfig.RepositoryRoot, ".env");
            if (!File.Exists(file)) return result;

            foreach (var raw in File.ReadAllLines(file))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith('#')) continue;
                var eq = line.IndexOf('=');
                if (eq <= 0) continue;
                result[line[..eq].Trim()] = line[(eq + 1)..].Trim().Trim('"');
            }
            return result;
        });

        public static IReadOnlyDictionary<string, string> Values => values.Value;
    }
}
