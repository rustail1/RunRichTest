# Run Rich 3D — Unity Test Task

Reconstruction of the core gameplay loop and the first level of **Run Rich 3D**, created as a Unity developer test task.

## Gameplay Video

https://youtube.com/shorts/gGYoXAp_9W4?feature=share

## Unity Version

**Unity 6.3 LTS — 6000.3.11f1**

## Implemented

- Gameplay scene
- Camera and lighting
- Start UI
- Swipe / swerve movement
- Tutorial
- First playable level
- Money pickups
- Alcohol / negative pickups
- Wealth progression
- Character state changes based on wealth
- Checkpoints / flag areas
- Choice gates
- Finish multiplier section
- Victory flow
- Defeat / retry flow
- Next level transition
- Gameplay UI
- Idle / walk animation setup
- VFX and SFX presentation

Shop and meta systems are intentionally not included because they were outside the scope of the test task.

## Controls

- **Tap / hold** to start running
- **Drag left / right** to steer the character

## How to Run

1. Open the project in **Unity 6.3 LTS**.
2. Open:

   `Assets/_Game/Scenes/Gameplay_Clean.unity`

3. Enter **Play Mode**.

## Project Structure

- `Assets/_Game/Scenes/` — gameplay scene
- `Assets/_Game/Runtime/` — runtime gameplay code
- `Assets/_Game/Editor/` — scene builder / validation tools
- `Assets/ThirdParty/ReferenceAssets/` — reference meshes, materials, textures, VFX and sounds

## Notes

The project focuses on reproducing the reference gameplay, controls, first-level flow, UI and presentation as closely as possible within the scope of the test assignment.

The next-level transition is implemented; for demonstration purposes the authored gameplay level may be reused after the transition.

## Repository

https://github.com/rustail1/RunRichTest
