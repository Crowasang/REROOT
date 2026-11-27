# REROOT Docs — Claude guide

Loaded when working on files in `Docs/`. Project-wide rules (systems, code, assets, git) are in [../CLAUDE.md](../CLAUDE.md); this file only covers the docs themselves.
- **Write docs in Korean.** Short sentences, tables for lists of classes and rules.

## Which doc covers what

| File | Covers | Update when |
|---|---|---|
| [ARCHITECTURE.md](ARCHITECTURE.md) | Two-level DI (`RootLifetimeScope` / scene scope), Root services, game flow state machine, upgrades and stats, save, scene managers, scene objects, how to add features, registration rules | A manager, service, state, saveable, view or scene is added/removed, or a VContainer registration changes |
| [CODE_CONVENTION.md](CODE_CONVENTION.md) | Naming, folder tree, class member order, no `Update`, dependencies, comments, data, UniTask, R3 | A folder is added/moved or a code rule changes |
| [GIT_CONVENTION.md](GIT_CONVENTION.md) | Branches (`main`/`dev`/`feat`/`fix`/`doc`), PR flow, commit types, never-commit list | The git or PR flow changes |
| [CONVENTION.md](CONVENTION.md) | One-page summary and links | A doc is added or renamed |

## Rules

- Keep docs in sync with code **in the same PR** as the code change. Put doc edits in their own commit (`doc: ...`).
- Use real class / interface / file names and check they still exist before writing them. Remove entries for deleted code.
- Root [../CLAUDE.md](../CLAUDE.md) summarizes these docs. When a rule here changes, update the matching line there too (and the reverse).
- "How to add" sections describe the actual steps (which file, which registration line), not general advice.
- Links are relative to `Docs/` (`CODE_CONVENTION.md`, `../CLAUDE.md`).
- The repo is public: no other projects' documents, team members' names, or private info.
