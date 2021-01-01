using Assets.Scripts.Ui.Crafting;
using UnityEngine;
using UnityEngine.UI;

public class ComponentRemove : MonoBehaviour
{
    void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(OnClick); ;
    }

    private void OnClick()
    {
        //Change the name so it is not used in the UI before being destroyed
        var slot = transform.parent.parent.gameObject;
        slot.name = "Deleting";

        Destroy(transform.parent.parent.gameObject);

        UiHelper.UpdateResults(transform.parent.parent.parent.parent.parent, new ResultFactory());
    }

}
