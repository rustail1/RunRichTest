# DECISIONS.md

Record only decisions that affect architecture or future work.

## D001 — Engine
Decision:
Use Unity 6.3 LTS (6000.3.11f1).

Reason:
Installed locally, LTS, suitable for the assignment.

## D002 — Architecture scope
Decision:
Use lightweight SOLID/modular architecture without DI framework or ECS.

Reason:
Test project is small; reviewability and speed are more important than framework complexity.

## D003 — Runtime state
Decision:
Runtime mutable state lives in plain C# objects, not ScriptableObject assets.

Reason:
Avoid asset mutation/state leakage between runs and keep logic testable.

## D004 — Designer data
Decision:
Designer-tunable values live in ScriptableObject configs/definitions.

Reason:
Allows tuning movement, camera, wealth thresholds and pickup values without code changes.

## D005 — Frame loop
Decision:
Project-owned per-frame execution is centralized through one explicit game loop.

Reason:
Deterministic ordering, easier profiling/debugging, smaller AI context.

## D006 — Vendor code
Decision:
BG_LevelManager is isolated behind an adapter and not used directly throughout gameplay code.

Reason:
Avoid vendor static/singleton coupling.

## D007 — Input
Decision:
Use relative swerve input, not lane switching or absolute screen-position steering.

Reason:
Matches observed reference behavior.

## D008 — Reference truth
Decision:
Unknown original constants remain configurable and labelled as estimates until measured.

Reason:
Avoid pretending inferred values are exact.
