using System.Net;
using System.Reflection;
using HeelnetTests.Data;
using HeelnetTests.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog.Context;

namespace HeelnetTests.Tests
{
    /// <summary>
    /// Basis voor alle Selenium-testen:
    ///  - zet per test de testdata terug (zie <see cref="TestDataSeeder"/>);
    ///  - start een (headless) Chrome-browser;
    ///  - logt iedere stap, controle en uitkomst met Serilog;
    ///  - maakt bij een gefaalde test een screenshot.
    ///
    /// Controles worden als "soft assert" verzameld met <see cref="Check"/>, zodat één test álle
    /// afwijkingen t.o.v. het verwachte resultaat rapporteert. Sluit een test af met <see cref="AssertAllChecks"/>.
    ///
    /// Naamgeving testmethoden: TC{us}_{scenario}_{nr}_{Omschrijving}, bijv. TC3_1_1_AfspraakMaken.
    /// </summary>
    public abstract class BaseTest
    {
        public TestContext TestContext { get; set; } = null!;

        protected IWebDriver Driver { get; private set; } = null!;
        protected WebDriverWait Wait { get; private set; } = null!;
        protected string DownloadDirectory { get; private set; } = null!;

        private readonly List<IWebDriver> extraDrivers = new();
        private readonly List<string> failedChecks = new();
        private IDisposable? logContext;
        private int stepNumber;

