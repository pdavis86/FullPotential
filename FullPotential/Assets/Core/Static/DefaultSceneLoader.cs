#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// ReSharper disable UnusedType.Global
// ReSharper disable AccessToStaticMemberViaDerivedType

namespace FullPotential.Core.Static
{
    [InitializeOnLoadAttribute]
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
                {
                    var activeScene = EditorSceneManager.GetActiveScene();
                    if (activeScene.buildIndex != 0)
                    {
                        EditorSceneManager.LoadScene(0);
                        Debug.LogWarning("Force loaded scene 0 as it was not the active scene in the editor");
                    }

                    break;
                }
            }
        }
    }
}
#endif
