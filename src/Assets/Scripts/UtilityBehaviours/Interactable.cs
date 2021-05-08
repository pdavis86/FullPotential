using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public abstract class Interactable : NetworkBehaviour
{
    public float Radius = 3f;

    internal TMPro.TextMeshProUGUI _interactionBubble;

    void Start()
    {
        _interactionBubble = GameManager.Instance.MainCanvasObjects.InteractionBubble.GetComponent<TMPro.TextMeshProUGUI>();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }

    public abstract void OnFocus();

    public abstract void OnInteract(NetworkInstanceId playerNetId);

    public abstract void OnBlur();
}
