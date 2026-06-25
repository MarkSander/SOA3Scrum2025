# Changelog — Casus-alignment van de Application Core

**Project**: Avans DevOps — Scrum/DevOps Project Management System
**Datum**: 2026-06-13
**Doel**: Vastleggen welke wijzigingen zijn doorgevoerd om de code volledig in lijn te
brengen met de casus-business-rules, met onderbouwing per wijziging. Dit document helpt om
tijdens het assessment elke keuze te kunnen verdedigen.

---

## Aanleiding

Een vergelijking van de oorspronkelijke code met de casus liet enkele afwijkingen zien
tussen de **werkelijke** state-transities en de **beschreven** spelregels. De design
patterns waren correct toegepast, maar enkele domeinregels weken af. Deze ronde brengt code,
casus, diagrammen en tests met elkaar in lijn — zonder de bestaande pattern-structuur te
breken.

---

## Wijzigingen

### 1. BacklogItem — state-transities conform casus
**Bestand**: `Domain/Entities/BacklogItem.cs`

| Voor | Na | Reden (casus) |
|------|----|----|
| `Doing → {ReadyForTesting, Todo}` | `Doing → {ReadyForTesting}` | Regressie naar todo loopt via de testfasen, niet vanuit Doing |
| `ReadyForTesting → {Testing, Doing}` | `ReadyForTesting → {Testing, Todo}` | *"terug naar doing kan niet"*; afgekeurd → **todo** |
| `Testing → {Tested, ReadyForTesting}` | `Testing → {Tested, Todo}` | Fout tijdens testen → werk voor developer → **todo** |
| `Tested → {Done, Testing}` | `Tested → {Done, ReadyForTesting}` | DoD afgekeurd → **ready for testing** (hertest) |

**Effect**: De toegestane overgangen komen nu exact overeen met de casus-flow
(todo → doing → ready for testing → testing → tested → done, met de juiste regressiepaden).

### 2. BacklogItem — BR-01 hard afgedwongen
**Bestand**: `Domain/Entities/BacklogItem.cs` (`ChangeState`)

`ChangeState(new DoneState())` roept nu `CanMarkAsDone()` aan en gooit een
`InvalidOperationException` als niet alle activities Done zijn. Voorheen kon een item naar
Done ondanks open activities (de regel stond alleen in een waarschuwingstekst).

> Casus: *"kan pas done zijn, indien alle onderliggende taken dat zijn"*.

### 3. BacklogItem — gerichte notificaties
**Bestand**: `Domain/Entities/BacklogItem.cs` (`NotifyOnTransition`)

Notificaties zijn nu transitie-afhankelijk: testers bij *ReadyForTesting*, scrum master bij
regressie naar *Todo*, (lead) developer bij *Done*. Voorheen ging er één generiek bericht uit.

### 4. Sprint — volwaardige lifecycle als State Pattern
**Bestand**: `Domain/Entities/Sprint.cs`

De anemische enum `{Planned, Active, Completed}` is vervangen door een echte State-machine
(`PlannedState`, `ActiveState`, `FinishedState`, `ReleasingState`, `ReleaseFailedState`,
`ClosedState`, `CancelledState`) met:
- wijzigbaarheid alleen in `Planned` (`EnsureEditable`);
- onderscheid **review**- vs **release**-sprint (`SprintType`);
- review afsluiten alleen na `UploadReviewSummary`;
- release via de development pipeline met **succes → Closed** / **fout → ReleaseFailed**;
- **retry** of **cancel** na een mislukte release;
- lock tijdens pipeline-uitvoering (`ReleasingState`);
- notificaties naar PO/SM via `NotificationChannel`.

**Effect**: Dit is een tweede, natuurlijke toepassing van het State Pattern en maakt het
bestaande Sprint-lifecycle-diagram waar.

### 5. Nieuwe rol `Tester` en kanaal `SmsChannel`
**Bestanden**: `Domain/Entities/User.cs`, `Domain/Entities/BacklogItem.cs`

`Tester` toegevoegd (krijgt notificaties bij *ready for testing*) en `SmsChannel` +
factory-case `"sms"` toegevoegd. Hiermee komen code en UML-diagrammen overeen (die deze al
toonden) en wordt *"andere media als Slack etc."* concreet ondersteund.

### 6. Tests uitgebreid en deterministisch gemaakt
**Bestanden**: `Tests/BacklogItemTests.cs`, `Tests/SprintTests.cs`, `Tests/AssemblyInfo.cs`

- BacklogItem: tests voor alle nieuwe regressiepaden, het verbod op terug-naar-doing en de
  harde BR-01-afdwinging.
- Sprint: 20 tests die de volledige lifecycle dekken (planned/active/finished, review-close,
  release succes/falen/retry/cancel, locking).
- `DisableTestParallelization` toegevoegd: de console-stubs in de pipeline schreven naar een
  door de testhost vastgelegde writer die bij parallel draaien gesloten kon zijn.

**Resultaat**: van 52 → **76 tests**, allemaal groen.

---

## Niet gewijzigd (bewust)

- De zes/zeven design patterns en hun structuur (Composite, Decorator, Strategy, Command,
  Observer, State, Factory) — die waren correct en zijn behouden.
- De stub-aard van kanalen, repositories en pipeline-acties — conform de opdracht
  (*"stub-implementaties voldoen"*).

---

## Verificatie

```bash
dotnet test
# Passed!  -  Failed: 0, Passed: 76, Skipped: 0, Total: 76
```
