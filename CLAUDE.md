# REROOT — Claude guide

Solo Unity game: 3/4 top-down (quarter view) active-incremental tree chopping. Portfolio piece, aiming for a Steam release (overseas too).
Unity 6000.3, URP 3D (Universal Renderer, Forward+), New Input System, VContainer, UniTask, R3, Cinemachine 3, Newtonsoft JSON.
- Our assets: `Assets/_Project/`, scripts: `Assets/_Project/Scripts/`
- Docs: `Docs/` — structure [Docs/ARCHITECTURE.md](Docs/ARCHITECTURE.md), code [Docs/CODE_CONVENTION.md](Docs/CODE_CONVENTION.md), git [Docs/GIT_CONVENTION.md](Docs/GIT_CONVENTION.md)
- **Write replies, code comments and commit messages in Korean.** In-game UI text stays English.

## Rule #1: never rebuild existing systems

Do NOT create new managers, save, upgrade, stat, game flow, pause, scene loading, object pool or sound systems.
They exist in `Scripts/Core`, `Scripts/Upgrade`, `Scripts/Audio`, `Scripts/Tree`. Read [Docs/ARCHITECTURE.md](Docs/ARCHITECTURE.md) before coding and build on it.

| Need | Use |
|---|---|
| Currency (wood) | `IWallet` (`Wallet`, Root) |
| Upgrade level / cost / purchase | `IUpgradeService` (`UpgradeService`, Root) |
| Final stat value after upgrades | `IStatService.Evaluate(StatType, baseValue)` (`StatService`, Root) |
| Lifetime records | `IPlayerRecords` (`PlayerRecords`, Root) |
| Save now / reset save | `ISaveService` (`SaveService`, Root) |
| Make something saveable | implement `ISaveable`, add a property to `SaveData`, register in `RootLifetimeScope` |
| Scene change | `ISceneLoader.LoadAsync(SceneId)` (saves first) |
| Game phase (forest / result / future upgrade screen) | `IGameStateMachine.ChangeState<T>()`, new phase = new `IGameState` class |
| Session start/end, result, timer | `ISessionService`, `ISessionTimer` |
| Pause | `IPauseService` (`IsPaused`, `Pause()`, `Resume()`) |
| Trees on the map | `IForest` (`TreeSpawner` + `TreePool`) |
| Something the axe can hit | implement `IDamageable` |
| Player from outside | `IPlayerSpawner` → `IPlayerControl` (`PlayerController` facade) |
| Camera / map bounds | `ICameraController` / `IMapBoundary` |
| Sound | `IAudioService` (plays), `AudioManager` (decides what to play), clips in `SoundLibrary` |
| UI | view interfaces in `UI/Views/`; always-on screens wired in `UIManager`, phase screens opened by their state |

- Missing a feature? Don't build a look-alike. **Tell the user first: "existing code needs this feature added".**
- Keep the two-level DI: things that survive scene changes go in `RootLifetimeScope` (prefab `Prefabs/Core/RootLifetimeScope`, registered in `Settings/VContainerSettings`); scene-only things go in the scene's LifetimeScope (`GameLifetimeScope`).
- Keep `VContainerSettings.DisableScriptModifier = true`. Otherwise VContainer overwrites new `*LifetimeScope.cs` files with an empty template.
- Scenes (`Assets/_Project/Scenes/`): `GameScene` exists. Planned: `TitleScene` → `GameScene` → `EndingScene` (`SceneId`: Title / Game / Ending).

## Code convention ([Docs/CODE_CONVENTION.md](Docs/CODE_CONVENTION.md))

| Target | Rule | Example |
|---|---|---|
| Fields, locals, parameters | `_camelCase` | `_moveSpeed`, `_damageAmount` |
| Classes, methods, properties | `PascalCase` (interfaces `I` prefix) | `TreeSpawner`, `TakeDamage()` |
| Constants | `UPPER_SNAKE_CASE` | `CURRENT_VERSION` |
| Async methods | `Async` suffix | `FallAsync()` |

- **No `Update` / `FixedUpdate` / `LateUpdate` (VContainer `ITickable` too).** Input: Input System `performed` events. Value changes: R3 subscriptions. Waiting: UniTask. Smooth changes: DOTween. Truly per-frame work (physics movement): R3 `Observable.EveryUpdate()` / `EveryUpdate(UnityFrameProvider.FixedUpdate)` with `.AddTo`.
- SOLID, no god objects, no singletons, no `FindObjectOfType`. Take other scripts as interfaces through VContainer constructor injection (or `[Inject]`).
- R3: expose read-only. UniTask: pass a `CancellationToken` (no coroutines, no `async void`).
- `///` summary on public methods when useful. TODO format: `// TODO: 내용 (작성자)`.
- Balance numbers live in ScriptableObjects (`Data/`), never hard-coded; don't change SO values at runtime.
- New upgrade = new `UpgradeData` asset added to `UpgradeCatalog_01_Default`, no code change. New stat = new `StatType` + `Evaluate` at the use site.
- Frequently created/destroyed objects: object pool, not `Instantiate/Destroy`.
- File name = type name, one top-level type per file. Asset names: `Type_00_Name` (e.g. `Tree_01_Birch`).
- Performance target: 120+ fps.
- Check Unity has no errors before committing.

## Assets

- VARCO-generated or external assets go only in `Assets/_Project/Art/Varco/` (gitignored). Blender sources and raw downloads go in `ArtSource/` at the repo root (gitignored).
- Optimize high-poly models in Blender before importing.
- VARCO 3D: always work in a new flow (the user's "REROOT" flow); never modify the user's other flows.
- Move files inside Unity, or move the `.meta` file together with the asset.

## Docs (`Docs/`)

- Write docs in Korean: short sentences, tables for lists of classes and rules. Use real names only and check they still exist.
- Keep docs in sync with code in the same PR as the code change, in a separate `doc:` commit. Structure changes (manager, service, state, saveable, view, scene, registration) → `ARCHITECTURE.md`; folder or code rule changes → `CODE_CONVENTION.md`; git/PR flow → `GIT_CONVENTION.md`.
- When a rule in `Docs/` changes, update the matching line in this file too (and the reverse).
- The repo is public: no other projects' documents, team members' names, or private info.

## Git convention ([Docs/GIT_CONVENTION.md](Docs/GIT_CONVENTION.md))

- Branches: `main` (stable), `dev` (integration). Always branch from `dev`, one branch per task: `feat/name`, `fix/name`, `doc/name` (lowercase, hyphens, e.g. `feat/upgrade-ui`).
- Commits: `feat: 한글 내용`, `fix: 한글 내용`, `doc: 한글 내용` (space after colon, Korean, one task per commit). Asset and balance changes are `feat`.
- **Merge through a PR:** push the branch, open a PR into `dev` (Korean title and description), merge on GitHub, then delete the branch (local and remote). `main` merges only from `dev` by PR.
- **Commit, push, open PRs and merge only when the user asks.**
- Never commit: `ProjectSettings` changes (if one is truly needed for the game to run, tell the user and commit it separately only with their OK), `Art/Varco/`, `Assets/Resources`, `ArtSource/` (assets are shared separately), DOTween / NuGet plugin folders (`Assets/Packages/`, restored from `packages.config`).
- Don't edit the same scene on two branches at once; experiment in a test scene.
- Tell the user before entering play mode for a test.
