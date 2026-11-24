using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using Breakout;

public static class BreakoutBuild
{
    const string ScenePath = "Assets/Scenes/Breakout.unity";

    [MenuItem("Breakout/Create or open game scene")]
    public static void CreateScene()
    {
        if (File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath);
        else
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Breakout Game", typeof(BreakoutGame));
            var camera = new GameObject("Main Camera", typeof(Camera));
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 0, -10);
            camera.GetComponent<Camera>().orthographic = true;
            camera.GetComponent<Camera>().orthographicSize = 6.2f;
            EditorSceneManager.SaveScene(scene, ScenePath);
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.companyName = "Breakout";
        PlayerSettings.productName = "Breakout";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 800;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Breakout/Build Windows game")]
    public static void BuildWindows()
    {
        CreateScene();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath }, locationPathName = "Builds/Windows/Breakout.exe",
            target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Breakout build failed: " + report.summary.result);
        Debug.Log("Breakout Windows build succeeded.");
    }
}
