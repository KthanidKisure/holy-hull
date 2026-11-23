# Holy Hull

CM2119 Experimental Game Mechanics coursework. Made in Unity 6000.6.10f1 (module version is 6000.3.24f1 LTS).

**Spaceship, but... the crew are useless holy knights.** Fly the cathedral-ship, collect a relic in each of three star systems and fly through the black hole warp to wake the Emperor. The crew causes random "holy malfunctions" along the way.

## Controls

| Key | Action |
|---|---|
| W | Thrust |
| A / D | Turn |
| Space | Fire / start / continue |
| Esc | Quit |

## Opening the project

1. Open the `HolyHull` folder in Unity Hub with Unity 6000.3.24f1.
2. Open `Assets/Scenes/Menu.unity` and press Play.
3. If anything goes missing, **Holy Hull > Build Everything** in the top menu rebuilds the sprites settings, prefabs, animator and all six scenes.

## Scenes

Menu → Level1 → Level2 → Level3 → Victory, or GameOver when faith runs out.

## Scripts

| Script | What it does |
|---|---|
| `MoveShip` | Thrust and rotation with forces, firing, taking hits |
| `MoveBullet` | Bolt movement, destroys asteroids and heretics, adds score |
| `AsteroidMove` | Random drift and spin, sparks on collision |
| `HereticShip` | Enemy drone that chases the player |
| `Relic` | Pickup that opens the warp |
| `BlackHole` | Warp trigger that loads the next scene |
| `LevelManager` | Spawns the level, runs malfunctions, updates the HUD |
| `GameState` | Faith, score and relics kept between scenes |
| `AstroAnimation` | Brother Astro's Idle/Run animation on the menu |
| `MenuController`, `ScoreText`, `AutoDestroy` | Screen input, score on end screens, effect clean-up |
| `Editor/GameBuilder` | Builds the scenes and prefabs |

## Credits

All sprites, UI art, sound effects and music were made for this project. Fonts: Pirata One and VT323 (Google Fonts, SIL Open Font License, see `Assets/Fonts`).
