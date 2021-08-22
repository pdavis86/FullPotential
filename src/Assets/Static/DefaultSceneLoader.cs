﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global
// ReSharper disable AccessToStaticMemberViaDerivedType

[InitializeOnLoadAttribute]
public static class DefaultSceneLoader
{
    static DefaultSceneLoader()
    {
        EditorApplication.playModeStateChanged += LoadDefaultScene;
    }

    static void LoadDefaultScene(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.buildIndex != 0)
            {
                EditorSceneManager.LoadScene(0);
                Debug.Log("Scene 0 force loaded");
            }
        }
    }
}
#endif
