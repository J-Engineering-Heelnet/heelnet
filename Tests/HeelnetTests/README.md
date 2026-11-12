# HeelnetTests

Geautomatiseerde end-to-end-testen (Selenium + MSTest) voor HeelNet.

## Mappen

| Map            | Inhoud |
| -------------- | ------ |
| `Tests/`       | Testklassen, één map per user story: `US{nr}_{Naam}/{Naam}Tests.cs`. `BaseTest` en `AssemblySetup` staan hier ook. |
| `PageObjects/` | Nog leeg. Hier komt per scherm van de app een klasse met de Selenium-selectors, zodra de schermen er zijn. |
| `Data/`        | Vaste testdata (`TestData`, `TestModels`) en de `TestDataSeeder` die de database per test terugzet. |
| `Utils/`       | Configuratie (`TestConfig`), logging (Serilog), databasehelper en versie-info. |
| `TestRuns/`    | Wordt per run aangemaakt: log, screenshots en downloads. Staat niet in git. |

## Draaien

1. Start de applicatie (vanuit de root van de repository):
   ```bash
   docker compose -f docker-compose.dev.yml up -d --build
   ```
2. Zorg dat `.env` in de root ingevuld is (`SA_PASSWORD`, `DATABASE_NAME`); de testen gebruiken die om met de database te verbinden.
3. Draai de testen:
   ```bash
   dotnet test Tests/HeelnetTests
   ```
   of via Test Explorer in Visual Studio.

Instellingen (URL, headless) staan in `heelnet.runsettings`. Zet `HEELNET_HEADLESS` op `false` om mee te kijken in de browser.
Draait de app niet, dan worden de UI-testen als *Inconclusive* overgeslagen.

## Testen schrijven

- Naamgeving: `TC{us}_{scenario}_{nr}_{Omschrijving}`, gelijk aan het testcase-document.
- Gebruik `Step(...)` voor iedere teststap, `Check(...)` voor controles (soft assert) en sluit af met `AssertAllChecks()`.
- `Require(...)` voor precondities waarna doorgaan geen zin heeft.
- Geef elementen in de Vue-code een `data-testid` en zoek ze op met `ByTestId(...)` in het Page Object.
