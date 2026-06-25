# Domain Business Rules & Casus-Traceability — Avans DevOps

**Project**: Avans DevOps — Scrum/DevOps Project Management System
**Student**: Mark Sander
**Vak**: INVT3.3 Softwareontwerp & -architectuur 3 — Eindopdracht
**Doel van dit document**: Voor elke business rule uit de casus laten zien *waar* hij in de
code staat, *hoe* hij is afgedwongen en *welke test* dat bewijst. Dit is het centrale
naslagdocument voor het assessment: het toont aantoonbaar dat de application core voldoet
aan de spelregels uit de opdracht.

> Broncasus: `Domain/Entities/2122-ATIx-INVT3.3-SO&A3-EindOpdracht DesignPatterns (1).pdf`,
> sectie *Casus → Project management volgens Scrum / DevOps*.

---

## 1. Overzicht van de twee state-machines

De casus beschrijft twee kerngedragingen met strikte spelregels. Beide zijn als
**State Pattern** geïmplementeerd, zodat elke toestand zijn eigen toegestane overgangen
encapsuleert (Open/Closed + Single Responsibility).

| State-machine | Entiteit | Bestand | Aantal states |
|---------------|----------|---------|---------------|
| BacklogItem-workflow | `BacklogItem` | `Domain/Entities/BacklogItem.cs` | 6 |
| Sprint-lifecycle | `Sprint` | `Domain/Entities/Sprint.cs` | 7 |

---

## 2. BacklogItem state-machine

### 2.1 Casus-tekst (letterlijk)

> *"Backlog items van een sprint kennen de fasen todo, doing, ready for testing, testing,
> tested en done. (...) Wanneer een backlog item in de fase ready for testing komt, krijgen
> testers een notificatie. Mocht een tester erachter komen dat er toch iets ontbreekt of
> fout gaat (...), dan gaat het item terug naar todo. Terug naar doing kan niet (...).
> Overigens krijgt de scrum master een notificatie wanneer dit gebeurt (...). Wanneer een
> backlog item getest is, controleert een (lead) developer via de definition of done of het
> echt naar de done toestand mag. Mocht dat niet zo zijn, dan gaat het item eerst terug naar
> ready for testing. (...) Zo niet, dan (...) komt opnieuw in to do."*

### 2.2 Toegestane overgangen (geïmplementeerd)

| Van | Naar | Casus-grondslag |
|-----|------|-----------------|
| Todo | Doing | Werk starten bij aanvang sprint |
| Doing | ReadyForTesting | Developer levert op voor test |
| ReadyForTesting | Testing | Tester pakt het item op |
| ReadyForTesting | **Todo** | Tester ziet dat het niet klaar is → terug naar todo (**niet** doing) |
| Testing | Tested | Test geslaagd |
| Testing | **Todo** | Tijdens testen gaat iets mis → werk voor developer |
| Tested | Done | (Lead) developer keurt definition of done goed |
| Tested | **ReadyForTesting** | DoD niet akkoord → nieuwe testronde |
| Done | — | Eindtoestand binnen de sprintuitvoering |

**Expliciet verboden** (gooit `InvalidOperationException`):
- Elke overgang **terug naar Doing** behalve `Todo → Doing` → *"terug naar doing kan niet"*.
- Elke "sprong" die de volgorde overslaat (bv. `Todo → Tested`).

Code: `TodoState`/`DoingState`/`ReadyForTestingState`/`TestingState`/`TestedState`/`DoneState`
in `BacklogItem.cs` (elke `CanTransitionTo`).

### 2.3 Business rules en afdwingpunten

| ID | Business rule | Casus | Afgedwongen in | Test |
|----|---------------|-------|----------------|------|
| BR-01 | Een item mag pas naar **Done** als **alle** activities Done zijn | *"kan pas done zijn, indien alle onderliggende taken dat zijn"* | `BacklogItem.ChangeState` (harde check vóór state-wissel) + `CanMarkAsDone()` | `ChangeState_ToDone_Throws_WhenActivitiesNotComplete`, `ChangeState_ToDone_Succeeds_WhenAllActivitiesDone` |
| BR-02 | "Terug naar doing kan niet" | letterlijk | `DoingState`/`ReadyForTestingState`/`TestingState`/`TestedState` staan geen `DoingState` toe | `ChangeState_ReadyForTestingToDoing_Throws` |
| BR-03 | Afgekeurd bij (ready for) testing → terug naar **Todo** | letterlijk | `ReadyForTestingState`/`TestingState` → `TodoState` | `ChangeState_ReadyForTestingToTodo_NotifiesScrumMaster`, `ChangeState_TestingToTodo_IsAllowed` |
| BR-04 | DoD afgekeurd → terug naar **ReadyForTesting** voor hertest | letterlijk | `TestedState` → `ReadyForTestingState` | `ChangeState_TestedToReadyForTesting_IsAllowed` |
| BR-05 | Done is een eindtoestand | impliciet | `DoneState.CanTransitionTo` retourneert altijd `false` | `ChangeState_InvalidTransition_*` |

