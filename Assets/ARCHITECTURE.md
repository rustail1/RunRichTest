# ARCHITECTURE.md

## Core rule

**Data is authored in ScriptableObjects. Runtime state lives in plain C# models. Unity components are adapters/views. Dependencies are assembled only in the Composition Root. Per-frame execution passes through one explicit GameLoop. Cross-module communication uses narrow interfaces or typed events. Vendor code is isolated behind adapters.**

The goal is maintainable test-assignment code, not framework-heavy enterprise architecture.

---

## 1. SOLID interpretation

### Single Responsibility
Each module owns one reason to change.

Examples:
- `RunnerMotor` — runner motion.
- `RunnerInput` — input interpretation.
- `WealthModel` — wealth value and state transitions.
- `PlayerAppearance` — visual representation of wealth state.
- `CameraController` — camera behavior.
- `GameFlow` — Start / Playing / Win / Lose.
- `HUDController` — UI presentation only.

Avoid "god" controllers.

### Open / Closed
Behavior should be extensible mainly through data/configuration where practical.

Example:
- One `Pickup` runtime implementation.
- Different `PickupDefinitionSO` assets define +2, +5, -20, VFX/SFX, etc.

Do not create a new C# class for every money/bottle variant unless behavior is genuinely different.

### Liskov Substitution
Interfaces must be behaviorally substitutable.

Example:
- `IRunnerInput` can be implemented by mouse, touch, recorded test input.

### Interface Segregation
Prefer narrow contracts.

Good examples:
- `IRunnerInput`
- `ILevelService`
- `IWealthReader`
- `IWealthWriter`
- `ITickable`

Avoid large "everything service" interfaces.

### Dependency Inversion
Gameplay code depends on abstractions or injected concrete collaborators, not hidden globals.

Avoid:
- `FindObjectOfType`
- `GameManager.Instance`
- direct use of vendor singleton/static state inside gameplay modules

---

## 2. Runtime composition

### Composition Root
`GameBootstrapper` is the only place allowed to know all concrete implementations.

Responsibilities:
- obtain serialized scene references;
- create plain C# runtime models;
- construct controllers/services;
- wire dependencies;
- register tickables;
- start the game flow.

It must not contain gameplay rules.

### Central game loop
Project-owned per-frame logic flows through one explicit runtime driver.

Conceptual order:

```csharp
void Update()
{
    float dt = Time.deltaTime;

    _input.Tick(dt);
    _gameFlow.Tick(dt);
    _runner.Tick(dt);
    _camera.Tick(dt);
}
```

The exact list can evolve, but order stays explicit.

Do not add arbitrary `Update()` methods across gameplay components.

---

## 3. Layering

### Data layer
ScriptableObjects:
- authored configuration;
- tuning values;
- content definitions;
- typed event channels.

Examples:
- `RunnerConfigSO`
- `CameraConfigSO`
- `WealthConfigSO`
- `PickupDefinitionSO`
- `CharacterStateDefinitionSO`
- typed event-channel assets if needed

### Runtime domain/state
Plain C# objects:
- current wealth;
- current game state;
- run session state;
- decisions/state transitions.

Examples:
- `WealthModel`
- `GameFlow`
- `RunSession`

### Unity adapters/views
MonoBehaviours:
- input adapter;
- transform movement adapter;
- collision/trigger adapters;
- UI views;
- VFX/audio views;
- camera view;
- vendor integration adapter.

MonoBehaviours should be thin.

---

## 4. ScriptableObject policy

Use ScriptableObjects for values a game designer should tune without changing code.

### RunnerConfigSO
Suggested fields:
- Forward Speed
- Swerve Sensitivity
- Swerve Smoothing
- Max Lateral Speed
- Road Half Width
- Start Delay

### CameraConfigSO
Suggested fields:
- FOV
- Offset
- Pitch
- Horizontal Follow
- Follow Smooth Time
- Look Ahead
- Target Screen X
- Target Screen Y

### WealthConfigSO
Suggested fields:
- Initial Wealth
- Min Wealth
- Max Wealth
- Wealth-state thresholds

### PickupDefinitionSO
Suggested fields:
- Wealth Delta
- Optional visual id/reference
- SFX reference
- VFX reference
- popup style

### CharacterStateDefinitionSO
Suggested fields:
- Minimum Wealth
- Display Label
- Character visual/prefab reference
- Optional material/VFX references

### Rule
Never store mutable current-run values inside asset ScriptableObjects.

Bad:
- `CurrentWealth` saved into a ScriptableObject asset.
- `CurrentGameState` saved into a ScriptableObject asset.

Good:
- `InitialWealth` in SO.
- `CurrentWealth` in `WealthModel`.

---

## 5. Input architecture

Contract:

```csharp
public interface IRunnerInput
{
    float HorizontalDelta { get; }
    bool StartPressed { get; }
}
```

Implementation may combine editor mouse and mobile touch while producing the same normalized input.

Gameplay movement must not care whether input came from:
- mouse;
- touch;
- recorded/test data.

Swerve uses relative pointer delta, not absolute screen position.

---

## 6. Camera architecture

Camera tuning belongs to `CameraConfigSO`.

Runtime camera code:
- follows player position according to config;
- keeps projection/pitch behavior centralized;
- contains no game-state/wealth logic.

Camera matching is validated against reference composition.

---

## 7. Wealth architecture

`WealthModel` owns:
- current value;
- clamping;
- wealth-state transition;
- change notification.

It does not:
- modify UI directly;
- swap character models directly;
- play effects directly.

Consumers react to changes.

---

## 8. Pickups

Preferred design:
- `PickupTrigger` MonoBehaviour detects player once.
- it uses a `PickupDefinitionSO`.
- it sends a wealth change request to the runtime model/service.
- then disables/consumes itself.
- feedback is requested through a dedicated feedback boundary/event.

Requirements:
- cannot trigger twice;
- does not know HUD;
- does not know player appearance.

---

## 9. Events

Use direct references when modules are already naturally coupled.

Use typed event channels only for true one-to-many cross-module notifications.

Allowed examples:
- RunStarted
- WealthChanged
- RunWon
- RunLost

Do not introduce a generic string/object global message bus.

---

## 10. Vendor isolation

Employer-provided `BG_LevelManager` is vendor code.

Project code accesses it only through an adapter such as:

```csharp
public interface ILevelService
{
    void Restart();
    void LoadNext();
}
```

Concrete implementation:
`ButchersLevelService`.

Do not spread `LevelManager.Default` calls across gameplay code.

---

## 11. Proposed feature-first layout

```text
Assets/
├── _Game/
│   ├── Runtime/
│   │   ├── Bootstrap/
│   │   ├── GameFlow/
│   │   ├── Runner/
│   │   ├── Camera/
│   │   ├── Wealth/
│   │   ├── Pickups/
│   │   ├── Gates/
│   │   ├── Finish/
│   │   ├── Level/
│   │   ├── UI/
│   │   └── Feedback/
│   │
│   ├── Data/
│   │   ├── Configs/
│   │   ├── Definitions/
│   │   └── Events/
│   │
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Materials/
│   ├── VFX/
│   ├── Audio/
│   └── Tests/
│       ├── EditMode/
│       └── PlayMode/
│
└── ThirdParty/
    ├── ButchersGames/
    └── ReferenceAssets/
```

---

## 12. Explicit non-goals

Do not add:
- Zenject/Extenject
- VContainer
- DOTS/ECS
- Service Locator
- generic Event Bus
- complex command framework
- repository pattern
- save system
- analytics framework
- backend
- shop/meta architecture

unless the test scope later explicitly requires it.
