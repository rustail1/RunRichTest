# AGENTS.md

## Project
Run Rich 3D — Level 1 replica for a Unity test assignment.

## Engine
Unity 6.3 LTS — 6000.3.11f1.

## Read order
Before changing gameplay code:
1. Read `TASK.md`.
2. Read the relevant section of `REFERENCE_LEVEL1.md`.
3. Read `ARCHITECTURE.md`.
4. Read `DECISIONS.md`.
5. Check `PROGRESS.md`.

Before changing architecture:
- Read all of `ARCHITECTURE.md`.
- Do not silently violate it.
- If a requirement conflicts with the architecture, explain the conflict first and propose the smallest exception.

## Context policy
Do not scan the whole Unity project.

Never inspect unless explicitly required:
- Library/
- Temp/
- Logs/
- obj/
- Builds/
- UserSettings/

Do not inspect binary assets unless directly relevant:
- .fbx
- .png
- .jpg
- .jpeg
- .wav
- .mp3
- .ogg
- .mp4
- .dll

Prefer targeted searches and small file reads.
Do not dump large logs; extract only relevant errors and stack traces.
Do not reread unchanged files unnecessarily.

## Engineering rules
- Follow `ARCHITECTURE.md`.
- Prefer the smallest working implementation.
- SOLID means clear responsibilities, not maximum abstraction.
- No DI framework, ECS/DOTS, Service Locator, global Event Bus or new third-party package without explicit approval.
- Do not create a new manager if an existing module can own the responsibility.
- Runtime state must not live in ScriptableObject assets.
- ScriptableObjects are for authored configuration/definitions/events, not current run state.
- Dependencies are assembled in the composition root.
- Vendor code is isolated behind adapters.
- Avoid `FindObjectOfType`, hidden singletons and direct vendor static access in gameplay code.
- Per-frame execution goes through the explicit game loop.
- UI does not own gameplay state.
- Pickups do not directly control UI or player visuals.
- Do not edit Unity scene/prefab YAML manually when Editor setup is safer.

## Unity callback policy
For project-owned runtime code:
- `Update()` is allowed only in the central runtime driver / bootstrap layer.
- `Start()` should be avoided outside bootstrap unless a concrete Unity lifecycle reason requires it.
- Event callbacks such as `OnTriggerEnter`, `OnEnable`, `OnDisable` are allowed when they represent real Unity events.
- Do not poll in Update when an event can express the same behavior.

## Feature workflow
For each task:
1. State the expected observable result.
2. Inspect existing implementation.
3. Make the smallest change.
4. Compile.
5. Check Console.
6. Run the relevant EditMode/PlayMode tests.
7. Verify manually in Play Mode.
8. Update `PROGRESS.md`.
9. Record only important architecture decisions in `DECISIONS.md`.

## Priority
1. Controls matching reference.
2. Camera matching reference.
3. Complete game loop.
4. Wealth / pickups / appearance.
5. Gate.
6. Finish.
7. UI/tutorial.
8. Animation/VFX/audio.
9. Profiling/cleanup.

## Definition of done
A task is not done until:
- project compiles;
- no new Console errors;
- happy path works;
- obvious duplicate-trigger/null-reference edge case is checked;
- relevant tests pass;
- manual Play Mode behavior is verified.