### 2.4 Notificatiematrix (Observer Pattern)

`BacklogItem.NotifyOnTransition` stuurt **gerichte** notificaties, afhankelijk van de overgang:

| Overgang | Ontvanger (casus) | Bericht bevat | Test |
|----------|-------------------|---------------|------|
| → ReadyForTesting | Testers | `"... ReadyForTesting: testers are notified."` | `ChangeState_ReadyForTesting_SendsNotification` |
| → Todo (regressie) | Scrum master | `"... scrum master is notified."` | `ChangeState_ReadyForTestingToTodo_NotifiesScrumMaster` |
| → Done | (Lead) developer | `"... Done: lead developer is notified."` | `ChangeState_Done_SendsNotification` |

De feitelijke verzending verloopt via uitwisselbare `NotificationChannel`s
(`EmailChannel`, `SlackChannel`, `SmsChannel`), aangemaakt door het
`NotificationChannelFactory` (Factory Method). Combinaties zijn mogelijk door meerdere
`NotificationChannelSubscriber`s te abonneren — conform *"via e-mail, maar ook via andere
media als Slack etc. (combinaties zijn mogelijk)"*.

---

## 3. Sprint state-machine

### 3.1 Casus-tekst (samengevat)

> *"Een sprint kan als doel hebben een deelproduct af te hebben voor een sprintreview (...)
> of een release van de software (deployment). (...) Het eerste stadium is dat de sprint
> alleen nog is gecreëerd en alleen eigenschappen als naam, begin- en einddatum gewijzigd
> kunnen worden. (...) Wanneer de sprint uitgevoerd wordt, kunnen voorgaande activiteiten
> niet meer aangepast worden. (...) Wanneer een sprint af is gelopen krijgt deze de status
> 'finished'. (...) [Release] mocht dat niet (...) dan wordt de release geannuleerd (...)
> met automatisch bericht naar de product owner én scrum master (...). Zolang de activiteiten
> van de development pipeline worden uitgevoerd, kan de sprint niet gewijzigd worden. (...)
> Bij sprints die afgesloten worden met een sprint review (...) kan [de scrum master deze]
> alleen uitvoeren wanneer hij een samenvatting van de review als document (...) heeft
> geüpload."*

### 3.2 States en toegestane acties

| State | Toegestane acties | Spelregel |
|-------|-------------------|-----------|
| `PlannedState` | `AddBacklogItem`, `Rename`, `Reschedule`, `Start`, `Cancel` | Alleen hier is de sprint wijzigbaar |
| `ActiveState` | `Finish` | Uitvoering; voorgaande activiteiten zijn gelocked |
| `FinishedState` | `UploadReviewSummary`, `CloseAfterReview` (review), `StartRelease` (release), `Cancel` | Afronding verschilt per type |
| `ReleasingState` | *(geen — gelocked)* | Pipeline draait; sprint niet wijzigbaar |
| `ReleaseFailedState` | `StartRelease` (retry), `Cancel` | Opnieuw proberen of annuleren |
| `ClosedState` | *(geen)* | Eindtoestand (succesvol/gereleased) |
| `CancelledState` | *(geen)* | Eindtoestand (geannuleerd) |

Niet-toegestane acties gooien een `InvalidOperationException` via `SprintStateBase.NotAllowed`.

### 3.3 Business rules en afdwingpunten

