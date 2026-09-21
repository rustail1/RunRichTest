# REFERENCE_LEVEL1.md

## Source
Supplied `IMG_3474.MP4`.

## Facts observed
- Portrait runner.
- Character auto-runs forward.
- Player steers left/right by drag/swipe.
- Not lane-based.
- Start includes swipe tutorial.
- Positive money pickups increase wealth.
- Red alcohol/bottle pickups reduce wealth.
- Player appearance changes with wealth.
- Mid-level two-choice gate includes Party / School.
- Supplied run chooses School.
- End includes x2 / x3 / x4 / x5 staged finish.
- Win UI appears after finish.

## Video
- Approx. 880x1920 capture.
- Approx. 32.65 s total.
- Gameplay roughly ~1 s to ~27 s.

## Visible wealth checkpoints
Useful calibration points:
- Start: 40
- ~2.0 s: 42
- ~2.5 s: 47
- ~3.0 s: 52
- ~3.5 s: 54
- ~4.0 s: 58
- ~4.5 s: 60
- bottle hit: visible -20 -> 40
- later: 41
- 50 before gate
- School gate: visible +20 -> 70
- later: 79
- 82
- 90
- 96
- 102
- around 106–110 near finish

## Inferred wealth states
These are inferred and must stay configurable:
- Poor: < 70
- Well-off: 70–99
- Rich: >= 100

Observed transitions:
- around 70: outfit changes, label becomes wealthier state.
- around 102: richer appearance, label becomes rich.

## Camera composition — observed
- player stays close to horizontal center;
- player center roughly around mid-height;
- camera follows X strongly;
- camera follows forward progression;
- roll ~0;
- yaw ~0 relative to track;
- no obvious gameplay zoom animation.

## Camera initial calibration — estimate
Not proven original values:
- Perspective
- FOV ~60
- relative offset around (0, +2.9, -6.5), assuming ~2-unit character height
- pitch ~16.5 degrees
- X follow strong
- small X damping initial guess ~0.08 s

Treat these as starting values only.

## Input model — observed
- hold + relative horizontal drag;
- auto-forward;
- release stops horizontal change;
- no lane switching.

Unknown from current video:
- exact pixel-to-world sensitivity;
- whether horizontal response uses velocity scaling;
- exact smoothing/inertia;
- exact max lateral speed.

## Input calibration plan
Record original Android gameplay with Developer Options -> Pointer location enabled.

Perform controlled tests:
1. 200 px right slowly.
2. 200 px right quickly.
3. 400 px right.
4. 200 px right then 200 px left.
5. Hold finger still.

Use result to estimate:
- normalized drag sensitivity;
- speed dependence;
- smoothing;
- frame lag;
- max lateral speed;
- camera X damping.

## Suggested level order
1. Start/tutorial.
2. Opening money + bottles.
3. First flag/section marker.
4. Party/School gate.
5. Middle pickups + hazards.
6. More section markers.
7. Dense pre-finish money.
8. x2.
9. x3.
10. x4.
11. x5.
12. Win.

## Important
Keep a strict distinction:
- OBSERVED = directly visible in reference;
- MEASURED = derived from repeatable measurement;
- ESTIMATE = implementation starting point only.
