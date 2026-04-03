# UML Diagrammen - Avans DevOps
## Software Ontwerp & Architectuur 3

**Project**: Avans DevOps - Scrum/DevOps Project Management System  
**Student**: [Naam]  
**Datum**: 2025  
**Versie**: 1.0

---

## Inhoudsopgave
1. [Class Diagrams](#1-class-diagrams)
2. [State Diagrams](#2-state-diagrams)
3. [Sequence Diagrams](#3-sequence-diagrams)
4. [Design Pattern Diagrams](#4-design-pattern-diagrams)

---

## Hoe PlantUML Diagrammen Gebruiken

### Online Rendering
Bezoek: http://www.plantuml.com/plantuml/uml/
Plak de code en klik op "Submit" voor een PNG afbeelding.

### VS Code Extension
Installeer: "PlantUML" extension door jebbs
Druk op `Alt+D` om preview te zien.

### Lokaal met Java
```bash
java -jar plantuml.jar diagram.puml
```

---

## 1. Class Diagrams

### 1.1 Overall Domain Model

```plantuml
@startuml Overall_Domain_Model
!define TITLE Domain Model - Avans DevOps

title TITLE

' Entities
class Project {
    +Guid Id
    +string Name
    +string Description
    +List<Sprint> Sprints
    +ProductOwner Owner
    --
    +Project(name, description)
}

class Sprint {
    +Guid Id
    +string Name
    +DateTime StartDate
    +DateTime EndDate
    +List<BacklogItem> BacklogItems
    +SprintStatus Status
    +Pipeline? Pipeline
    --
    +Sprint(name, startDate, endDate)
}

enum SprintStatus {
    Planned
    Active
    Completed
}

interface IWorkItem {
    +{abstract} string GetStatus()
    +{abstract} int GetEffortPoints()
}

class BacklogItem {
    +Guid Id
    +string Title
    +string Description
    +Developer? AssignedDeveloper
    +List<Activity> Activities
    +DiscussionThread? Discussion
    +IBacklogItemState State
    +List<IWorkItem> WorkItems
    +int EffortPoints
    --
    +BacklogItem(title, description, effortPoints)
    +void AddWorkItem(IWorkItem)
    +void RemoveWorkItem(IWorkItem)
    +void Subscribe(INotificationSubscriber)
    +void ChangeState(IBacklogItemState)
    +bool CanMarkAsDone()
    +string GetStatus()
    +int GetEffortPoints()
}

class Activity {
    +Guid Id
    +string Name
    +ActivityStatus Status
    +Developer? AssignedDeveloper
    +List<Activity> SubActivities
    +int EffortPoints
    --
    +Activity(name, effortPoints)
    +void Add(Activity)
    +void Remove(Activity)
    +bool CanMarkAsDone()
    +void SetStatus(ActivityStatus)
    +string GetStatus()
    +int GetEffortPoints()
}

enum ActivityStatus {
    Todo
    Doing
    Done
}

class DiscussionThread {
    +Guid Id
    +List<DiscussionComment> Comments
    +bool IsClosed
    --
    +void AddComment(DiscussionComment)
    +void Close()
}

class DiscussionComment {
    +Guid Id
    +User? Author
    +string? Content
    +DateTime CreatedAt
}

' Users
abstract class User {
    +Guid Id
    +string Name
    --
    #User(name)
}

class Developer {
    +Developer(name)
}

class ScrumMaster {
    +ScrumMaster(name)
}

class ProductOwner {
    +ProductOwner(name)
}

class Tester {
    +Tester(name)
}

' Relationships
Project "1" *-- "0..*" Sprint : contains
Sprint "1" *-- "0..*" BacklogItem : contains
BacklogItem "1" *-- "0..*" Activity : contains
BacklogItem "1" *-- "0..1" DiscussionThread : has
DiscussionThread "1" *-- "0..*" DiscussionComment : contains
BacklogItem "0..*" --> "0..1" Developer : assigned to
Activity "0..*" --> "0..1" Developer : assigned to
Project "1" --> "1" ProductOwner : owned by
Sprint "0..*" --> "0..1" ScrumMaster : managed by
DiscussionComment "*" --> "1" User : authored by

IWorkItem <|.. BacklogItem : implements
IWorkItem <|.. Activity : implements
BacklogItem o-- IWorkItem : contains
Activity o-- Activity : contains (composite)

User <|-- Developer
User <|-- ScrumMaster
User <|-- ProductOwner
User <|-- Tester

note right of BacklogItem
  **Composite Pattern**
  BacklogItem can contain Activities
  Activities can contain sub-Activities
  Both implement IWorkItem
end note

note bottom of Sprint
  Sprint Status:
  - Planned: Can modify properties
  - Active: Backlog items in progress
  - Completed: Sprint finished
end note

@enduml
```

---

### 1.2 Pipeline & Actions

```plantuml
@startuml Pipeline_Class_Diagram
!define TITLE Pipeline - Command Pattern

title TITLE

interface IPipelineAction {
    +void Execute()
    +void Undo()
    +bool CanUndo
    +string Name
    +ActionStatus Status
}

enum ActionStatus {
    NotExecuted
    Executing
    Success
    Failed
}

abstract class PipelineActionBase {
    +string Name
    +ActionStatus Status
    +bool CanUndo
    --
    +void Execute()
    #void ExecuteAction()
    +void Undo()
    #void UndoAction()
}

class FetchSourceCodeAction {
    +string Name = "Fetch Source Code"
    +bool CanUndo = true
    --
    #void ExecuteAction()
    #void UndoAction()
}

class InstallPackagesAction {
    +string Name = "Install Packages"
    +bool CanUndo = true
    --
    #void ExecuteAction()
    #void UndoAction()
}

class BuildAction {
    +string Name = "Build"
    +bool CanUndo = true
    --
    #void ExecuteAction()
    #void UndoAction()
}

class TestAction {
    +string Name = "Test"
    +bool CanUndo = false
    --
    #void ExecuteAction()
    #void UndoAction()
}

class AnalyseAction {
    +string Name = "Analyse"
    +bool CanUndo = false
    --
    #void ExecuteAction()
    #void UndoAction()
}

class DeployAction {
    +string Name = "Deploy"
    +bool CanUndo = true
    --
    #void ExecuteAction()
    #void UndoAction()
}

class Pipeline {
    +List<IPipelineAction> Actions
    +PipelineStatus Status
    -Stack<IPipelineAction> _executedActions
    --
    +Pipeline()
    +void AddAction(IPipelineAction)
    +bool Run()
    +bool Retry()
    +void Rollback()
}

enum PipelineStatus {
    NotStarted
    Running
    Success
    Failed
    RolledBack
}

' Relationships
IPipelineAction <|.. PipelineActionBase : implements
PipelineActionBase <|-- FetchSourceCodeAction
PipelineActionBase <|-- InstallPackagesAction
PipelineActionBase <|-- BuildAction
PipelineActionBase <|-- TestAction
PipelineActionBase <|-- AnalyseAction
PipelineActionBase <|-- DeployAction

Pipeline o-- IPipelineAction : contains
Pipeline --> PipelineStatus : has

Sprint "0..1" --> "0..1" Pipeline : has

note right of Pipeline
  **Command Pattern (Invoker)**
  - Executes commands sequentially
  - Maintains execution history
  - Supports retry and rollback
end note

note left of IPipelineAction
  **Command Pattern (Command Interface)**
  - Encapsulates actions
  - Supports undo/redo
  - Some actions cannot be undone
    (Test, Analyse)
end note

note bottom of PipelineActionBase
  **Template Method Pattern**
  Execute() calls ExecuteAction()
  Undo() calls UndoAction()
  Subclasses implement specific logic
end note

@enduml
```

---

### 1.3 Sprint Report & Export

```plantuml
@startuml SprintReport_Class_Diagram
!define TITLE Sprint Report - Decorator & Strategy Patterns

title TITLE

class SprintReport {
    +Sprint? Sprint
    +IReport? Report
    -IReportExportStrategy? _exportStrategy
    --
    +string Generate()
    +void SetExportStrategy(IReportExportStrategy)
    +string Export()
}

interface IReport {
    +string Generate()
}

class BasicSprintReport {
    +string Generate()
}

abstract class ReportDecorator {
    #IReport _component
    --
    #ReportDecorator(IReport)
    +{abstract} string Generate()
}

class HeaderDecorator {
    +HeaderDecorator(IReport)
    +string Generate()
}

class FooterDecorator {
    +FooterDecorator(IReport)
    +string Generate()
}

class ChartDecorator {
    +ChartDecorator(IReport)
    +string Generate()
}

interface IReportExportStrategy {
    +string Export(string content)
    +string FileExtension
}

class PdfExportStrategy {
    +string FileExtension = ".pdf"
    --
    +string Export(string content)
}

class PngExportStrategy {
    +string FileExtension = ".png"
    --
    +string Export(string content)
}

class CsvExportStrategy {
    +string FileExtension = ".csv"
    --
    +string Export(string content)
}

' Relationships
SprintReport --> IReport : generates
SprintReport --> IReportExportStrategy : uses

IReport <|.. BasicSprintReport : implements
IReport <|.. ReportDecorator : implements
ReportDecorator <|-- HeaderDecorator
ReportDecorator <|-- FooterDecorator
ReportDecorator <|-- ChartDecorator
ReportDecorator o-- IReport : wraps

IReportExportStrategy <|.. PdfExportStrategy : implements
IReportExportStrategy <|.. PngExportStrategy : implements
IReportExportStrategy <|.. CsvExportStrategy : implements

Sprint "1" --> "0..*" SprintReport : generates

note right of ReportDecorator
  **Decorator Pattern**
  - Adds functionality dynamically
  - Can be stacked:
    HeaderDecorator(
      FooterDecorator(
        BasicReport))
end note

note bottom of IReportExportStrategy
  **Strategy Pattern**
  - Interchangeable export algorithms
  - Can switch at runtime
  - Open/Closed Principle
end note

note left of SprintReport
  **Context for Strategy**
  Uses composition to delegate
  export behavior to strategy
end note

@enduml
```

---

## 2. State Diagrams

### 2.1 BacklogItem State Machine

```plantuml
@startuml BacklogItem_State_Diagram
!define TITLE BacklogItem State Transitions

title TITLE

[*] --> Todo : create BacklogItem

state Todo {
    Todo : Entry: Initial state
    Todo : Activities can be added
}

state Doing {
    Doing : Entry: Developer starts work
    Doing : Can go back to Todo
}

state ReadyForTesting {
    ReadyForTesting : Entry: Developer finished
    ReadyForTesting : Exit: Notify Testers
    ReadyForTesting : Can go back to Doing
}

state Testing {
    Testing : Entry: Tester starts testing
    Testing : Can go back to ReadyForTesting
}

state Tested {
    Tested : Entry: Testing successful
    Tested : Can go back to Testing if issues found
}

state Done {
    Done : Entry: Fully completed
    Done : Exit: Lock Discussion Thread
    Done : Validation: All Activities must be Done
    Done : Final state
}

Todo --> Doing : ChangeState(DoingState)
Doing --> ReadyForTesting : ChangeState(ReadyForTestingState)\n[notify testers]
Doing --> Todo : ChangeState(TodoState)\n[issue found]

ReadyForTesting --> Testing : ChangeState(TestingState)
ReadyForTesting --> Doing : ChangeState(DoingState)\n[not ready]

Testing --> Tested : ChangeState(TestedState)\n[tests passed]
Testing --> ReadyForTesting : ChangeState(ReadyForTestingState)\n[tests failed, retry]

Tested --> Done : ChangeState(DoneState)\n[validate: all activities done]\n[notify lead developer]
Tested --> Testing : ChangeState(TestingState)\n[DoD not met]

Done --> [*]

note right of Done
  **Business Rule BR-01:**
  BacklogItem can only transition
  to Done if ALL Activities are Done.
  
  Enforced by: CanMarkAsDone()
end note

note bottom of ReadyForTesting
  **Observer Pattern Trigger:**
  When entering ReadyForTesting,
  all subscribed Testers receive
  notification via Email/Slack
end note

note left of Doing
  **Invalid Transitions:**
  Todo -> Tested ❌
  Todo -> Done ❌
  Testing -> Doing ❌
  (must go via ReadyForTesting)
end note

@enduml
```

---

### 2.2 Sprint Lifecycle State Diagram

```plantuml
@startuml Sprint_Lifecycle_State_Diagram
!define TITLE Sprint Lifecycle

title TITLE

[*] --> Planned

state Planned {
    Planned : Sprint properties editable
    Planned : Can add/remove BacklogItems
    Planned : Can set Pipeline
}

state Active {
    Active : Sprint execution started
    Active : BacklogItems in progress
    Active : Properties locked
}

state Finished {
    Finished : End date reached
    Finished : All work should be done
}

state "Review\nPending" as ReviewPending {
    ReviewPending : Awaiting sprint review
    ReviewPending : Review document required
}

state "Release\nPending" as ReleasePending {
    ReleasePending : Ready for release
    ReleasePending : Can trigger pipeline
}

state "Pipeline\nRunning" as PipelineRunning {
    PipelineRunning : Development pipeline executing
    PipelineRunning : Cannot modify sprint
}

state Closed {
    Closed : Sprint successfully completed
    Closed : Final state
}

state Cancelled {
    Cancelled : Sprint cancelled/failed
    Cancelled : Final state
}

Planned --> Active : Start sprint
Active --> Finished : End date reached

Finished --> ReviewPending : [Sprint type = Review]
Finished --> ReleasePending : [Sprint type = Release]

ReviewPending --> Closed : Upload review document\n& close sprint
ReviewPending --> Cancelled : Cancel sprint

ReleasePending --> PipelineRunning : Trigger release\n[start pipeline]
ReleasePending --> Cancelled : Cancel release

PipelineRunning --> Closed : [Pipeline success]\n[notify PO & SM]
PipelineRunning --> ReleasePending : [Pipeline failed]\n[can retry]\n[notify SM]
PipelineRunning --> Cancelled : Cancel release

Closed --> [*]
Cancelled --> [*]

note right of PipelineRunning
  **Command Pattern in Action:**
  Pipeline executes actions sequentially.
  On failure:
  - Retry() to run again
  - Rollback() to undo actions
end note

note bottom of ReleasePending
  **Business Rule:**
  Release can only be triggered
  if Sprint has a configured Pipeline
end note

note left of ReviewPending
  **Business Rule:**
  Sprint can only close if
  review document is uploaded
end note

@enduml
```

---

### 2.3 Activity Status State Diagram

```plantuml
@startuml Activity_Status_State_Diagram
!define TITLE Activity Status Transitions

title TITLE

[*] --> Todo : create Activity

state Todo {
    Todo : Initial state
    Todo : Not started yet
}

state Doing {
    Doing : Developer working on it
}

state Done {
    Done : Activity completed
    Done : Validation: All sub-activities Done
}

Todo --> Doing : SetStatus(Doing)
Doing --> Done : SetStatus(Done)\n[validate: all sub-activities done]
Doing --> Todo : SetStatus(Todo)\n[restart work]

Done --> [*]

note right of Done
  **Business Rule BR-04:**
  Activity can only be Done
  if all SubActivities are Done.
  
  Enforced by: CanMarkAsDone()
  Checked in: SetStatus()
end note

note bottom of Doing
  **Composite Pattern:**
  Activity.GetEffortPoints()
  recursively sums effort of
  all sub-activities
end note

@enduml
```

---

## 3. Sequence Diagrams

### 3.1 BacklogItem State Change with Notification

```plantuml
@startuml BacklogItem_StateChange_Sequence
!define TITLE BacklogItem State Change with Observer Pattern

title TITLE

actor Developer
participant "backlogItem:\nBacklogItem" as BI
participant "state:\nDoingState" as Doing
participant "newState:\nReadyForTestingState" as Ready
participant "notifier:\nNotifier" as Notifier
participant "testerSub:\nNotificationChannelSubscriber" as Sub
participant "emailChannel:\nEmailChannel" as Email
participant "slackChannel:\nSlackChannel" as Slack

Developer -> BI : ChangeState(ReadyForTestingState)
activate BI

BI -> Doing : CanTransitionTo(ReadyForTestingState)
activate Doing
Doing --> BI : true
deactivate Doing

BI -> BI : state = ReadyForTestingState
note right: State Pattern\nState object changed

BI -> Notifier : NotifyAll(this, "Status changed from Doing to ReadyForTesting")
activate Notifier

loop for each subscriber
    Notifier -> Sub : Notify(backlogItem, message)
    activate Sub
    
    Sub -> Email : Send("BacklogItem 'Feature X': Status changed...")
    activate Email
    Email --> Sub : [Email sent]
    deactivate Email
    
    Sub -> Slack : Send("BacklogItem 'Feature X': Status changed...")
    activate Slack
    Slack --> Sub : [Slack message sent]
    deactivate Slack
    
    deactivate Sub
end

Notifier --> BI : [all notified]
deactivate Notifier

BI --> Developer : [state changed]
deactivate BI

note over BI, Notifier
  **Observer Pattern:**
  - BacklogItem is Subject
  - NotificationChannelSubscriber is Observer
  - Loose coupling via interface
  - Multiple channels can be notified
end note

note over Doing, Ready
  **State Pattern:**
  - Each state validates allowed transitions
  - Encapsulates state-specific behavior
  - Open/Closed Principle
end note

@enduml
```

---

### 3.2 Pipeline Execution with Rollback

```plantuml
@startuml Pipeline_Execution_Sequence
!define TITLE Pipeline Execution with Command Pattern

title TITLE

actor "DevOps\nEngineer" as Engineer
participant "pipeline:\nPipeline" as Pipeline
participant "fetchAction:\nFetchSourceCodeAction" as Fetch
participant "buildAction:\nBuildAction" as Build
participant "testAction:\nTestAction" as Test

== Pipeline Execution ==

Engineer -> Pipeline : Run()
activate Pipeline

Pipeline -> Pipeline : status = Running

Pipeline -> Fetch : Execute()
activate Fetch
Fetch -> Fetch : ExecuteAction()
note right: Fetch source code\nfrom repository
Fetch -> Fetch : status = Success
Fetch --> Pipeline : [executed]
deactivate Fetch

Pipeline -> Pipeline : _executedActions.Push(fetchAction)

Pipeline -> Build : Execute()
activate Build
Build -> Build : ExecuteAction()
note right: Build project
Build -> Build : status = Success
Build --> Pipeline : [executed]
deactivate Build

Pipeline -> Pipeline : _executedActions.Push(buildAction)

Pipeline -> Test : Execute()
activate Test
Test -> Test : ExecuteAction()
note right: Run unit tests
Test -> Test : status = Failed
Test --> Pipeline : [exception thrown]
deactivate Test

Pipeline -> Pipeline : status = Failed
Pipeline --> Engineer : false (failure)
deactivate Pipeline

== Rollback Scenario ==

Engineer -> Pipeline : Rollback()
activate Pipeline

loop while _executedActions not empty
    Pipeline -> Pipeline : action = _executedActions.Pop()
    
    alt action.CanUndo == true
        Pipeline -> Build : Undo()
        activate Build
        Build -> Build : UndoAction()
        note right: Clean build output
        Build -> Build : status = NotExecuted
        Build --> Pipeline : [undone]
        deactivate Build
        
        Pipeline -> Fetch : Undo()
        activate Fetch
        Fetch -> Fetch : UndoAction()
        note right: Clean up\nfetched source
        Fetch -> Fetch : status = NotExecuted
        Fetch --> Pipeline : [undone]
        deactivate Fetch
    else action.CanUndo == false
        note over Pipeline, Test: TestAction cannot be undone\n(skipped)
    end
end

Pipeline -> Pipeline : status = RolledBack
Pipeline --> Engineer : [rolled back]
deactivate Pipeline

note over Pipeline
  **Command Pattern Benefits:**
  - Actions are encapsulated
  - Execution history maintained
  - Undo/Redo support
  - Can replay commands (Retry)
end note

note over Fetch, Build
  **Undoable Commands:**
  - FetchSourceCodeAction ✓
  - BuildAction ✓
  - DeployAction ✓
  
  **Non-Undoable:**
  - TestAction ✗
  - AnalyseAction ✗
end note

@enduml
```

---

### 3.3 Report Generation with Decorator and Strategy

```plantuml
@startuml Report_Generation_Sequence
!define TITLE Report Generation - Decorator & Strategy Patterns

title TITLE

actor "Scrum\nMaster" as SM
participant "sprintReport:\nSprintReport" as SR
participant "decorated:\nHeaderDecorator" as Header
participant "footer:\nFooterDecorator" as Footer
participant "basic:\nBasicSprintReport" as Basic
participant "strategy:\nPdfExportStrategy" as PDF

== Report Generation (Decorator Pattern) ==

SM -> SR : Generate()
activate SR

SR -> Header : Generate()
activate Header
note right: Decorator adds header

Header -> Footer : _component.Generate()
activate Footer
note right: Decorator adds footer

Footer -> Basic : _component.Generate()
activate Basic
Basic --> Footer : "Sprint data"
deactivate Basic

Footer --> Header : "Sprint data\n[Footer]"
deactivate Footer

Header --> SR : "[Bedrijfslogo + Header]\nSprint data\n[Footer]"
deactivate Header

SR --> SM : [full report content]
deactivate SR

== Export (Strategy Pattern) ==

SM -> SR : SetExportStrategy(PdfExportStrategy)
activate SR
SR -> SR : _exportStrategy = strategy
SR --> SM : [strategy set]
deactivate SR

SM -> SR : Export()
activate SR

SR -> SR : content = Generate()
note right: Gets decorated content

SR -> PDF : Export(content)
activate PDF
PDF -> PDF : // Convert to PDF format
PDF --> SR : "[PDF Export]\n[Header]\nSprint data\n[Footer]\n[End PDF]"
deactivate PDF

SR --> SM : [exported PDF content]
deactivate SR

== Switch Strategy ==

SM -> SR : SetExportStrategy(PngExportStrategy)
activate SR
SR -> SR : _exportStrategy = new strategy
SR --> SM : [strategy switched]
deactivate SR

SM -> SR : Export()
activate SR
SR -> SR : content = Generate()
note right: Same content,\ndifferent export
SR --> SM : [exported PNG content]
deactivate SR

note over Header, Footer
  **Decorator Pattern:**
  - Decorators wrap components
  - Can be stacked infinitely
  - Adds behavior dynamically
  - Open/Closed Principle
end note

note over SR, PDF
  **Strategy Pattern:**
  - Export algorithm is interchangeable
  - Can switch at runtime
  - Context (SprintReport) delegates
    to strategy
  - Dependency Inversion Principle
end note

@enduml
```

---

### 3.4 Composite Pattern - Recursive Effort Points Calculation

```plantuml
@startuml Composite_EffortPoints_Sequence
!define TITLE Composite Pattern - Effort Points Calculation

title TITLE

actor Developer
participant "backlogItem:\nBacklogItem" as BI
participant "activity1:\nActivity" as A1
participant "activity2:\nActivity" as A2
participant "subActivity:\nActivity" as SubA

== Building Composite Structure ==

Developer -> BI : <<create>>(title, desc, effortPoints=10)
Developer -> A1 : <<create>>(name="Frontend", effortPoints=5)
Developer -> A2 : <<create>>(name="Backend", effortPoints=8)
Developer -> SubA : <<create>>(name="Database", effortPoints=3)

Developer -> A2 : Add(subActivity)
activate A2
A2 -> A2 : SubActivities.Add(subActivity)
deactivate A2

Developer -> BI : AddWorkItem(activity1)
activate BI
BI -> BI : WorkItems.Add(activity1)
BI -> BI : Activities.Add(activity1)
deactivate BI

Developer -> BI : AddWorkItem(activity2)
activate BI
BI -> BI : WorkItems.Add(activity2)
BI -> BI : Activities.Add(activity2)
deactivate BI

== Recursive Calculation ==

Developer -> BI : GetEffortPoints()
activate BI
BI -> BI : total = 10 (own effort)

loop for each workItem in WorkItems
    BI -> A1 : GetEffortPoints()
    activate A1
    A1 -> A1 : total = 5 (own effort)
    A1 -> A1 : SubActivities is empty
    A1 --> BI : 5
    deactivate A1
    
    BI -> BI : total += 5 (now 15)
    
    BI -> A2 : GetEffortPoints()
    activate A2
    A2 -> A2 : total = 8 (own effort)
    
    loop for each subActivity
        A2 -> SubA : GetEffortPoints()
        activate SubA
        SubA -> SubA : total = 3 (own effort)
        SubA -> SubA : SubActivities is empty
        SubA --> A2 : 3
        deactivate SubA
        
        A2 -> A2 : total += 3 (now 11)
    end
    
    A2 --> BI : 11
    deactivate A2
    
    BI -> BI : total += 11 (now 26)
end

BI --> Developer : 26
deactivate BI

note right of BI
  **Composite Pattern:**
  BacklogItem (10)
  ├─ Activity1 (5)
  └─ Activity2 (8)
      └─ SubActivity (3)
  
  Total: 10+5+8+3 = 26
end note

note over A1, A2
  **Recursive Traversal:**
  GetEffortPoints() calls itself
  on all children (depth-first).
  
  Leaf nodes (no children) return
  their own effort points.
  
  Composite nodes sum:
  own + all children (recursive)
end note

note over BI
  **Uniform Interface (IWorkItem):**
  Both BacklogItem and Activity
  implement same interface.
  
  Client treats leaf and composite
  objects uniformly.
end note

@enduml
```

---

## 4. Design Pattern Diagrams

### 4.1 State Pattern - Detailed

```plantuml
@startuml State_Pattern_Detailed
!define TITLE State Pattern - BacklogItem States

title TITLE

interface IBacklogItemState {
    +string Name
    +bool CanTransitionTo(IBacklogItemState newState)
}

class BacklogItem {
    -IBacklogItemState State
    --
    +void ChangeState(IBacklogItemState newState)
    +string GetStatus()
}

class TodoState {
    +string Name = "Todo"
    --
    +bool CanTransitionTo(IBacklogItemState newState)
}

class DoingState {
    +string Name = "Doing"
    --
    +bool CanTransitionTo(IBacklogItemState newState)
}

class ReadyForTestingState {
    +string Name = "ReadyForTesting"
    --
    +bool CanTransitionTo(IBacklogItemState newState)
}

class TestingState {
    +string Name = "Testing"
    --
    +bool CanTransitionTo(IBacklogItemState newState)
}

class TestedState {
    +string Name = "Tested"
    --
    +bool CanTransitionTo(IBacklogItemState newState)
}

class DoneState {
    +string Name = "Done"
    --
    +bool CanTransitionTo(IBacklogItemState newState)
}

BacklogItem o--> IBacklogItemState : has current state

IBacklogItemState <|.. TodoState
IBacklogItemState <|.. DoingState
IBacklogItemState <|.. ReadyForTestingState
IBacklogItemState <|.. TestingState
IBacklogItemState <|.. TestedState
IBacklogItemState <|.. DoneState

TodoState ..> DoingState : allows transition to
DoingState ..> ReadyForTestingState : allows transition to
DoingState ..> TodoState : allows transition to
ReadyForTestingState ..> TestingState : allows transition to
ReadyForTestingState ..> DoingState : allows transition to
TestingState ..> TestedState : allows transition to
TestingState ..> ReadyForTestingState : allows transition to
TestedState ..> DoneState : allows transition to
TestedState ..> TestingState : allows transition to

note right of BacklogItem
  **Context**
  Delegates state-specific
  behavior to current State object
end note

note bottom of IBacklogItemState
  **State Interface**
  Each concrete state determines
  valid transitions
end note

note left of TodoState
  **OO Principles Applied:**
  
  1. Open/Closed Principle:
     New states can be added
     without modifying BacklogItem
  
  2. Single Responsibility:
     Each state class has one job
  
  3. Encapsulation:
     State transition logic is
     inside each state
end note

@enduml
```

---

### 4.2 Observer Pattern - Detailed

```plantuml
@startuml Observer_Pattern_Detailed
!define TITLE Observer Pattern - Notification System

title TITLE

interface INotificationSubscriber {
    +void Notify(BacklogItem item, string message)
}

class BacklogItem {
    -Notifier _notifier
    --
    +void Subscribe(INotificationSubscriber)
    +void Unsubscribe(INotificationSubscriber)
    +void ChangeState(IBacklogItemState)
}

class Notifier {
    -List<INotificationSubscriber> _subscribers
    --
    +void Subscribe(INotificationSubscriber)
    +void Unsubscribe(INotificationSubscriber)
    +void NotifyAll(BacklogItem item, string message)
}

class NotificationChannelSubscriber {
    -NotificationChannel _channel
    --
    +NotificationChannelSubscriber(NotificationChannel)
    +void Notify(BacklogItem item, string message)
}

abstract class NotificationChannel {
    +{abstract} void Send(string message)
}

class EmailChannel {
    +void Send(string message)
}

class SlackChannel {
    +void Send(string message)
}

class SmsChannel {
    +void Send(string message)
}

' Factory Method Pattern (bonus)
class NotificationChannelFactory <<static>> {
    +{static} NotificationChannel CreateChannel(string type)
}

BacklogItem *--> Notifier : maintains
Notifier o--> INotificationSubscriber : notifies
INotificationSubscriber <|.. NotificationChannelSubscriber
NotificationChannelSubscriber --> NotificationChannel : uses
NotificationChannel <|-- EmailChannel
NotificationChannel <|-- SlackChannel
NotificationChannel <|-- SmsChannel
NotificationChannelFactory ..> NotificationChannel : creates

note right of BacklogItem
  **Subject (Observable)**
  - Maintains list of observers
  - Notifies on state changes
  - Delegates to Notifier helper
end note

note bottom of INotificationSubscriber
  **Observer Interface**
  - Defines update method
  - Loose coupling with subject
end note

note left of NotificationChannelFactory
  **Factory Method Pattern**
  (Bonus Creational Pattern)
  Creates appropriate channel
  based on string type
end note

note bottom of NotificationChannel
  **Strategy Pattern Integration**
  NotificationChannel acts as
  strategy for sending notifications
  via different media
end note

note right of Notifier
  **OO Principles Applied:**
  
  1. Dependency Inversion:
     BacklogItem depends on
     interface, not concrete classes
  
  2. Open/Closed:
     New observers can be added
     without modifying BacklogItem
  
  3. Single Responsibility:
     Notifier handles notification
     logic separately
end note

@enduml
```

---

### 4.3 Composite Pattern - Detailed

```plantuml
@startuml Composite_Pattern_Detailed
!define TITLE Composite Pattern - Work Item Hierarchy

title TITLE

interface IWorkItem {
    +string GetStatus()
    +int GetEffortPoints()
}

class BacklogItem {
    +int EffortPoints
    +List<IWorkItem> WorkItems
    +List<Activity> Activities
    --
    +void AddWorkItem(IWorkItem)
    +void RemoveWorkItem(IWorkItem)
    +string GetStatus()
    +int GetEffortPoints()
    +bool CanMarkAsDone()
}

class Activity {
    +int EffortPoints
    +List<Activity> SubActivities
    --
    +void Add(Activity)
    +void Remove(Activity)
    +string GetStatus()
    +int GetEffortPoints()
    +bool CanMarkAsDone()
}

IWorkItem <|.. BacklogItem : implements
IWorkItem <|.. Activity : implements

BacklogItem "1" o-- "0..*" IWorkItem : contains children
Activity "1" o-- "0..*" Activity : contains sub-activities

note right of IWorkItem
  **Component**
  Common interface for
  both leaf and composite objects
end note

note bottom of BacklogItem
  **Composite**
  - Can contain IWorkItems
  - Implements operations recursively
  - GetEffortPoints() sums all children
  - CanMarkAsDone() checks all children
end note

note left of Activity
  **Composite & Leaf**
  - Can be leaf (no SubActivities)
  - Can be composite (has SubActivities)
  - Implements same interface as parent
end note

note as N1
  **Tree Structure Example:**
  
  BacklogItem "User Authentication" (10)
    ├─ Activity "Frontend" (5)
    │   ├─ Activity "Login Form" (2)
    │   └─ Activity "Error Handling" (3)
    └─ Activity "Backend API" (8)
        ├─ Activity "JWT Service" (4)
        └─ Activity "User Repository" (4)
  
  Total Effort: 10+5+2+3+8+4+4 = 36
end note

note bottom of N1
  **OO Principles Applied:**
  
  1. Liskov Substitution Principle:
     BacklogItem and Activity are
     substitutable via IWorkItem
  
  2. Single Responsibility:
     Each class manages only its
     own children
  
  3. Open/Closed:
     New IWorkItem types can be added
     without modifying existing code
  
  4. Composite Reusability:
     Same structure can represent
     various hierarchies
end note

@enduml
```

---

### 4.4 All Patterns Integration Overview

```plantuml
@startuml All_Patterns_Integration
!define TITLE Design Patterns Integration Overview

title TITLE

package "Domain Model" {
    class BacklogItem {
        Uses: State, Observer, Composite
    }
    
    class Activity {
        Uses: Composite
    }
    
    class Pipeline {
        Uses: Command
    }
    
    class SprintReport {
        Uses: Decorator, Strategy
    }
}

package "State Pattern" {
    interface IBacklogItemState
    class TodoState
    class DoingState
    class DoneState
}

package "Observer Pattern" {
    interface INotificationSubscriber
    class Notifier
    class NotificationChannelSubscriber
}

package "Composite Pattern" {
    interface IWorkItem
}

package "Command Pattern" {
    interface IPipelineAction
    class PipelineActionBase
    class BuildAction
    class DeployAction
}

package "Decorator Pattern" {
    interface IReport
    class ReportDecorator
    class HeaderDecorator
}

package "Strategy Pattern" {
    interface IReportExportStrategy
    class PdfExportStrategy
    class PngExportStrategy
}

BacklogItem --> IBacklogItemState : uses
BacklogItem --> Notifier : uses
BacklogItem ..|> IWorkItem : implements
Activity ..|> IWorkItem : implements

Pipeline --> IPipelineAction : uses
PipelineActionBase ..|> IPipelineAction : implements
BuildAction --|> PipelineActionBase : extends

SprintReport --> IReport : uses
SprintReport --> IReportExportStrategy : uses
ReportDecorator ..|> IReport : implements
HeaderDecorator --|> ReportDecorator : extends
PdfExportStrategy ..|> IReportExportStrategy : implements

note right of BacklogItem
  **Integration Point:**
  BacklogItem uses 3 patterns:
  - State: for status transitions
  - Observer: for notifications
  - Composite: for activities
end note

note bottom of SprintReport
  **Integration Point:**
  SprintReport combines 2 patterns:
  - Decorator: for content
  - Strategy: for export format
end note

note left of Pipeline
  **Command Pattern:**
  Pipeline acts as Invoker
  for executing, undoing, and
  retrying pipeline actions
end note

@enduml
```

---

## 5. UML Diagram Export Instructions

### 5.1 Voor PDF Inlevering

**Stap 1: Render alle diagrammen**
1. Kopieer elke PlantUML code block
2. Ga naar: http://www.plantuml.com/plantuml/uml/
3. Plak code en klik "Submit"
4. Klik rechts op afbeelding → "Save image as..."
5. Sla op als PNG met beschrijvende naam

**Stap 2: Voeg toe aan document**
- Voeg alle PNG bestanden toe aan je PDF document
- Organiseer per sectie (Class, State, Sequence)
- Voeg toelichting toe onder elk diagram

### 5.2 Voor Live Editing in VS Code

**Install PlantUML Extension:**
```
1. Open VS Code
2. Ga naar Extensions (Ctrl+Shift+X)
3. Zoek "PlantUML" door jebbs
4. Installeer de extensie
5. Install Java JRE (required)
```

**Preview Diagram:**
```
1. Open dit .md bestand in VS Code
2. Plaats cursor in PlantUML code block
3. Druk Alt+D voor preview
4. Of: Ctrl+Shift+P → "PlantUML: Preview Current Diagram"
```

**Export from VS Code:**
```
Ctrl+Shift+P → "PlantUML: Export Current Diagram"
Kies formaat: PNG, SVG, PDF
```

---

## 6. Design Rationale

### 6.1 Waarom deze UML Diagrammen?

| Diagram Type | Rationale |
|--------------|-----------|
| **Overall Domain Model** | Toont complete structuur en relaties tussen entities |
| **Pipeline Class Diagram** | Illustreert Command Pattern implementatie in detail |
| **Report Class Diagram** | Toont interactie tussen Decorator en Strategy patterns |
| **BacklogItem State Diagram** | Visualiseert complexe state machine met business rules |
| **Sprint Lifecycle** | Toont sprint flow inclusief pipeline execution |
| **State Change Sequence** | Demonstreert Observer Pattern in actie |
| **Pipeline Execution Sequence** | Toont Command Pattern met undo/retry |
| **Report Generation Sequence** | Toont samenwerking Decorator + Strategy |
| **Composite Sequence** | Illustreert recursieve operaties in boom-structuur |

### 6.2 OO Principes per Pattern

**State Pattern:**
- ✅ Open/Closed: Nieuwe states zonder bestaande code te wijzigen
- ✅ Single Responsibility: Elke state één verantwoordelijkheid
- ✅ Liskov Substitution: Alle states uitwisselbaar

**Observer Pattern:**
- ✅ Dependency Inversion: Afhankelijk van abstractie
- ✅ Open/Closed: Nieuwe observers toevoegen zonder wijzigingen
- ✅ Single Responsibility: Notifier gescheiden van domain logic

**Command Pattern:**
- ✅ Single Responsibility: Elke command één actie
- ✅ Open/Closed: Nieuwe commands toevoegen
- ✅ Command Query Separation: Execute vs query methods

**Strategy Pattern:**
- ✅ Open/Closed: Nieuwe strategies toevoegen
- ✅ Dependency Inversion: Context afhankelijk van interface
- ✅ Liskov Substitution: Alle strategies uitwisselbaar

**Composite Pattern:**
- ✅ Liskov Substitution: Uniform treatment leaf/composite
- ✅ Single Responsibility: Elk niveau eigen kinderen
- ✅ Open/Closed: Nieuwe IWorkItem types toevoegen

**Decorator Pattern:**
- ✅ Open/Closed: Nieuwe decorators toevoegen
- ✅ Single Responsibility: Elke decorator één decoratie
- ✅ Liskov Substitution: Decorators uitwisselbaar

---

**Document Versie**: 1.0  
**Laatste Update**: 2025  
**Status**: Ready for Export to PDF
