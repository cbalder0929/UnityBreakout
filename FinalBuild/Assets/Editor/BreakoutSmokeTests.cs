using System;
using System.Collections;
using System.Reflection;
using Breakout;
using UnityEditor;
using UnityEngine;

// Run with -batchmode -executeMethod BreakoutSmokeTests.Run (without -quit).
[InitializeOnLoad]
public static class BreakoutSmokeTests
{
    const string Pending = "Breakout.SmokeTests";
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static BreakoutSmokeTests() { EditorApplication.update += Check; }
    public static void Run()
    {
        BreakoutBuild.CreateScene();
        SessionState.SetBool(Pending, true);
        EditorApplication.isPlaying = true;
    }

    static void Check()
    {
        if (!SessionState.GetBool(Pending, false) || !EditorApplication.isPlaying) return;
        var game = UnityEngine.Object.FindObjectOfType<BreakoutGame>();
        if (game == null || Get<int>(game, "lives") == 0) return;
        SessionState.SetBool(Pending, false);
        try
        {
            Require(Get<IList>(game, "bricks").Count == 50, "50 initial bricks");
            Set(game, "position", new Vector2(7.83f, 0));
            Set(game, "velocity", new Vector2(7, 0));
            Tick(game);
            Require(Get<Vector2>(game, "velocity").x < 0, "wall rebound");
            Set(game, "position", new Vector2(.8f, -3.69f));
            Set(game, "velocity", new Vector2(0, -7));
            Tick(game);
            Require(Get<Vector2>(game, "velocity").y > 0 && Get<Vector2>(game, "velocity").x > 0, "paddle rebound and aim");
            Set(game, "position", new Vector2(-6.75f, .64f));
            Set(game, "velocity", new Vector2(0, 7));
            Tick(game);
            Require(Get<IList>(game, "bricks").Count == 49 && Get<int>(game, "score") == 10, "brick destruction and scoring");
            Require(Get<Vector2>(game, "velocity").y < 0, "brick rebound");
            Set(game, "position", new Vector2(0, -5.6f));
            Tick(game);
            Require(Get<int>(game, "lives") == 2 && !Get<bool>(game, "launched"), "life lost and serve reset");
            Set(game, "lives", 1);
            Set(game, "position", new Vector2(0, -5.6f));
            Tick(game);
            Require(Get<bool>(game, "ended"), "game over");
            Call(game, "ResetGame");
            var bricks = Get<IList>(game, "bricks");
            while (bricks.Count > 1)
            {
                var brick = bricks[bricks.Count - 1];
                UnityEngine.Object.Destroy((GameObject)brick.GetType().GetField("visual").GetValue(brick));
                bricks.RemoveAt(bricks.Count - 1);
            }
            Set(game, "position", new Vector2(-6.75f, 3.23f));
            Set(game, "velocity", new Vector2(0, 7));
            Tick(game);
            Require(Get<bool>(game, "ended") && bricks.Count == 0, "win state");
            Call(game, "ResetGame");
            Require(bricks.Count == 50 && Get<int>(game, "score") == 0 && Get<int>(game, "lives") == 3 && !Get<bool>(game, "ended"), "restart");
            Debug.Log("BREAKOUT SMOKE TESTS PASSED: walls, paddle aim, bricks, score, lives, game over, win, restart.");
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    static T Get<T>(object game, string name) { return (T)typeof(BreakoutGame).GetField(name, Private).GetValue(game); }
    static void Set(object game, string name, object value) { typeof(BreakoutGame).GetField(name, Private).SetValue(game, value); }
    static void Call(object game, string name) { typeof(BreakoutGame).GetMethod(name, Private).Invoke(game, null); }
    static void Tick(object game) { typeof(BreakoutGame).GetMethod("TickBall", Private).Invoke(game, new object[] { 1f / 240f }); }
    static void Require(bool condition, string description) { if (!condition) throw new Exception("Failed: " + description); }
}
