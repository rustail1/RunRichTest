# README_SETUP.md

## Step 1 — Create project
Unity Hub:
- New project
- Editor: Unity 6.3 LTS (6000.3.11f1)
- Template: 3D / Built-In Render Pipeline
- Project name suggestion: `RunRichTest`
- Create

## Step 2 — Unity project settings
In Unity:
- Edit -> Project Settings -> Editor
- Version Control / Meta Files: Visible Meta Files
- Asset Serialization: Force Text

## Step 3 — Git
At project root:

```bash
git init
git add .
git commit -m "chore: clean Unity 6.3 project"
```

Use the included `.gitignore`.

## Step 4 — Add documentation
Copy all root `.md` files from this pack into the project root.
Commit them separately:

```bash
git add *.md
git commit -m "docs: add architecture and development rules"
```

## Step 5 — Import vendor/reference content
Recommended destinations:
- `Assets/ThirdParty/ButchersGames/`
- `Assets/ThirdParty/ReferenceAssets/`

After import:
- wait for Unity to finish;
- resolve compile errors;
- do not start gameplay code yet;
- commit vendor import separately.

## Step 6 — Codex
Open Codex at project root.

First prompt:

> Read AGENTS.md, TASK.md, ARCHITECTURE.md, ROADMAP.md and DECISIONS.md. Inspect the current Unity project and the imported BG_LevelManager. Do not modify anything. Identify the existing runtime/editor code, vendor dependencies, likely compile/build issues, and propose the smallest architecture skeleton that conforms to ARCHITECTURE.md. List the exact files you would create or touch.

Review the plan before allowing code changes.
