# ROADMAP.md

## Phase 0 — Reference lock
Goal:
- establish what "1:1" means.

Tasks:
- document video resolution/aspect;
- document gameplay timeline;
- record wealth checkpoints;
- record camera composition;
- record gate/finish order;
- mark facts vs estimates;
- later record Android Pointer Location data for input calibration.

Done when:
- `REFERENCE_LEVEL1.md` contains enough observable targets to test against.

---

## Phase 1 — Clean Unity project
Engine:
- Unity 6.3 LTS (6000.3.11f1)

Tasks:
- create clean 3D project;
- initialize Git;
- set Visible Meta Files;
- set Force Text serialization;
- add Unity `.gitignore`;
- commit clean baseline.

Done when:
- clean project opens with zero errors;
- Git working tree is clean.

---

## Phase 2 — Codex integration
Tasks:
- place root docs:
  - AGENTS.md
  - ARCHITECTURE.md
  - ROADMAP.md
  - TASK.md
  - REFERENCE_LEVEL1.md
  - DECISIONS.md
  - PROGRESS.md
  - TEST_PLAN.md
- connect Codex to repository.
- optionally install official Unity/Codex integration if stable in the environment.

Done when:
- Codex can inspect the repo and follows AGENTS.md.

---

## Phase 3 — Import employer/vendor assets
Tasks:
- import BG_LevelManager;
- import supplied reference assets;
- keep vendor/reference content isolated;
- compile;
- document vendor issues instead of spreading workarounds.

Done when:
- all supplied content imports;
- project compiles;
- no pink/broken materials that block gameplay work.

---

## Phase 4 — Architecture skeleton
Create only the minimum:
- GameBootstrapper
- GameLoop/runtime driver
- GameFlow
- ILevelService + vendor adapter
- base config ScriptableObjects
- test assembly structure

Done when:
- architecture compiles;
- no gameplay yet;
- dependencies are wired from composition root.

---

## Phase 5 — Input + camera vertical slice
Implement:
- Start state
- first swipe/drag starts run
- auto-forward
- relative horizontal swerve
- horizontal clamp
- camera follow

Tune against reference.

Done when:
- movement and camera feel close before adding pickups.

---

## Phase 6 — Wealth + pickups + appearance
Implement:
- WealthModel
- wealth state thresholds
- generic Pickup
- money
- alcohol
- HUD wealth display
- player visual-state switching

Done when:
- reference wealth checkpoints can be reproduced.

---

## Phase 7 — Gate + flag sections
Implement:
- Party / School gate
- one-shot gate choice
- School positive delta
- flag-zone triggers if required

Done when:
- gate cannot double-trigger;
- School path matches reference outcome.

---

## Phase 8 — Finish + full game loop
Implement:
- x2 / x3 / x4 / x5 finish presentation
- Win
- Lose
- Restart
- Next Level
- BG_LevelManager integration through adapter

Done when:
- full run is playable from Start to Next Level.

---

## Phase 9 — UI/tutorial
Implement:
- gameplay UI
- swipe tutorial
- win/lose UI
- no shop/meta

Done when:
- first-level UI matches reference sufficiently for the test.

---

## Phase 10 — Animation/VFX/audio
Implement only visible polish:
- pickup feedback
- +/- popups
- appearance-change feedback
- footsteps
- win feedback
- gate/finish polish

Done when:
- visual/audio feedback resembles the supplied recording.

---

## Phase 11 — Optimization/review
Tasks:
- Profiler pass;
- allocations in hot paths;
- duplicate triggers;
- null safety;
- project cleanup;
- code review;
- regression pass.

Done when:
- stable gameplay;
- no obvious performance/code-review issues.

---

## Phase 12 — Submission
Tasks:
- duplicate Level 1 if needed for Next Level demonstration;
- record ~1 minute video;
- upload video;
- push GitHub;
- prepare short submission message.

Done when:
- recruiter can watch gameplay and inspect code without setup confusion.
