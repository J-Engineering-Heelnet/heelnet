# Bijdragen aan HeelNet

Deze afspraken gelden voor alle teamleden. Ze zorgen voor een gestructureerde en traceerbare werkwijze (versiebeheer, peer reviews, OTAP).

## Branches

| Branch            | Doel                                   | Regels                                         |
| ----------------- | -------------------------------------- | ---------------------------------------------- |
| `main`            | Productie                              | Alleen merges vanuit `release/*`, beschermd    |
| `develop`         | Integratie en testomgeving             | Alleen merges via pull request, beschermd      |
| `release/<versie>`| Acceptatie (bijv. `release/v1.0`)      | Aangemaakt vanaf `develop`, alleen bugfixes    |
| `feature/<naam>`  | Eén user story                         | Aangemaakt vanaf `develop`                     |
| `bugfix/<naam>`   | Defect uit test of acceptatie          | Aangemaakt vanaf `develop` (of `release/*`)    |

Branchnamen bevatten bij voorkeur de userstory nummer: `feature/12-afspraak-maken`.

**Er wordt nooit direct gepusht naar `main` of `develop`.**

## Werkwijze per user story

1. Pak een user story op die voldoet aan de **Definition of Ready** (acceptance criteria zijn duidelijk) en zet het op "In progress".
2. Maak een `feature/`-branch vanaf `develop`.
3. Commit regelmatig met duidelijke berichten.
4. Open een pull request naar `develop` en koppel het issue.
5. Wacht op review en pas feedback aan.
6. Na goedkeuring en een geslaagde CI-build merge je (squash merge) en verwijder je de branch.

## Commitberichten

Formaat: `type: korte beschrijving (#issue)`

Types: `feat`, `fix`, `test`, `docs`, `refactor`

Voorbeelden:
- `feat: afspraak boeken in patiëntenportaal (#12)`
- `fix: dubbele boeking van zorgpersoneel voorkomen (#27)`
- `test: scenario's voor audit trail toegevoegd (#31)`

## Pull requests en reviews

- Elke PR heeft minimaal **1 goedkeuring** van een ander teamlid.
- Je keurt je eigen PR nooit goed.
- De CI-build en alle tests moeten slagen.
- Reviewers controleren: werkt het volgens de acceptance criteria, zijn er tests, is de code leesbaar, is de documentatie bijgewerkt.
- Review-opmerkingen blijven in de PR staan als bewijs van de peer review.
- Reageer binnen 2 werkdagen op een reviewverzoek.

## Definition of Done

Een user story is af als:

- [ ] Alle acceptance criteria zijn gerealiseerd
- [ ] Unit- en integratietests zijn geschreven en slagen
- [ ] De code is gereviewd en goedgekeurd door minimaal één teamlid
- [ ] De CI-build is geslaagd
- [ ] Documentatie is bijgewerkt waar nodig
- [ ] De story is gemerged naar `develop` en getest in de testomgeving
