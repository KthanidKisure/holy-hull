# Holy Hull

CM2119 Experimental Game Mechanics coursework. Made in Unity 6000.6.10f1 (module version is 6000.3.24f1 LTS).

**Spaceship, but... the crew are useless holy knights.** Fly the cathedral-ship, collect a relic in each of three star systems and fly through the warp to wake the Emperor. Brother Navigator, Brother Gunner and Brother Engineer keep causing "holy malfunctions" along the way.

**Play in the browser:** https://kthanidkisure.github.io/holy-hull/

## Controls

| Key | Action |
|---|---|
| W / Up | Thrust |
| A / D, Left / Right | Turn |
| Space or left click | Fire |
| Esc or P | Pause (volume, resume, main menu, quit) |

## Features

- Force-based ship movement with drift
- Three levels with their own background, more enemies each time and an Arch-Heretic boss on level 3
- Big asteroids split into small ones
- Heretic drones chase you and sometimes drop holy water (+1 faith)
- Random crew malfunctions: reversed controls, jammed cannon, no thrust
- Pause menu with music and sound sliders, saved high score
- Pixel-art sprites, explosion animation, screen shake, scrolling stars

## Opening the project

1. Open the `HolyHull` folder in Unity Hub.
2. Open `Assets/Scenes/Menu.unity` and press Play.
3. If anything goes missing, **Holy Hull > Build Everything** in the top menu rebuilds the sprite settings, prefabs, animator and all six scenes.

## Scenes

Menu → Level1 → Level2 → Level3 → Victory, or GameOver when faith runs out.

## Scripts

| Script | What it does |
|---|---|
| `MoveShip` | Thrust and rotation with forces, firing, taking hits, engine flame |
| `MoveBullet` | Bolt movement, tells asteroids and heretics they were hit |
| `AsteroidMove` | Random drift and spin, dust on collision, splitting |
| `HereticShip` | Enemy drone that chases the player, health, drops |
| `Relic`, `HolyWater` | Pickups |
| `BlackHole` | Warp trigger that loads the next scene |
| `LevelManager` | Spawns the level, runs malfunctions, updates the HUD |
| `GameState` | Faith, score, relics, volume and high score kept between scenes |
| `PauseMenu` | Pause, volume sliders, menu buttons |
| `AstroAnimation` | Brother Astro's Idle/Run animation on the menu |
| `SpriteAnimator`, `CameraShake`, `ScrollStars` | Explosion frames, screen shake, moving stars |
| `MenuController`, `ScoreText` | Screen input, score and high score text |
| `Editor/GameBuilder` | Builds the scenes and prefabs |

## Credits

All sprites, UI art, sound effects and music were made for this project. Font: VT323 (Google Fonts, SIL Open Font License, see `Assets/Fonts`).
