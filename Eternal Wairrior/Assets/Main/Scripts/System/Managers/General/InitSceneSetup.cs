#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class InitSceneSetup
{
    [MenuItem("Setup/Create Init Scene")]
    public static void CreateInitScene()
    {
        // 货肺款 纠 积己
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

        // InitializationManager 积己
        var initGO = new GameObject("InitializationManager");
        initGO.AddComponent<InitializationManager>();

        // 纠 历厘
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/InitScene.unity");

        Debug.Log("Init Scene created successfully!");
    }
}
#endif