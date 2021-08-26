using System.Linq;
using UnityEngine;

namespace FullPotential.Assets.Core.Helpers
{
    public static class UnityHelper
    {
        public static GameObject GetObjectAtRoot(string name)
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == name);
        }

        //public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
        //{
        //    var uiLayer = LayerMask.NameToLayer("UI");
        //    return GetEventSystemRaycastResults().FirstOrDefault(x => x.gameObject.layer == uiLayer).gameObject != null;
        //}

        //static List<RaycastResult> GetEventSystemRaycastResults()
        //{
        //    var raycastResults = new List<RaycastResult>();
        //    EventSystem.current.RaycastAll(
        //        new PointerEventData(EventSystem.current)
        //        {
        //            position = Input.mousePosition
        //        }, raycastResults
        //    );
        //    return raycastResults;
        //}

    }
}
