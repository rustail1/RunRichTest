# TEST_PLAN.md

## EditMode targets
### WealthModel
- starts from configured value;
- clamps min/max;
- positive delta works;
- negative delta works;
- state transition at configured thresholds;
- one change produces one notification.

### GameFlow
- Start -> Playing;
- Playing -> Win;
- Playing -> Lose;
- invalid duplicate transitions do not corrupt state.

### Input math
- normalized pointer delta maps to expected horizontal delta;
- zero pointer movement produces zero horizontal change;
- clamp works.

## PlayMode targets
### Runner
- first swipe starts run;
- forward motion remains stable;
- horizontal movement stops when pointer stops;
- player cannot leave road bounds.

### Pickup
- applies once;
- cannot double-trigger;
- disables/consumes after collect.

### Gate
- only one side can be applied;
- gate cannot apply twice.

### Finish
- finish triggers Win once;
- input/movement stops on Win;
- Next Level calls level service.

### Restart
- Lose -> restart works;
- relevant state resets.

## Manual reference checks
- player screen composition;
- camera tracking;
- swerve feel;
- wealth checkpoints;
- appearance transitions;
- gate presentation;
- x2/x3/x4/x5 sequence;
- win presentation.

## Regression
Before submission:
- full level from start to win;
- deliberate bottle collision;
- gate choice;
- restart path;
- next-level path;
- Console clean;
- no repeated trigger spam;
- no missing references.
