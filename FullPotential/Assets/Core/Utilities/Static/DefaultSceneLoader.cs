#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Utilities.Static
{
    [InitializeOnLoad]
    public static class DefaultSceneLoader
    {
        static DefaultSceneLoader()
        {
            EditorApplication.playModeStateChanged += LoadDefaultScene;
        }

        private static void LoadDefaultScene(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    var activeScene = SceneManager.GetActiveScene();
                    if (activeScene.buildIndex != 0)
                    {
                        SceneManager.LoadScene(0);
                        Debug.LogWarning("Force loaded scene 0 as it was not the active scene in the editor");
                    }

                    break;
            }
        }
    }
}

#endif
