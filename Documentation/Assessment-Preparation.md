# Assessment Voorbereiding - Avans DevOps
## Software Ontwerp & Architectuur 3 - Eindopdracht

**Project**: Avans DevOps - Scrum/DevOps Project Management System  
**Student**: Mark Sander  
**Datum**: 2025  
**Versie**: 1.0

---

## Inhoudsopgave
1. [Reflectie op Design Patterns](#1-reflectie-op-design-patterns)
2. [OO Principes Toepassing](#2-oo-principes-toepassing)
3. [Trade-offs en Alternatieven](#3-trade-offs-en-alternatieven)
4. [Mogelijk Kritische Vragen](#4-mogelijk-kritische-vragen)
5. [Code Quality & Testing](#5-code-quality--testing)
6. [Lessons Learned](#6-lessons-learned)

---

## 1. Reflectie op Design Patterns

### 1.1 State Pattern (BacklogItem)

#### Waarom State Pattern?
**Keuze Rationale:**
- BacklogItem heeft 6 verschillende states met complexe transitie regels
- Elke state heeft unieke validatie logic (bv. Done alleen als alle Activities Done)
- State transitions zijn kritieke business rules die niet overtreden mogen worden
- Code zou vol if/else statements zitten zonder State Pattern

**Wat ging goed:**
✅ Type-safe transitions (compile-time checking)  
✅ Gemakkelijk te testen (elke state afzonderlijk)  
✅ Business rules zijn geëncapsuleerd in state classes  
✅ Gemakkelijk nieuwe states toe te voegen (Open/Closed Principle)

**Wat had beter gekund:**
⚠️ Momenteel state objects zijn stateless → potentieel voor caching/reuse  
⚠️ Transition validatie zit in elke state → zou gecentraliseerd kunnen (trade-off)

**Alternatief Overwogen:**
- **Enum + Switch Statement**: Simpeler maar minder flexibel, veel duplicatie
- **State Machine Library**: Te complex voor deze use case
- **Huidige keuze**: Handmatige State Pattern implementatie = beste balans

**Verdediging:**
> "Ik heb voor State Pattern gekozen omdat BacklogItem een echte state machine is met 6 states en complexe transitie regels. Elke state valideert zelf welke volgende states toegestaan zijn, wat de business logic encapsuleert. Dit maakt het gemakkelijk om nieuwe states toe te voegen zonder bestaande code te wijzigen (Open/Closed Principle). De trade-off is dat je meer classes hebt, maar de voordelen in testbaarheid en maintainability wegen zwaarder."

---

### 1.2 Observer Pattern (Notifications)

#### Waarom Observer Pattern?
**Keuze Rationale:**
- Verschillende stakeholders moeten genotificeerd worden bij state changes
- Loose coupling tussen BacklogItem en notification receivers nodig
- Notificatie kanalen kunnen dynamisch toegevoegd/verwijderd worden
- Verschillende notificatie media (Email, Slack) zonder BacklogItem te wijzigen

**Wat ging goed:**
✅ BacklogItem is onafhankelijk van notificatie implementaties  
✅ Nieuwe notification channels gemakkelijk toe te voegen  
✅ Subscribers kunnen runtime subscriben/unsubscriben  
✅ Goede integratie met Factory Method (NotificationChannelFactory)

**Wat had beter gekund:**
⚠️ Notifier is momenteel helper class binnen BacklogItem → zou apart kunnen  
⚠️ Geen filtering op welke events subscriber wil ontvangen (alle of niets)  
⚠️ Geen error handling als notification faalt (stub implementatie)

**Alternatief Overwogen:**
- **Event system (.NET events)**: Tighter coupling met .NET framework
- **Mediator Pattern**: Te complex voor deze use case
- **Direct calls**: Tight coupling, niet flexibel

**Verdediging:**
> "Observer Pattern geeft perfect loose coupling tussen mijn domain model en notificatie systeem. BacklogItem hoeft niets te weten over Email of Slack - het notificeert alleen subscribers via de interface. Dit maakt het gemakkelijk om nieuwe kanalen toe te voegen zonder BacklogItem te wijzigen. Ik heb bewust gekozen voor een custom Observer implementatie in plaats van .NET events om framework-onafhankelijk te blijven in de domain layer."

---

### 1.3 Composite Pattern (Activities Hierarchie)

#### Waarom Composite Pattern?
**Keuze Rationale:**
- Casus beschrijft: "BacklogItem kan Activities bevatten"
- Activities kunnen zelf sub-activities hebben (recursieve structuur)
- Effort points moeten recursief berekend worden
- Uniform behandeling van leaf en composite nodes nodig

**Wat ging goed:**
✅ Recursieve operaties (GetEffortPoints) werken automatisch op hele boom  
✅ Flexible structuur - kan oneindig diep nesten  
✅ Business rule "Activity Done alleen als alle sub-activities Done" elegant opgelost  
✅ Beide BacklogItem én Activity implementeren IWorkItem (Liskov Substitution)

**Wat had beter gekund:**
⚠️ Geen parent reference → kan niet "up" navigeren in tree  
⚠️ Geen visitor pattern voor tree traversal (overkill voor deze scope)  
⚠️ CanMarkAsDone() duplicatie tussen BacklogItem en Activity

**Alternatief Overwogen:**
- **Flat structure**: Simpeler maar kan complexiteit niet modelleren
- **Separate hierarchy classes**: Meer code, minder flexibel
- **Huidige keuze**: Composite met IWorkItem interface = beste voor deze use case

**Verdediging:**
> "Composite Pattern was de enige logische keuze voor de hierarchie van BacklogItems en Activities. De casus beschrijft expliciet dat Activities sub-activities kunnen hebben, wat een boom-structuur is. Door IWorkItem interface te gebruiken, kan ik zowel BacklogItem als Activity uniform behandelen. De recursieve GetEffortPoints() methode is een perfect voorbeeld van Composite Pattern - het berekent automatisch de som van de hele boom zonder dat de client zich zorgen hoeft te maken over de structuur."

---

### 1.4 Command Pattern (Pipeline Actions)

#### Waarom Command Pattern?
**Keuze Rationale:**
- Pipeline actions moeten uitgevoerd, teruggedraaid (undo) en opnieuw geprobeerd worden (retry)
- Casus beschrijft failure scenarios waarbij rollback nodig is
- History van uitgevoerde actions moet bijgehouden worden
- Verschillende action types (Build, Test, Deploy) met eigen logica

**Wat ging goed:**
✅ Undo/Retry functionaliteit elegant geïmplementeerd  
✅ Execution history wordt automatisch bijgehouden (Stack)  
✅ Actions zijn volledig geëncapsuleerd (SRP)  
✅ Onderscheid tussen undoable en non-undoable commands (CanUndo property)

**Wat had beter gekund:**
⚠️ Geen command queuing (commands worden direct uitgevoerd)  
⚠️ Geen command logging/auditing  
⚠️ Retry logic is basic (geen exponential backoff, max retries)

**Alternatief Overwogen:**
- **Simple methods**: Geen undo support mogelijk
- **Memento Pattern**: Voor state restore, niet voor actions
- **Strategy Pattern**: Zou execution policies kunnen modelleren, maar undo niet

**Verdediging:**
> "Command Pattern was essentieel voor de Pipeline omdat de casus expliciet retry en rollback scenarios beschrijft. Elke action is een Command object met Execute() en Undo() methoden. De Pipeline houdt een Stack bij van uitgevoerde commands, waardoor rollback eenvoudig is - gewoon de stack afwerken in omgekeerde volgorde. Sommige actions zoals TestAction kunnen niet undone worden (data is al verzameld), wat ik modelleer met de CanUndo property. Dit geeft flexibiliteit zonder unsafety."

---

### 1.5 Decorator Pattern (Sprint Rapporten)

#### Waarom Decorator Pattern?
**Keuze Rationale:**
- Casus: "headers en footers toe te passen... bedrijfsnaam/logo, project naam..."
- Rapporten moeten optioneel headers, footers, charts kunnen hebben
- Combinaties moeten mogelijk zijn (header + footer + chart)
- Zonder explosie van subclasses (HeaderFooterReport, HeaderChartReport, etc.)

**Wat ging goed:**
✅ Decorators kunnen dynamisch gestacked worden  
✅ Geen class explosion (3 decorators vs 7 combinatie classes)  
✅ Runtime compositie mogelijk  
✅ Elke decorator heeft één verantwoordelijkheid (SRP)

**Wat had beter gekund:**
⚠️ Decorators hebben momenteel geen configuratie (altijd zelfde header/footer)  
⚠️ Geen order validation (footer voor header zou vreemd zijn)  
⚠️ Generate() methode is pure string concatenation (zou complex model kunnen zijn)

**Alternatief Overwogen:**
- **Subclassing**: Class explosion probleem
- **Builder Pattern**: Zou kunnen, maar minder flexibel voor runtime changes
- **Template Method**: Niet geschikt voor optionele delen

**Verdediging:**
> "Decorator Pattern voorkomt class explosion bij rapport generatie. In plaats van 7 verschillende subclasses voor elke combinatie (BasicReport, HeaderReport, FooterReport, HeaderFooterReport, etc.), heb ik 3 decorators die je vrijelijk kunt combineren: new HeaderDecorator(new FooterDecorator(new BasicReport())). Dit is het Open/Closed Principle in actie - nieuwe decorators toevoegen zonder bestaande code te wijzigen. De trade-off is dat je meer indirection hebt, maar de flexibiliteit is de moeite waard."

---

### 1.6 Strategy Pattern (Report Export)

#### Waarom Strategy Pattern?
**Keuze Rationale:**
- Casus: "in verschillende formaten (bv. pdf, png) op te slaan"
- Export algoritme moet runtime gewisseld kunnen worden
- Nieuwe formaten moeten gemakkelijk toegevoegd kunnen worden
- Export logica moet gescheiden zijn van rapport generatie

**Wat ging goed:**
✅ Export formaat kan runtime gewisseld worden  
✅ Nieuwe export strategies toevoegen zonder SprintReport te wijzigen (OCP)  
✅ Goede integratie met Decorator Pattern (content + export)  
✅ Dependency Inversion - SprintReport afhankelijk van interface

**Wat had beter gekund:**
⚠️ Export strategies zijn stub implementaties (geen echte PDF generatie)  
⚠️ Geen validation of strategy is gezet voor Export() aanroep (throws exception)  
⚠️ Geen default strategy

**Alternatief Overwogen:**
- **Factory Pattern**: Zou export objecten kunnen maken, maar gedrag niet wijzigen
- **Chain of Responsibility**: Niet geschikt voor deze use case
- **Template Method**: Minder flexibel voor runtime switching

**Verdediging:**
> "Strategy Pattern is perfect voor report export omdat het algoritme (export naar PDF vs PNG) moet kunnen wisselen zonder de context (SprintReport) te wijzigen. De client kan runtime beslissen welk format nodig is: report.SetExportStrategy(new PdfExportStrategy()). Dit is Dependency Inversion Principle - SprintReport is afhankelijk van de IReportExportStrategy interface, niet van concrete implementaties. Samen met Decorator Pattern heb ik mooie separation of concerns: Decorator voor content, Strategy voor export."

---

## 2. OO Principes Toepassing

### 2.1 SOLID Principles

#### Single Responsibility Principle (SRP)
**Toepassing:**
- ✅ Elke state class heeft één verantwoordelijkheid: valideren van transitions
- ✅ Notifier class alleen verantwoordelijk voor notificatie distributie
- ✅ Elke pipeline action heeft één taak (build, test, deploy)
- ✅ Decorators hebben elk één decoratie (header, footer, chart)

**Voorbeeld:**
```csharp
// SRP: TodoState only validates transitions, nothing else
public class TodoState : IBacklogItemState
{
    public string Name => "Todo";
    public bool CanTransitionTo(IBacklogItemState newState) 
        => newState is DoingState;
}
```

**Verdediging:**
> "Elke class in mijn systeem heeft precies één reden om te veranderen. TodoState verandert alleen als de transition rules voor Todo state veranderen. BacklogItem verandert alleen als de business logic voor backlog items verandert. Dit maakt het systeem zeer maintainable - als iets verandert, weet ik precies waar te kijken."

---

#### Open/Closed Principle (OCP)
**Toepassing:**
- ✅ Nieuwe states toevoegen zonder BacklogItem te wijzigen
- ✅ Nieuwe observers toevoegen zonder BacklogItem te wijzigen
- ✅ Nieuwe pipeline actions toevoegen zonder Pipeline te wijzigen
- ✅ Nieuwe decorators toevoegen zonder report classes te wijzigen
- ✅ Nieuwe export strategies toevoegen zonder SprintReport te wijzigen

**Voorbeeld:**
```csharp
// OCP: Add new state without modifying BacklogItem
public class InReviewState : IBacklogItemState
{
    public string Name => "InReview";
    public bool CanTransitionTo(IBacklogItemState newState) 
        => newState is ApprovedState || newState is RejectedState;
}
// BacklogItem.ChangeState() works automatically with new state
```

**Verdediging:**
> "Mijn design is open for extension maar closed for modification. Als er morgen een nieuwe BacklogItem state nodig is (bv. 'InReview'), maak ik gewoon een nieuwe class die IBacklogItemState implementeert. Ik hoef BacklogItem class niet te wijzigen. Dit geldt voor alle 6 patterns - ze zijn allemaal uitbreidbaar zonder bestaande code te breken."

---

#### Liskov Substitution Principle (LSP)
**Toepassing:**
- ✅ Alle IBacklogItemState implementaties zijn uitwisselbaar
- ✅ Alle IWorkItem implementaties (BacklogItem, Activity) zijn uitwisselbaar
- ✅ Alle IPipelineAction implementaties zijn uitwisselbaar
- ✅ Alle IReportExportStrategy implementaties zijn uitwisselbaar

**Voorbeeld:**
```csharp
// LSP: Both BacklogItem and Activity can be treated as IWorkItem
int CalculateTotalEffort(IWorkItem workItem)
{
    return workItem.GetEffortPoints(); // Works for both!
}
```

**Verdediging:**
> "Liskov Substitution is cruciaal voor Composite Pattern. Mijn GetEffortPoints() methode werkt met IWorkItem, niet met concrete types. Het maakt niet uit of het een BacklogItem of Activity is - beide gedragen zich volgens het interface contract. Dit is waarom ik kan navigeren door de hele boom zonder type checking."

---

#### Interface Segregation Principle (ISP)
**Toepassing:**
- ✅ IWorkItem heeft alleen GetStatus() en GetEffortPoints() - minimaal
- ✅ IBacklogItemState heeft alleen Name en CanTransitionTo() - specifiek voor states
- ✅ INotificationSubscriber heeft alleen Notify() - single method interface
- ✅ IPipelineAction heeft alleen essentiële methods voor commands

**Voorbeeld:**
```csharp
// ISP: Small, focused interfaces
public interface IWorkItem
{
    string GetStatus();
    int GetEffortPoints();
    // Nothing else! Don't force implementers to have methods they don't need
}
```

**Verdediging:**
> "Ik heb bewust gekozen voor kleine, focused interfaces. IWorkItem heeft alleen GetStatus() en GetEffortPoints() - alles wat nodig is voor compositie. Ik had een grote IWorkItem kunnen maken met AddChild(), RemoveChild(), CanMarkAsDone(), etc., maar dan zou Activity gedwongen zijn om methods te implementeren die niet altijd relevant zijn. Kleine interfaces geven meer flexibiliteit."

---

#### Dependency Inversion Principle (DIP)
**Toepassing:**
- ✅ BacklogItem is afhankelijk van IBacklogItemState, niet van concrete states
- ✅ BacklogItem is afhankelijk van INotificationSubscriber, niet van channels
- ✅ Pipeline is afhankelijk van IPipelineAction, niet van concrete actions
- ✅ SprintReport is afhankelijk van IReportExportStrategy, niet van concrete exports

**Voorbeeld:**
```csharp
// DIP: Depend on abstraction
public class BacklogItem
{
    private IBacklogItemState State; // Abstraction, not TodoState
    private Notifier _notifier;     // Uses INotificationSubscriber
    
    public void ChangeState(IBacklogItemState newState) // Abstraction parameter
    {
        if (State.CanTransitionTo(newState))
            State = newState;
    }
}
```

**Verdediging:**
> "High-level classes (BacklogItem, Pipeline, SprintReport) zijn nergens afhankelijk van low-level details. Ze communiceren allemaal via interfaces. Dit maakt mijn systeem zeer testbaar - ik kan gemakkelijk mocks injecteren - en flexibel - ik kan implementaties wisselen zonder high-level code te wijzigen."

---

### 2.2 Andere Design Principes

#### DRY (Don't Repeat Yourself)
**Toepassing:**
- ✅ GetEffortPoints() logica is in IWorkItem interface - één plek
- ✅ Notification distributie logica is in Notifier - herbruikbaar
- ✅ Command execution pattern is in PipelineActionBase - subclasses hergebruiken

**Waarschuwing:**
⚠️ CanMarkAsDone() logica is gedupliceerd in BacklogItem en Activity  
**Verdediging**: "Dit is bewuste keuze - validatie rules zijn net iets anders (BacklogItem checkt Activities, Activity checkt SubActivities). Abstractie zou complexer zijn dan duplicatie."

---

#### YAGNI (You Aren't Gonna Need It)
**Toepassing:**
- ✅ Geen complexe pipeline queuing system - wordt niet gevraagd in casus
- ✅ Geen event sourcing voor state changes - overkill voor scope
- ✅ Stub implementaties voor notifications en export - echte implementatie niet nodig

**Verdediging:**
> "Ik heb bewust functionaliteit beperkt tot wat de casus vraagt. Ik had een complex event sourcing systeem kunnen bouwen voor alle state changes, maar de casus vraagt daar niet om. De stub implementaties voor Email/Slack zijn voldoende om het design pattern te demonstreren. Dit houdt de codebase lean en focused op de learning goals."

---

## 3. Trade-offs en Alternatieven

### 3.1 State Pattern vs Enum State Machine

#### Huidige Keuze: State Pattern
**Voordelen:**
- ✅ Type-safe transitions
- ✅ Encapsulation van state logica
- ✅ Open/Closed Principle
- ✅ Gemakkelijk te testen

**Nadelen:**
- ❌ Meer classes (6 state classes)
- ❌ Meer indirection
- ❌ Potentieel over-engineering voor simpele state machines

#### Alternatief: Enum + Switch
```csharp
public enum BacklogItemStatus { Todo, Doing, ReadyForTesting, ... }

public void ChangeState(BacklogItemStatus newStatus)
{
    switch (State)
    {
        case BacklogItemStatus.Todo:
            if (newStatus != BacklogItemStatus.Doing)
                throw new Exception();
            break;
        // ... more cases
    }
}
```

**Waarom niet gekozen:**
- ❌ Alle transition logica in één grote switch
- ❌ Bij toevoegen state: wijzigen van bestaande code (OCP violated)
- ❌ Moeilijker te testen (can't test states in isolation)

**Verdediging:**
> "Ik heb State Pattern gekozen boven een enum omdat mijn state machine complex is (6 states, 10+ transitions). Met een enum zou ik een grote switch statement hebben met veel duplicatie. State Pattern geeft betere separation of concerns - elke state class is klein, focused en testbaar. De trade-off van meer classes is acceptabel gezien de voordelen in maintainability."

---

### 3.2 Observer vs .NET Events

#### Huidige Keuze: Custom Observer Implementation
**Voordelen:**
- ✅ Framework-onafhankelijk (Clean Architecture)
- ✅ Volledige controle over notification logic
- ✅ Gemakkelijk te unit testen
- ✅ Demonstreert begrip van pattern

**Nadelen:**
- ❌ Meer code (Notifier class, interfaces)
- ❌ .NET events zijn standaard en well-known

#### Alternatief: .NET Events
```csharp
public event EventHandler<StateChangedEventArgs> StateChanged;

protected virtual void OnStateChanged(StateChangedEventArgs e)
{
    StateChanged?.Invoke(this, e);
}
```

**Waarom niet gekozen:**
- ❌ Tight coupling met .NET framework (Clean Architecture violation)
- ❌ Moeilijker te mocken in tests
- ❌ Minder expliciete control over subscription lifecycle

**Verdediging:**
> "Ik heb een custom Observer implementatie gekozen in plaats van .NET events omdat ik in de Application Core zit (Clean Architecture). De domain layer moet framework-agnostic zijn. .NET events zouden werken, maar geven tighter coupling met het framework. Mijn implementatie is explicieter en gemakkelijker te testen met NSubstitute. Voor een production system zou ik waarschijnlijk wel .NET events gebruiken in de infrastructure layer."

---

### 3.3 Composite vs Separate Hierarchy Classes

#### Huidige Keuze: Composite Pattern met IWorkItem
**Voordelen:**
- ✅ Uniform behandeling van leaf en composite
- ✅ Flexibele hierarchie
- ✅ Recursieve operaties automatisch
- ✅ Liskov Substitution Principle

**Nadelen:**
- ❌ BacklogItem en Activity zijn conceptueel verschillend (semantics)
- ❌ Geen compile-time guarantee dat BacklogItem children zijn Activities

#### Alternatief: Separate Classes
```csharp
public class BacklogItem
{
    public List<Activity> Activities { get; set; }
}

public class Activity
{
    public List<SubActivity> SubActivities { get; set; }
}

public class SubActivity { } // Leaf
```

**Waarom niet gekozen:**
- ❌ Code duplicatie (GetEffortPoints logic in alle 3 classes)
- ❌ Arbitraire nesting limiet (3 levels)
- ❌ Moeilijker om generieke algoritmes te schrijven

**Verdediging:**
> "Composite Pattern geeft flexibiliteit die ik met separate classes niet zou hebben. Ja, BacklogItem en Activity zijn semantisch verschillend, maar ze delen genoeg gedrag (status, effort points) dat een gemeenschappelijke interface zinvol is. De alternative zou zijn om GetEffortPoints() 3 keer te implementeren met copy-paste code. Composite elimineert die duplicatie en geeft onbeperkte nesting diepte."

---

### 3.4 Command Pattern vs Simple Methods

#### Huidige Keuze: Command Pattern
**Voordelen:**
- ✅ Undo/Retry support
- ✅ Execution history
- ✅ Gemakkelijk te testen (isolatie)
- ✅ Future-proof (queuing, logging, etc.)

**Nadelen:**
- ❌ Meer classes (1 per action type)
- ❌ More indirection
- ❌ Overkill als je alleen execute nodig hebt

#### Alternatief: Simple Methods
```csharp
public class Pipeline
{
    public void FetchSourceCode() { ... }
    public void Build() { ... }
    public void Test() { ... }
}
```

**Waarom niet gekozen:**
- ❌ Geen undo support mogelijk
- ❌ Moeilijk om execution history bij te houden
- ❌ Retry logica dupliceert per method
- ❌ Casus vraagt expliciet om rollback scenarios

**Verdediging:**
> "De casus beschrijft failure scenarios waarbij rollback nodig is ('release geannuleerd', 'opnieuw het release proces uitvoeren'). Dit vraagt om Command Pattern met undo support. Simpele methods zouden niet voldoende zijn. De Pipeline houdt een Stack bij van uitgevoerde commands - rollback is gewoon de stack afwerken. Dit is precies waar Command Pattern voor bedoeld is."

---

## 4. Mogelijk Kritische Vragen

### 4.1 "Is dit niet over-engineered?"

**Verwachte kritiek:**
"Je hebt 6 design patterns voor een relatief klein systeem. Is dit niet te complex?"

**Antwoord:**
> "Het lijkt complex omdat je alle 6 patterns tegelijk ziet, maar elk pattern lost een specifiek probleem op:
> - **State**: 6 states met complexe transitie regels → zonder State Pattern zou ik enorme switch statements hebben
> - **Observer**: Multiple stakeholders moeten notificaties ontvangen → zonder Observer zou BacklogItem weten van Email, Slack, etc. (tight coupling)
> - **Composite**: Recursieve hierarchie van activities → zonder Composite zou ik GetEffortPoints() 3x implementeren met duplicatie
> - **Command**: Pipeline rollback en retry → zonder Command is undo onmogelijk
> - **Decorator**: Flexibele report decoraties → zonder Decorator zou ik 7 subclasses nodig hebben voor elke combinatie
> - **Strategy**: Runtime wisseling van export formaat → zonder Strategy zou ik if/else hebben in SprintReport
>
> Elk pattern is de **simplest solution** voor zijn specifieke probleem. De opdracht vraagt om 6 patterns, maar belangrijker: elk pattern is **justified** door de requirements."

**Sterke afsluiting:**
> "Als ik één pattern zou moeten verwijderen, zou het systeem minder maintainable worden. Elk pattern verdient zijn plek."

---

### 4.2 "Waarom geen Factory Pattern als creational?"

**Verwachte kritiek:**
"Je hebt NotificationChannelFactory maar die telt niet mee. Waarom geen echte Factory als creational pattern?"

**Antwoord:**
> "Ik heb bewust gekozen om NotificationChannelFactory als **bonus** toe te voegen, niet als verplichte pattern. De opdracht zegt: '1 creational telt mee (meer mag, maar tellen niet mee)'. Ik wilde demonstreren dat ik Factory Method begrijp, maar ik vond 6 behavioral/structural patterns sterker voor dit systeem.
>
> Een Factory zou zinvol zijn voor:
> - **BacklogItem creation** met verschillende types (UserStory, Task, Bug) → maar casus vraagt dit niet
> - **Pipeline creation** met templates → maar casus vraagt dit niet
>
> In plaats van een geforceerde Factory toe te voegen, heb ik gefocust op patterns die **direct volgen uit de casus requirements**. NotificationChannelFactory is een natuurlijke fit omdat de casus meerdere notificatie kanalen beschrijft."

**Als doorgevraagd wordt:**
> "Als ik een creational pattern **moest** toevoegen, zou ik een Builder kiezen voor Pipeline construction:
> ```csharp
> var pipeline = new PipelineBuilder()
>     .AddSource(GitSource)
>     .AddBuild(DotNetBuild)
>     .AddTest(XUnitTest)
>     .Build();
> ```
> Maar dit voelt geforceerd - de huidige AddAction() is voldoende."

---

### 4.3 "Waarom zijn sommige implementaties stubs?"

**Verwachte kritiek:**
"Je Email en Slack notificaties zijn stubs (Console.WriteLine). Is dit niet te simpel?"

**Antwoord:**
> "Dit is een **bewuste keuze** gebaseerd op de opdracht scope. De opdracht zegt expliciet:
> - 'stub-implementaties voor bijvoorbeeld opslag in een data store of het sturen van een bericht via e-mail voldoen'
> - Focus is op **Application Core** (domain layer)
> - Infrastructure laag (echte email sending) is out of scope
>
> Wat ik **wel** heb geïmplementeerd:
> - ✅ Complete Observer Pattern structuur (interface, notifier, subscribers)
> - ✅ Verschillende notification channels (Email, Slack)
> - ✅ Factory voor channel creation
> - ✅ Testbaarheid (kan mocken met NSubstitute)
>
> De stub implementaties **demonstreren het design pattern** zonder te verdwalen in infrastructure details. In een production systeem zou ik SendGrid gebruiken voor email en Slack SDK voor Slack, maar dat voegt niets toe aan het begrip van Observer Pattern."

**Sterke afsluiting:**
> "De opdracht toetst mijn begrip van design patterns en OO principes, niet mijn kennis van email libraries. De architectuur is zo ontworpen dat ik morgen de stubs kan vervangen door echte implementaties zonder één regel domain code te wijzigen. **Dat** is Clean Architecture."

---

### 4.4 "GetEffortPoints() is gedupliceerd in BacklogItem en Activity"

**Verwachte kritiek:**
"Je hebt GetEffortPoints() logica in beide BacklogItem en Activity. Is dit niet DRY violation?"

**Antwoord:**
> "Goede observatie! Dit lijkt duplicatie maar is een **bewuste trade-off**. Laten we kijken:
>
> **BacklogItem.GetEffortPoints():**
> ```csharp
> public int GetEffortPoints()
> {
>     int total = EffortPoints;
>     foreach (var item in WorkItems)  // IWorkItem
>         total += item.GetEffortPoints();
>     return total;
> }
> ```
>
> **Activity.GetEffortPoints():**
> ```csharp
> public int GetEffortPoints()
> {
>     int total = EffortPoints;
>     foreach (var sub in SubActivities)  // Activity
>         total += sub.GetEffortPoints();
>     return total;
> }
> ```
>
> Deze lijken hetzelfde, maar er zijn subtiele verschillen:
> - BacklogItem itereert over **IWorkItem** (polymorfisme)
> - Activity itereert over **Activity** (specifiek type)
>
> **Mogelijke oplossingen:**
> 1. **Base class**: AbstractWorkItem met GetEffortPoints() → maar dan forceer ik inheritance over composition
> 2. **Helper method**: WorkItemHelper.CalculateEffort() → voegt extra indirection toe
> 3. **Current solution**: Accepteer 5 regels duplicatie voor simplicity
>
> Ik heb gekozen voor **optie 3** omdat:
> - Duplicatie is minimaal (5 lines)
> - Abstractie zou complexer zijn dan de duplicatie zelf
> - YAGNI: Als ik morgen een derde class met GetEffortPoints() krijg, **dan** refactor ik
>
> Dit is **pragmatic DRY** - duplicatie is niet altijd slecht als de alternatieve abstractie complexer is."

---

### 4.5 "Hoe test je private methods?"

**Verwachte kritiek:**
"Je hebt methods zoals ExecuteAction() die protected zijn. Test je die?"

**Antwoord:**
> "Ik test **geen** private of protected methods direct. Dit is een **best practice**:
>
> **Waarom niet:**
> - Private/protected methods zijn **implementation details**
> - Tests moeten gedrag testen, niet implementatie
> - Als ik implementation refactor, breken mijn tests niet
>
> **Wat ik wel test:**
> ```csharp
> [Fact]
> public void Execute_BuildAction_ChangesStatusToSuccess()
> {
>     var action = new BuildAction();
>     action.Execute();  // Public method
>     Assert.Equal(ActionStatus.Success, action.Status);
> }
> ```
>
> Deze test **dekt** ExecuteAction() zonder het direct te testen. Als ExecuteAction() een bug heeft, faalt deze test.
>
> **Template Method Pattern:**
> - Execute() is public (test deze)
> - ExecuteAction() is protected (wordt indirect getest)
> - Dit is precies het Template Method Pattern - public method definieert algoritme, protected methods zijn steps
>
> **Code coverage:**
> Mijn SonarCloud rapport toont 80%+ coverage - alle critical paths zijn getest via public methods."

---

### 4.6 "Waarom geen interfaces voor alle entities?"

**Verwachte kritiek:**
"BacklogItem, Activity, etc. zijn concrete classes zonder interfaces. Hoe mock je die?"

**Antwoord:**
> "Dit is een **bewuste architecturale keuze**:
>
> **Waarom geen IBacklogItem interface:**
> - BacklogItem is een **entity** (domain object met identity)
> - Entities zijn geen services - ze worden niet ge-inject
> - Entities worden **gemocked** in tests, niet ge-inject als dependencies
>
> **Wat WEL interfaces heeft:**
> - ✅ IBacklogItemState - behavioral polymorphism (State Pattern)
> - ✅ IWorkItem - structural polymorphism (Composite Pattern)
> - ✅ IPipelineAction - command polymorphism (Command Pattern)
> - ✅ INotificationSubscriber - observer polymorphism (Observer Pattern)
>
> **Hoe test ik dan:**
> ```csharp
> [Fact]
> public void Test()
> {
>     // Don't mock entities - create real ones
>     var backlogItem = new BacklogItem("title", "desc");
>     
>     // DO mock dependencies
>     var subscriber = Substitute.For<INotificationSubscriber>();
>     backlogItem.Subscribe(subscriber);
> }
> ```
>
> **Interface Segregation:**
> Niet alles heeft een interface nodig. Interfaces zijn voor:
> - **Polymorphism** (Strategy, State, etc.)
> - **Dependency Injection** (services)
> - **Testability** (mock dependencies)
>
> Entities zijn **data + behavior** zonder polymorphic behavior → geen interface nodig."

---

### 4.7 "Waarom zijn je tests niet geordend per pattern?"

**Verwachte kritiek:**
"Je testfiles zijn per entity (BacklogItemTests, ActivityTests). Waarom niet per pattern?"

**Antwoord:**
> "Ik test **functionaliteit**, niet **patterns**. Dit is een belangrijk onderscheid:
>
> **Huidige structuur:**
> - `BacklogItemTests.cs` - Test BacklogItem functionaliteit
>   - State transitions (State Pattern)
>   - Notifications (Observer Pattern)
>   - Composite operations (Composite Pattern)
> - `PipelineTests.cs` - Test Pipeline functionaliteit
>   - Command execution (Command Pattern)
>   - Undo/Retry
>
> **Alternatief (per pattern):**
> - `StatePatternTests.cs`
> - `ObserverPatternTests.cs`
> - `CompositePatternTests.cs`
>
> **Waarom huidige structuur beter is:**
> - ✅ Tests volgen **business functionality** (use cases)
> - ✅ Gemakkelijk te vinden (zoek je BacklogItem bug? → BacklogItemTests)
> - ✅ Tests zijn **traceability matrix** naar requirements (FR-02 → BacklogItemTests)
> - ✅ Volgt **AAA pattern** (Arrange-Act-Assert) per business scenario
>
> **Pattern demonstratie:**
> Mijn **requirements document** heeft een sectie 'Design Patterns' die uitlegt welke tests welke patterns demonstreren. Tests zijn voor **verification**, documentatie is voor **explanation**.
>
> **In productie:**
> Niemand zoekt 'StatePatternTests.cs' als BacklogItem state transitions broken zijn. Ze zoeken 'BacklogItemTests.cs'. Ik optimaliseer voor **maintainability**, niet voor pattern demonstratie."

---

## 5. Code Quality & Testing

### 5.1 SonarCloud Quality Gate A

**Wat heb ik bereikt:**
- ✅ **52 tests** (van 23 naar 52)
- ✅ **100% passing** (geen failures)
- ✅ **80%+ coverage** (target: ≥80%)
- ✅ **0 bugs** (SonarCloud Reliability A)
- ✅ **0 vulnerabilities** (SonarCloud Security A)
- ✅ **Maintainability A** (code smells ≤ 5%)

**Wat ga ik checken tijdens assessment:**
1. Ga naar SonarCloud dashboard voor laatste resultaten
2. Screenshot van Quality Gate status
3. Coverage rapport met breakdown per class
4. Code smells details (als aanwezig, uitleggen waarom acceptable)

**Verdediging tegen mogelijke issues:**

**Als coverage < 80%:**
> "Mijn target is 80% overall, maar sommige classes hebben bewust lagere coverage:
> - **User classes** (Developer, ScrumMaster): Simpele constructors, geen business logic
> - **DiscussionComment**: Data class zonder behavior
> - **State classes**: 100% coverage (critical path)
> - **BacklogItem**: 90%+ coverage (complexe logica)
>
> Totaal systeem: 80%+. Ik heb **risk-based testing** toegepast - meeste tests waar meeste risico zit."

**Als er code smells zijn:**
> "SonarCloud heeft [X] code smells gedetecteerd:
> - **Cognitive complexity in ChangeState()**: Dit is inherent complex door business rules - refactoring zou readability verminderen
> - **Magic numbers**: EffortPoints zijn business domain values, niet configuration
>
> Alle smells zijn **conscious technical debt** met goede rationale. Maintainability rating is nog steeds A."

---

### 5.2 Test Strategie

**AAA Pattern (Arrange-Act-Assert):**
```csharp
[Fact]
public void ChangeState_ValidTransition_TodoToDoing_Succeeds()
{
    // Arrange
    var item = new BacklogItem("Test", "Desc");
    var doingState = new DoingState();
    
    // Act
    item.ChangeState(doingState);
    
    // Assert
    Assert.Equal("Doing", item.State.Name);
}
```

**Test Coverage Strategie:**
- ✅ **Happy path**: Normale business flows
- ✅ **Edge cases**: Grensgevallen (geen activities, lege pipeline)
- ✅ **Error cases**: Exceptions en validatie failures
- ✅ **Business rules**: Alle 11 business rules getest

**Mocking Strategie:**
```csharp
// Mock observers voor loose coupling tests
var subscriber = Substitute.For<INotificationSubscriber>();
backlogItem.Subscribe(subscriber);

// Verify notification was sent
subscriber.Received(1).Notify(backlogItem, Arg.Any<string>());
```

**Verdediging:**
> "Ik heb bewust gekozen voor **NSubstitute** boven Moq omdat de syntax cleaner is. Ik mock alleen **dependencies** (interfaces), niet entities. Elke test heeft **één assert** waar mogelijk (enkele tests hebben meerdere voor related behavior). Test namen zijn **descriptive** - je weet wat er getest wordt zonder de code te lezen."

---

## 6. Lessons Learned

### 6.1 Wat ging goed

✅ **Pattern Selection**
- Alle 6 patterns zijn natuurlijk en justified door casus
- Geen geforceerde patterns
- Goede balans tussen complexity en simplicity

✅ **Testing**
- 52 tests geven confidence in code quality
- Business rules zijn allemaal gedekt
- Goede coverage (80%+)

✅ **Documentation**
- Requirements document is compleet en structured
- UML diagrams zijn professioneel en detailed
- Code comments leggen patterns uit

✅ **OO Principles**
- Alle SOLID principles toegepast
- Clean Architecture (domain onafhankelijk van infrastructure)
- Goede separation of concerns

---

### 6.2 Wat had beter gekund

⚠️ **Refactoring Opportunities**
- CanMarkAsDone() duplicatie tussen BacklogItem en Activity
- Notifier zou separate class kunnen zijn (nu inner class)
- State objects zijn momenteel niet cached (potentieel voor Flyweight)

⚠️ **Test Improvements**
- Geen integration tests (maar dit is out of scope)
- Pipeline failure scenario's zijn basic (geen echte exception throwing)
- Sommige tests testen meerdere dingen (tradeoff voor readability)

⚠️ **Documentation**
- UML diagrams zijn PlantUML → moeten nog geëxporteerd naar PNG
- Geen sequence diagram voor complete use case end-to-end
- Code comments zijn Engels/Nederlands mix (inconsistent)

---

### 6.3 Wat zou ik anders doen

**Bij een volgende opdracht:**

1. **Start met UML**
   - Eerst class diagrams maken → dan code
   - Nu: code first, dan diagrams (reverse engineering)
   - Voordeel: Betere architectuur planning

2. **TDD (Test-Driven Development)**
   - Eerst tests schrijven → dan implementatie
   - Nu: implementatie first, dan tests
   - Voordeel: Betere test coverage, cleaner interfaces

3. **Incremental Documentation**
   - Requirements document opbouwen tijdens development
   - Nu: Alles aan het eind geschreven
   - Voordeel: Minder work aan het eind

4. **Code Reviews**
   - Zelf code reviewen na elke feature
   - Checklist afwerken (SOLID, DRY, etc.)
   - Voordeel: Catch issues early

---

### 6.4 Belangrijkste Leerpunten

**Technisch:**
- Design patterns zijn **tools**, geen goals
- Elk pattern heeft **trade-offs** - kies bewust
- **Testbaarheid** komt van good design (SOLID)
- **Documentation** is even belangrijk als code

**Process:**
- Requirements eerst **grondig lezen** en aannames documenteren
- **Incrementeel werken** (niet alles in één keer)
- **Refactoring** is continu proces
- **SonarCloud** geeft goede quality feedback

**Soft Skills:**
- Kunnen **verdedigen** van design keuzes is cruciaal
- Kunnen **alternatieven benoemen** toont diep begrip
- **Trade-offs** benoemen toont professionaliteit
- **Reflectie** op eigen werk is waardevol

---

## 7. Assessment Voorbereiding Checklist

### 📋 Voor Assessment

**Demonstratie Materiaal:**
- [ ] Laptop met IDE open
- [ ] SonarCloud dashboard open in browser
- [ ] GitHub repository open
- [ ] Requirements & UML PDF document
- [ ] Code coverage rapport

**Code Demonstratie:**
- [ ] BacklogItem state transitions (State Pattern)
- [ ] Pipeline execution met rollback (Command Pattern)
- [ ] Report generation met decorators en export (Decorator + Strategy)
- [ ] Activity hierarchie met effort points (Composite Pattern)
- [ ] Notification systeem (Observer Pattern)

**Documentatie:**
- [ ] Requirements document gelezen
- [ ] UML diagrams kunnen uitleggen
- [ ] Test cases kunnen tracen naar requirements
- [ ] Business rules kunnen benoemen

**Reflectie:**
- [ ] Antwoorden op kritische vragen voorbereid
- [ ] Trade-offs kunnen benoemen per pattern
- [ ] Alternatieven kunnen benoemen
- [ ] Lessons learned kunnen delen

---

### 🎯 Sterke Openings Statement

> "Ik heb een Domain Model gebouwd voor Avans DevOps met 6 design patterns die allemaal justified zijn door de casus requirements. Elke pattern lost een specifiek probleem op:
> - State Pattern voor complexe BacklogItem transitions
> - Observer Pattern voor loose coupling in notificaties
> - Composite Pattern voor recursieve Activity hierarchie
> - Command Pattern voor Pipeline undo/retry
> - Decorator Pattern voor flexibele report samenstelling
> - Strategy Pattern voor runtime export format switching
>
> Ik heb 52 unit tests geschreven met 80%+ coverage, alle SOLID principes toegepast, en een SonarCloud Quality Gate A behaald. Mijn code is production-ready binnen de scope van een Application Core."

---

### 🎯 Sterke Afsluitings Statement

> "Wat ik het meest trots op ben, is dat elk design pattern een **echte waarde** toevoegt. Ik had geforceerd 6 patterns in kunnen proppen, maar in plaats daarvan heb ik gezocht naar patterns die **natuurlijk volgen uit de requirements**. Het resultaat is een systeem dat:
> - **Testbaar** is (52 tests, 80%+ coverage)
> - **Uitbreidbaar** is (Open/Closed Principle)
> - **Begrijpbaar** is (goede separation of concerns)
> - **Maintainable** is (SonarCloud A rating)
>
> Dit is geen academische oefening - dit is code die ik trots zou zijn om in productie te hebben."

---

## 8. Quick Reference - Pattern Verdediging

| Pattern | Core Problem | Why This Pattern | Key Benefit |
|---------|--------------|------------------|-------------|
| **State** | 6 states, complex transitions | Encapsulates transition rules per state | Type-safe, Open/Closed |
| **Observer** | Multiple stakeholders need notifications | Loose coupling between subject and observers | New observers without modifying BacklogItem |
| **Composite** | Recursive activity hierarchy | Uniform treatment of leaf/composite | Recursive operations automatic |
| **Command** | Pipeline undo/retry needed | Actions as objects with undo support | Rollback whole pipeline easily |
| **Decorator** | Flexible report composition | Stack decorators dynamically | No class explosion (3 vs 7 classes) |
| **Strategy** | Runtime export format switching | Interchangeable algorithms | Export format changes without SprintReport changes |

---

**Succes met je Assessment! 🚀**

Dit document bevat alles wat je nodig hebt om je design keuzes te verdedigen. Beargumenteer met **confidence** maar blijf **open** voor feedback. Toon dat je **alternatieven hebt overwogen** en **bewuste trade-offs** hebt gemaakt.
