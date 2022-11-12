﻿using FullPotential.Api.Ioc;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class Interactable : MonoBehaviour
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public float Radius = 3f;
        public bool RequiresServerCheck = true;
        // ReSharper restore FieldCanBeMadeReadOnly.Global

        // ReSharper disable once InconsistentNaming
        protected TMPro.TextMeshProUGUI _interactionBubble;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _interactionBubble = DependenciesContext.Dependencies.GetService<IModHelper>().GetGameManager().GetUserInterface().InteractionBubbleOverlay.GetComponent<TMPro.TextMeshProUGUI>();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable()
        {
            if (_interactionBubble != null)
            {
                OnBlur();
            }
        }

        public abstract void OnFocus();

        public abstract void OnInteract(NetworkObject networkObject);

        public abstract void OnBlur();
    }
}