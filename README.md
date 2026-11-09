# HeelNet

Digitaal afspraken- en communicatieplatform voor het regionale ziekenhuis **De zachte heelmeesters**.

HeelNet vervangt het handmatige, telefonische afsprakenproces door een veilig systeem dat patiënten, huisartsen, ziekenhuispersoneel en zorgverzekeraars met elkaar verbindt.

## Inhoud

- [Projectdoel](#projectdoel)
- [Functionaliteit](#functionaliteit)
- [Tech stack](#tech-stack)
- [Repositorystructuur](#repositorystructuur)
- [Aan de slag](#aan-de-slag)
- [Draaien met Docker](#draaien-met-docker)
- [Omgevingen (OTAP)](#omgevingen-otap)
- [Branching en werkwijze](#branching-en-werkwijze)
- [Testen](#testen)
- [Team](#team)

## Projectdoel

Het ziekenhuis gebruikt nu een handmatig afsprakensysteem dat inefficiënt en foutgevoelig is: lange wachttijden, dubbele boekingen van zorgpersoneel en telefonische of e-mailcommunicatie met huisartsen en zorgverzekeraars. HeelNet digitaliseert dit, vermindert administratieve fouten en verhoogt de patiënttevredenheid. Omdat medische gegevens zeer privacygevoelig zijn, is toegang beperkt tot gebruikers met een behandelrelatie en wordt elke handeling vastgelegd in een audit trail.

## Functionaliteit

- **Patiëntenportaal**: afspraken maken en het eigen medisch dossier inzien
- **Huisartsenportaal**: patiënten doorverwijzen, medische gegevens opsturen en onderzoeksuitslagen opvragen
- **Digitaal loket voor zorgverzekeraars**: het ziekenhuis dient declaraties in
- **Audit trail**: alle handelingen in het systeem worden vastgelegd

## Tech stack

| Laag           | Technologie     |
| -------------- | --------------- |
| Frontend       | Vue 3           |
| Backend        | ASP.NET (C#)    |
| Database       | SQL Server      |
| Tests          | MSTest (backend), Vitest (frontend) |
| Hosting        | Azure           |
| Containers     | Docker          |
| Reverse proxy  | nginx           |
| CI/CD          | GitHub Actions  |

## Repositorystructuur

```
heelnet/
├── Client/                    # Vue-applicatie
├── Server/                    # ASP.NET Web API
├── Tests/                     # MSTest-project(en)
├── docker/nginx/              # nginx-configuratie (reverse proxy)
├── docker-compose.yml         # Acceptatie en productie
├── docker-compose.dev.yml     # Lokale ontwikkeling
├── .env.example               # Sjabloon voor lokale variabelen (in git)
├── acceptatie.env.example     # Sjabloon voor acceptatie-variabelen (in git)
├── CONTRIBUTING.md
└── README.md
```

## Aan de slag

### Vereisten

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) LTS
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (moet draaien)
- Git

### Configuratie

De configuratie (groepsletter, SA-wachtwoord, enzovoort) staat in `.env` en `acceptatie.env`. Die bestanden staan **niet** in git, zodat er geen wachtwoorden in de repository komen. In de repository staan alleen de sjablonen:

| Sjabloon                 | Kopieer naar     | Gebruikt voor           |
| ------------------------ | ---------------- | ----------------------- |
| `.env.example`           | `.env`           | Lokaal draaien          |
| `acceptatie.env.example` | `acceptatie.env` | Acceptatieomgeving      |

**Eenmalig instellen na het clonen van de repository** (terminal):

```bash
cp .env.example .env
cp acceptatie.env.example acceptatie.env
```


Open daarna beide bestanden en vul de waarden in:

- **Groepsletter**: j.
- **SA-wachtwoord**: een sterk wachtwoord dat voldoet aan het SQL Server-beleid (minimaal 8 tekens, hoofdletter, kleine letter, cijfer en speciaal teken).

Let op:

- Het wachtwoord in `.env` moet overeenkomen met wat je gebruikt om met de database te verbinden.

## Draaien met Docker

Zorg dat Docker Desktop draait, dat poort 80 vrij is en dat je `.env` en `acceptatie.env` hebt aangemaakt (zie [Configuratie](#configuratie)).

### Starten (lokale ontwikkeling)

```bash
docker compose -f docker-compose.dev.yml up -d --build
```

Dit bouwt de images en start vier containers:

| Container | Rol                                   |
| --------- | ------------------------------------- |
| `client`  | Vue-frontend                          |
| `web`     | ASP.NET Core API                      |
| `db`      | SQL Server                            |
| `proxy`   | nginx, routeert verkeer naar client en API |

### Bereikbaar op

| Onderdeel | Adres                          |
| --------- | ------------------------------ |
| Frontend  | http://localhost               |
| API       | http://localhost/api           |
| Database  | `localhost,1433` (alleen in de dev-compose) |

Verbinden met de database: server `localhost,1433`, SQL Server Authentication, login `sa`, het wachtwoord uit `.env`, en "Trust server certificate" aangevinkt.

### Nuttige commando's

```bash
# Status van de containers
docker compose -f docker-compose.dev.yml ps -a

# Logs bekijken (alle services, of één service zoals web)
docker compose -f docker-compose.dev.yml logs -f
docker compose -f docker-compose.dev.yml logs web

# Opnieuw bouwen na codewijzigingen
docker compose -f docker-compose.dev.yml up -d --build

# Stoppen
docker compose -f docker-compose.dev.yml down

# Stoppen en de database-data wissen
docker compose -f docker-compose.dev.yml down --volumes --remove-orphans
```

### Veelvoorkomende problemen

- **Poort 80 is bezet:** stop de service die de poort gebruikt.
- **SQL Server start niet:** het SA-wachtwoord voldoet niet aan het wachtwoordbeleid (minimaal 8 tekens, hoofdletter, kleine letter, cijfer en speciaal teken).
- **Database is leeg:** de Entity Framework-migraties zijn nog niet uitgevoerd.
- **Wijzigingen zijn niet zichtbaar:** bouw opnieuw met `--build`.

## Omgevingen (OTAP)

| Omgeving         | Doel                                  | Branch      | Hosting          |
| ---------------- | ------------------------------------- | ----------- | ---------------- |
| Ontwikkeling     | Lokale ontwikkeling                   | `feature/*` | Lokaal (Docker)  |
| Test             | Geautomatiseerde en integratietests   | `develop`   | Lokaal           |
| Acceptatie       | Acceptatie door opdrachtgever (Docker)| `release/*` | Azure            |
| Productie        | Live systeem (Docker)                 | `main`      | Azure            |

De database is alleen in de dev-compose van buitenaf bereikbaar. In `docker-compose.yml` (acceptatie en productie) wordt poort 1433 niet gepubliceerd.

## Branching en werkwijze

- `main`: alleen productieklare code
- `develop`: integratiebranch en defaultbranch
- `feature/<korte-naam>`: één branch per user story
- Niet direct pushen naar `main` of `develop`; werk met pull requests en minimaal één review
- Details in [CONTRIBUTING.md](CONTRIBUTING.md)

## Testen

```bash
# Backend (via de solution, zodat ook het project in Tests/ wordt gevonden)
dotnet test Server/Heelnet.slnx

# Frontend
cd Client && npm run test
```

Tests draaien automatisch bij elke pull request via GitHub Actions.