        [TestInitialize]
        public void BaseSetUp()
        {
            logContext = LogContext.PushProperty("TestCase", TestCaseId);
            Log.Logger.Information("==== START {TestName} ====", TestContext.TestName);
            var description = GetType().GetMethod(TestContext.TestName!)?.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if (description != null)
            {
                Log.Logger.Information("Doel: {Description}", description);
            }

            if (!AssemblySetup.AppIsReachable)
            {
                Assert.Inconclusive($"HeelNet draait niet op {TestConfig.BaseUrl}; UI-test overgeslagen.");
            }

            if (TestConfig.HasDatabase)
            {
                TestDataSeeder.Seed();
            }

            // Per test een eigen map, zodat een download altijd bij deze test hoort
            DownloadDirectory = Path.Combine(TestConfig.RunDirectory, "Downloads", $"{TestContext.TestName}_{DateTime.Now:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(DownloadDirectory);

            Driver = CreateDriver();
            Wait = new WebDriverWait(Driver, TestConfig.DefaultWait);
        }

        [TestCleanup]
        public void BaseTearDown()
        {
            try
            {
                var outcome = TestContext.CurrentTestOutcome;
                if (outcome == UnitTestOutcome.Passed)
                {
                    Log.Logger.Information("==== RESULTAAT {TestName}: GESLAAGD ====", TestContext.TestName);
                }
                else
                {
                    TakeScreenshot(Driver, "FAILED");
                    Log.Logger.Error("==== RESULTAAT {TestName}: {Outcome} ====", TestContext.TestName, outcome);
                }
            }
            finally
            {
                foreach (var driver in extraDrivers.Append(Driver))
                {
                    try { driver?.Quit(); } catch { /* browser al gesloten */ }
                }
                logContext?.Dispose();
            }
        }

        /// <summary>"TC3_1_1_Iets" -> "TC3.1.1"</summary>
        private string TestCaseId
        {
            get
            {
                var parts = (TestContext.TestName ?? "").Split('_');
                var numbers = parts.Skip(1).TakeWhile(p => p.Length > 0 && p.All(char.IsDigit));
                return string.Join(".", numbers.Prepend(parts[0]));
            }
        }

        // ---------------------------------------------------------------- browser

        /// <summary>Start een Chrome-browser. Een tweede aanroep geeft een extra browser (bijv. tweede gebruiker).</summary>
        protected IWebDriver CreateDriver()
        {
            var options = new ChromeOptions();
            if (TestConfig.Headless)
            {
                options.AddArgument("--headless=new");
            }
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--lang=nl-NL");
            options.AddUserProfilePreference("download.default_directory", DownloadDirectory);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            // Geen wachtwoord-popups van Chrome tijdens de test
            options.AddUserProfilePreference("profile.password_manager_leak_detection", false);
            options.AddUserProfilePreference("credentials_enable_service", false);

            var driver = new ChromeDriver(options);
            if (Driver != null)
            {
                extraDrivers.Add(driver);
            }
            return driver;
        }

        protected void TakeScreenshot(IWebDriver? driver, string label)
        {
            if (driver is not ITakesScreenshot camera) return;
            try
            {
                var dir = Path.Combine(TestConfig.RunDirectory, "Screenshots");
                Directory.CreateDirectory(dir);
                var file = Path.Combine(dir, $"{TestContext.TestName}_{label}_{DateTime.Now:HHmmss}.png");
                camera.GetScreenshot().SaveAsFile(file);
                TestContext.AddResultFile(file);
                Log.Logger.Information("Screenshot opgeslagen: {File}", file);
            }
            catch (Exception ex)
            {
                Log.Logger.Warning("Screenshot maken mislukt: {Message}", ex.Message);
            }
        }

        // ---------------------------------------------------------------- logging & controles

        /// <summary>Logt een teststap uit het testcase-document.</summary>
        protected void Step(string description)
        {
            stepNumber++;
            Log.Logger.Information("Stap {Step}: {Description}", stepNumber, description);
        }

        /// <summary>Logt een afwijking tussen testcase-document en applicatie die de test niet laat falen.</summary>
        protected static void Note(string message, params object[] args) => Log.Logger.Warning("Let op: " + message, args);

        /// <summary>Soft assert: logt het resultaat en verzamelt de fout, de test gaat door.</summary>
        protected bool Check(bool condition, string expected, string? actual = null)
        {
            if (condition)
            {
                Log.Logger.Information("  [OK]   {Expected}", expected);
            }
            else
            {
                var message = actual == null ? expected : $"{expected} | Werkelijk: {actual}";
                Log.Logger.Error("  [FOUT] {Message}", message);
                failedChecks.Add(message);
            }
            return condition;
        }

        /// <summary>Laat de test falen als één of meer controles uit <see cref="Check"/> mislukt zijn.</summary>
        protected void AssertAllChecks()
        {
            if (failedChecks.Count > 0)
            {
                Assert.Fail($"{failedChecks.Count} controle(s) wijken af van het verwachte resultaat:{Environment.NewLine}- "
                            + string.Join(Environment.NewLine + "- ", failedChecks));
            }
        }

        /// <summary>Harde assert: als een stap niet uitgevoerd kan worden heeft doorgaan geen zin.</summary>
        protected static void Require(bool condition, string message)
        {
            if (!condition)
            {
                Log.Logger.Error("  [BLOKKADE] {Message}", message);
                Assert.Fail(message);
            }
        }

        // ---------------------------------------------------------------- helpers

        protected static string Url(string relative) => TestConfig.BaseUrl + relative;

        /// <summary>Tekst op één regel en ingekort, om in het log te tonen wat er wél staat.</summary>
        protected static string Shorten(string text)
        {
            var t = text.Replace("\r", " ").Replace("\n", " ");
            return t.Length > 200 ? t[..200] + "…" : t;
        }

        // TODO: zodra HeelNet inloggen heeft: LoginAs(TestUser user) hier toevoegen (via een LoginPage-pageobject).

        /// <summary>
        /// HttpClient op de API (/api) die de cookies van de browser meestuurt, voor directe API-aanroepen
        /// als ingelogde gebruiker. Werkt de app met een JWT in localStorage, voeg dan hier de Authorization-header toe.
        /// </summary>
        protected HttpClient CreateApiClient(IWebDriver? driver = null)
        {
            driver ??= Driver;
            var cookies = new CookieContainer();
            var apiUri = new Uri(TestConfig.ApiUrl + "/");
            foreach (var cookie in driver.Manage().Cookies.AllCookies)
            {
                cookies.Add(apiUri, new System.Net.Cookie(cookie.Name, cookie.Value, "/", apiUri.Host));
            }
            var handler = new HttpClientHandler { CookieContainer = cookies, AllowAutoRedirect = false };
            return new HttpClient(handler) { BaseAddress = apiUri };
        }
    }
}
