# Breakout Unity

A single-player Breakout game built with Unity and C#. Move the paddle, launch the ball, and clear all 50 bricks before losing your three lives. Where the ball hits the paddle determines its rebound direction.

## Tech stack

| Technology | Role in this project |
| --- | --- |
| Unity Editor **6000.3.11f1** (Unity 6) | Opens the project, runs the scene, and builds the game. This version is recorded in `FinalBuild/ProjectSettings/ProjectVersion.txt`. |
| C# and `MonoBehaviour` | Implement initialization, input, movement, collisions, scoring, and game state. |
| Built-in rendering pipeline | An orthographic camera and `SpriteRenderer` components display the game in 2D. Textures and sprites are created in code at startup. |
| Legacy Unity input (`UnityEngine.Input`) | Reads keyboard and mouse controls. The project uses the old Input Manager, rather than an Input Actions asset. |
| Unity IMGUI (`OnGUI`) | Draws the score, lives, remaining bricks, controls, and pause/win/game-over messages. |
| Unity Editor APIs | Provide the custom scene-setup menu, Windows build command, and smoke-test runner. |
| Unity Package Manager | Reads `Packages/manifest.json` and restores the project's declared packages. |

The package manifest also includes Multiplayer Center, accessibility, adaptive performance, IMGUI, and vector graphics packages/modules. The game itself is local single-player; the gameplay script does not use a multiplayer service. There is no separate backend, database, or web server to start.

## Project layout

```text
BreakOutUnity/
├── README.md
├── FinalBuild/                         # Open THIS folder as the Unity project
│   ├── Assets/
│   │   ├── Scenes/Breakout.unity        # Playable scene
│   │   ├── Scripts/BreakoutGame.cs      # Runtime gameplay and visuals
│   │   └── Editor/
│   │       ├── BreakoutBuild.cs         # Scene setup and Windows export
│   │       └── BreakoutSmokeTests.cs    # Automated gameplay checks
│   ├── Packages/                       # Dependency manifest and lockfile
│   ├── ProjectSettings/                # Unity version, input, graphics, build settings
│   ├── Builds/Windows/                 # Windows player output, when built
│   └── My project (2)/                 # Separate nested Unity project; not the game above
├── Documentation/                      # Documentation placeholder
└── UnityBreakOutFA20.1.unitypackage     # Starter archive; not required to run this game
```

Unity generates folders such as `Library`, `Temp`, and `Logs`. They are not the gameplay source.

## How the moving parts work together

1. **The scene starts the game.** `Breakout.unity` contains a main camera and a GameObject with the `BreakoutGame` component. Unity calls that component's `Start()` method when you press Play.
2. **Startup creates the board.** `Start()` configures the camera and creates textures, sprites, walls, the paddle, and the ball. `ResetGame()` creates five rows of ten bricks, resets the score and three lives, and calls `Serve()` to place the ball above the paddle. No external art or prefab setup is required.
3. **Input moves the paddle.** Each frame, `Update()` reads the keyboard or held left mouse button, moves the paddle, and clamps it inside the court. Before launch, the ball follows the paddle. Space gives it an initial velocity.
4. **The script simulates the ball.** `Update()` calls `TickBall()` in steps of at most 1/240 second to reduce the chance of passing through thin objects. Collision handling uses positions and rectangle bounds in C#; it does not rely on `Rigidbody2D` or Unity physics colliders. Walls reflect velocity, and the paddle's hit position sets the bounce angle.
5. **Collisions update game state and visuals.** Hitting a brick removes its visual and entry from the brick list and adds points. Rows award 50, 40, 30, 20, and 10 points per brick from top to bottom. Missing the ball removes a life and serves again if lives remain. Clearing the bricks wins; losing all lives ends the game.
6. **The HUD reads the same state.** `OnGUI()` displays the current score, lives, brick count, and status. Escape pauses/resumes gameplay, and R rebuilds the board for a new game.

`BreakoutBuild.cs` runs only inside the editor. It opens or creates the game scene, registers it in the build settings, and exports a Windows player. `BreakoutSmokeTests.cs` uses that setup and exercises the runtime component in Play mode.

## Start the project

### Run in the Unity Editor

1. Install Unity Hub and Unity Editor **6000.3.11f1**. Add **Windows Build Support** if you want to export the Windows game.
2. In Unity Hub, add an existing project and select the **`FinalBuild`** folder inside this repository. Select the folder containing `Assets`, `Packages`, and `ProjectSettings`; do not select the repository root or `FinalBuild/My project (2)`.
3. Open the project with that editor version. Allow Unity to import assets, restore packages, and compile scripts. The first import may take a while and package restoration may need internet access.
4. Open **`Assets/Scenes/Breakout.unity`** in the Project window. You can also choose **Breakout > Create or open game scene** from the editor menu.
5. Press **Play**, click the **Game** view so it receives keyboard input, and press **Space** to launch.

The older `FinalBuild/ReadMe.txt` mentions Unity 2022.3.20f1. Use the version in `ProjectVersion.txt`, **6000.3.11f1**, for the current project.

### Controls

| Action | Control |
| --- | --- |
| Move left/right | A / D or Left / Right arrows |
| Move to mouse position | Hold the left mouse button |
| Launch the ball after a serve | Space |
| Pause/resume | Escape |
| Restart, including after winning or losing | R |

### Run the Windows build

If the build output is present, open **`FinalBuild/Builds/Windows/Breakout.exe`** directly. Unity does not need to be open to play the exported game.

To create a fresh build, open the project and choose **Breakout > Build Windows game**. The build script creates `Builds/Windows/Breakout.exe` relative to `FinalBuild`, using the game scene and a 1280 × 800 windowed configuration.

When sharing the build, keep the entire `Windows` output folder together, including `Breakout_Data`, `UnityPlayer.dll`, and the other generated files.

## Automated smoke checks

The custom smoke runner checks the initial brick count, wall and paddle rebounds, brick removal and scoring, life loss, game over, winning, and restart. It runs through Unity rather than a standalone .NET test command.

Close the project in the editor before running this PowerShell command from the repository root. Adjust the executable path if Unity is installed elsewhere:

```powershell
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe'
& $unityEditor -batchmode -projectPath "$PWD\FinalBuild" -executeMethod BreakoutSmokeTests.Run -logFile "$PWD\FinalBuild\smoke-tests.log"
```

Do **not** add `-quit`: the runner needs to enter Play mode and exits Unity itself with code 0 on success or 1 on failure. Look for `BREAKOUT SMOKE TESTS PASSED` in the log.

## Troubleshooting

- **An empty or unrelated sample scene opens:** confirm you opened `FinalBuild`, then use **Breakout > Create or open game scene**.
- **Controls do not respond:** click the Game view, make sure Play mode is active, and press Escape if the game is paused.
- **Unity requests a different editor version:** check `FinalBuild/ProjectSettings/ProjectVersion.txt` and install the matching version through Unity Hub.
- **Windows export fails because platform support is missing:** add Windows Build Support to the editor installation in Unity Hub and retry the build menu command.
- **The Breakout menu is missing:** wait for compilation to finish and inspect the Unity Console for compile errors.