| ID | Business rule | Afgedwongen in | Test |
|----|---------------|----------------|------|
| BR-S1 | Eigenschappen/backlog items alleen wijzigbaar in **Planned** | `EnsureEditable` + `PlannedState` | `AddBacklogItem_WhenActive_Throws`, `Reschedule_WhenActive_Throws` |
| BR-S2 | Sprint loopt af → **Finished** | `ActiveState.Finish` | `Finish_WhenActive_MovesToFinished` |
| BR-S3 | Review-sprint sluit **alleen** na geüploade reviewsamenvatting | `FinishedState.CloseAfterReview` (check `HasReviewSummary`) | `CloseAfterReview_WithoutSummary_Throws`, `CloseAfterReview_WithSummary_ClosesAndNotifies` |
| BR-S4 | Release start de pipeline; **succes → Closed**, **fout → ReleaseFailed** (+ melding SM) | `Sprint.RunReleasePipeline` | `StartRelease_WithSuccessfulPipeline_ClosesAndNotifies`, `StartRelease_WithFailingPipeline_MovesToReleaseFailedAndNotifies` |
| BR-S5 | Na falen kan SM **retry** of **annuleren** | `ReleaseFailedState.StartRelease` / `.Cancel` | `Retry_AfterFailedRelease_CanSucceed`, `Cancel_AfterFailedRelease_MovesToCancelled` |
| BR-S6 | Tijdens pipeline is de sprint **gelocked** | `ReleasingState` (alle acties verboden) | `ReleasingState_LocksAllOperations` |
| BR-S7 | Annuleren stuurt bericht naar **PO én SM** | `Cancel` in `Planned`/`Finished`/`ReleaseFailed` | `Cancel_WhenFinished_MovesToCancelledAndNotifies` |
| BR-S8 | Alleen een **release**-sprint kan releasen; review-sprint niet | `FinishedState.StartRelease` (type-check) | `StartRelease_OnReviewSprint_Throws` |

### 3.4 Notificaties (Observer via NotificationChannel)

`Sprint.Notify` stuurt naar alle gekoppelde `NotificationChannel`s (PO/SM). Getriggerd bij:
gereleased & gesloten, release mislukt, en geannuleerd — exact de momenten die de casus
beschrijft voor automatische berichtgeving aan product owner en scrum master.

---

## 4. Overige casus-elementen (volledigheid)

| Casus-element | Implementatie | Pattern |
|---------------|---------------|---------|
| BacklogItem kan activities bevatten; effort recursief | `IWorkItem`, `BacklogItem`, `Activity` | Composite |
| Max. één developer per backlog item | `BacklogItem.AssignedDeveloper` (enkelvoudig) | — |
| Discussiethread per backlog item; bevroren bij afronding | `DiscussionThread` (`Close`/`IsClosed`) | — |
| Sprintrapportage met headers/footers/charts | `SprintReport` + `*Decorator` | Decorator |
| Rapport opslaan in pdf/png/... | `IReportExportStrategy` (`Pdf`/`Png`/`Csv`) | Strategy |
| Development pipeline (Sources→...→Deploy) met retry/rollback | `Pipeline`, `IPipelineAction` | Command |
| Notificaties via e-mail/Slack/sms | `NotificationChannel` + `NotificationChannelFactory` | Observer + Factory |
| Rollen developer/scrum master/product owner/tester | `User`-hiërarchie (`Developer`/`ScrumMaster`/`ProductOwner`/`Tester`) | — |

---

## 5. Gedocumenteerde aannames

| ID | Aanname | Onderbouwing |
|----|---------|--------------|
| A-01 | `Doing → Todo` is **niet** toegestaan; regressie naar Todo loopt uitsluitend via de testfasen | Casus benoemt regressie naar todo alleen vanuit (ready for) testing; "wisselen van taken voorkomen" |
| A-02 | `ReleasingState` is transiënt (pipeline draait synchroon) maar expliciet gemodelleerd om de lock-regel te tonen | Casus: sprint niet wijzigbaar tijdens pipeline |
| A-03 | Notificatie-ontvangers (testers/SM/PO/lead developer) zijn gemodelleerd via berichttekst + uitwisselbare kanalen, niet via rol-routing | Scope is application core; kanaalverzending is een stub |
| A-04 | Default `SprintType` = `Review` | Backwards-compatibele constructor; review is de meest voorkomende sprint |
| A-05 | Twee/drie notificatiekanalen volstaan (e-mail, Slack, sms) | Casus: "e-mail, maar ook andere media als Slack etc." — één/twee varianten volstaan |

---

## 6. Bewijs: testresultaat

```
Passed!  -  Failed: 0, Passed: 76, Skipped: 0, Total: 76
```

Alle business rules in dit document zijn gekoppeld aan minimaal één test (zie kolom *Test*).
Zie `Requirements-and-Testing.md` §7 voor de volledige traceability-matrix.
