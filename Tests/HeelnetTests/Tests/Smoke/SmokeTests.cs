using OpenQA.Selenium;

namespace HeelnetTests.Tests.Smoke
{
    /// <summary>
    /// Controleert alleen of de testomgeving werkt (app bereikbaar, browser start, logging).
    /// Echte testen komen per user story in een eigen map: Tests/US{nr}_{Naam}/{Naam}Tests.cs.
    /// </summary>
    [TestClass]
    public class SmokeTests : BaseTest
    {
        [TestMethod]
        [Description("De startpagina van HeelNet laadt en de Vue-app wordt getoond.")]
        public void TC0_1_1_StartpaginaLaadt()
        {
            Step("Open de startpagina");
            Driver.Navigate().GoToUrl(Url("/"));

            Step("Controleer dat de app geladen is");
            var appLoaded = Wait.Until(d => d.FindElements(By.CssSelector("#app > *")).Count > 0);
            Check(appLoaded, "De Vue-app is gemount in #app");
            Check(!string.IsNullOrWhiteSpace(Driver.Title), "De pagina heeft een titel", Driver.Title);

            AssertAllChecks();
        }
    }
}
