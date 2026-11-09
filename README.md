# HeelNet

Digitaal afspraken- en communicatieplatform voor het regionale ziekenhuis **De zachte heelmeesters**.

HeelNet vervangt het handmatige, telefonische afsprakenproces door een veilig systeem dat patiënten, huisartsen, ziekenhuispersoneel en zorgverzekeraars met elkaar verbindt.

## Inhoud

- [Projectdoel](#projectdoel)
- [Functionaliteit](#functionaliteit)
- [Tech stack](#tech-stack)
- [Repositorystructuur](#repositorystructuur)
- [Aan de slag](#aan-de-slag)
- [Omgevingen (OTAP)](#omgevingen-otap)
- [Branching en werkwijze](#branching-en-werkwijze)
- [Testen](#testen)
- [Documentatie](#documentatie)
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
| Hosting        | Azure           |
| Containers     | Docker (acceptatie en productie) |
| CI/CD          | GitHub Actions  |

## Repositorystructuur

```
heelnet/
├── client/        # Vue-applicatie
├── server/         # ASP.NET Web API + tests
├── docs/            # Projectplan, testplan, executive summaries
├── CONTRIBUTING.md
└── README.md
```

## Aan de slag

### Vereisten

- [.NET SDK](https://dotnet.microsoft.com/download) 
- [Node.js](https://nodejs.org/) LTS 
- SQL Server (lokale installatie of Docker)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Git

### Backend

```bash
cd server
dotnet restore
dotnet run
```

### Frontend

```bash
cd client
npm ci
npm run dev
```

### Configuratie

Kopieer `.env.example` naar `.env` en vul de waarden in. Commit nooit geheimen of echte patiëntgegevens.

## Omgevingen (OTAP)

| Omgeving         | Doel                                  | Branch      | Hosting          |
| ---------------- | ------------------------------------- | ----------- | ---------------- |
| Ontwikkeling     | Lokale ontwikkeling                   | `feature/*` | Lokaal           |
| Test             | Geautomatiseerde en integratietests   | `develop`   | _TODO_           |
| Acceptatie       | Acceptatie door opdrachtgever (Docker)| `release/*` | Azure            |
| Productie        | Live systeem (Docker)                 | `main`      | Azure            |

## Branching en werkwijze

- `main`: alleen productieklare code
- `develop`: integratiebranch
- `feature/<korte-naam>`: één branch per user story
- Niet direct pushen naar `main` of `develop`; werk met pull requests en minimaal één review
- Details in [CONTRIBUTING.md](CONTRIBUTING.md)

## Testen

```bash
# Backend
cd server && dotnet test

# Frontend
cd client && npm run test
```

Tests draaien automatisch bij elke pull request via GitHub Actions. De teststrategie en de Definition of Done staan beschreven in `docs/`.


