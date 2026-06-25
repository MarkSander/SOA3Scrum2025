# Documentatie-index — Avans DevOps (INVT3.3 Eindopdracht)

Dit is het instappunt voor de begeleidende documentatie. Hieronder per opleverdeel waar het
bewijs staat, plus een compact compliance-overzicht tegen de beoordelingsrubric.

## Documenten

| Document | Inhoud |
|----------|--------|
| [`Domain-Business-Rules.md`](./Domain-Business-Rules.md) | **Centraal**: elke casus-business-rule → code → test. Beide state-machines, notificatiematrix, aannames. |
| [`Requirements-and-Testing.md`](./Requirements-and-Testing.md) | Functionele & non-functionele requirements, acceptatiecriteria, testaanpak, traceability-matrix. |
| [`UML-Diagrams.md`](./UML-Diagrams.md) | 14 UML-diagrammen (class, state, sequence, pattern) in PlantUML. |
| [`Rubric-Compliance.md`](./Rubric-Compliance.md) | Onderbouwing per rubric-criterium met richtscore 9-10. |
| [`Assessment-Preparation.md`](./Assessment-Preparation.md) | Reflectie op patterns, OO-principes, alternatieven, kritische vragen. |
| [`Changelog-Casus-Alignment.md`](./Changelog-Casus-Alignment.md) | Wat is aangepast om code 100% casus-conform te maken, met onderbouwing. |

## Compliance-overzicht (rubric)

| Criterium | Weging | Waar onderbouwd | Status |
|-----------|--------|-----------------|--------|
| Requirements | 10% | `Requirements-and-Testing.md` §2-4, gekoppeld aan casus via `Domain-Business-Rules.md` | ✅ beknopt + casus-gekoppeld |
| Diagrammen | 10% | `UML-Diagrams.md` (state-diagrammen = exact de geïmplementeerde code) | ✅ code = diagram |
| Patterns (toepassing) | 10% | code-comments + `Requirements-and-Testing.md` §5 + pattern-diagrammen | ✅ 6 patterns + creational, duidelijk gelokaliseerd |
| Tests & code-analyse | 10% | `Requirements-and-Testing.md` §6-7, graaf/decision tables | ✅ 76 tests, alle business rules gekoppeld |
| Pattern-toelichting | 30% | `Assessment-Preparation.md` §1-3 (code + UML + OO + alternatieven) | ✅ State 2× toegepast, alternatieven beschreven |
| Non-functionals | 30% | `Requirements-and-Testing.md` §3 + `Rubric-Compliance.md` §6 | ⚠️ koppel definitief aan echte SonarCloud-cijfers |

## Design patterns in één oogopslag

| Pattern | Type | Locatie |
|---------|------|---------|
| State | behavioral | `BacklogItem.cs` (item-workflow) **én** `Sprint.cs` (lifecycle) |
| Observer | behavioral | `BacklogItem.cs` (`Notifier`/`INotificationSubscriber`) |
| Command | behavioral | `Pipeline.cs` (`IPipelineAction`, execute/undo/retry) |
| Strategy | behavioral | `SprintReport.cs` (`IReportExportStrategy`) |
| Composite | structural | `IWorkItem`, `BacklogItem`, `Activity` |
| Decorator | structural | `SprintReport.cs` (`ReportDecorator`) |
| Factory Method | creational | `NotificationChannelFactory` |

## Verificatie

```bash
dotnet test
# Passed!  -  Failed: 0, Passed: 76, Skipped: 0, Total: 76
```

## Nog te doen vóór inleveren

1. **SonarCloud**: draai de analyse en vervang de geschatte/voorbeeldcijfers in
   `Rubric-Compliance.md` §6 en `Requirements-and-Testing.md` §3 door de **werkelijke**
   waarden + screenshots. Dit is de laatste stap richting 9-10 op de non-functionals (30%).
2. **Diagrammen exporteren**: render de PlantUML-diagrammen naar PNG en voeg ze toe aan de
   inlever-PDF.
3. **Persoonsgegevens**: vul student-naam/-nummer in waar nog `[Naam]`/`[Nummer]` staat.
