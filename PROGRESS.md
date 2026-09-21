# PROGRESS.md

## Current phase

Phase 1 foundation — architecture skeleton implemented.

## Completed

- Inspected the project-owned folders, supplied BG_LevelManager code, reference assets, packages, and relevant project settings.
- Added the project composition root and the single project-owned Update loop.
- Added the explicit GameLoop and ITickable mechanism.
- Added plain C# GameFlow runtime state for Ready, Playing, Won, and Lost.
- Added ILevelService and isolated BG_LevelManager behind ButchersLevelService.
- Added RunnerConfigSO, CameraConfigSO, and WealthConfigSO classes for authored data only.
- Added the minimal UNITY_EDITOR guard required for player build safety in LevelManager.cs.
- Kept the scene, packages, config asset instances, and existing URP/ShaderGraph changes untouched.

## Verification

- Static source review found one project-owned Update method and no project-owned Start, LateUpdate, or FixedUpdate methods.
- Direct BG_LevelManager access is limited to GameBootstrapper (composition root) and ButchersLevelService (adapter).
- No mutable current-run data is stored in the configuration ScriptableObjects.
- `git diff --check` reports no whitespace errors in the implementation.
- Unity Editor compilation and Console status still require confirmation in the open Editor; project Logs and Library were intentionally not inspected.

## Next

- Confirm a clean Unity compile and Console.
- In the next approved phase, create the input and camera vertical slice.
- Do not start pickups, gates, finish, UI, VFX, or full gameplay before that phase.

## Known vendor risks deferred

- BG_LevelManager progression persistence and level-index handling require validation when level content is connected.
- BG_LevelManager null/empty-list handling, singleton lifecycle, and DestroyImmediate usage remain unchanged.
