﻿using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpellsAndGadgets.Behaviours
{
    public class SogBeamBehaviour : MonoBehaviour, ISpellOrGadgetBehaviour
    {
        public SpellOrGadgetItemBase SpellOrGadget;
        public IFighter SourceFighter;
        public bool IsLeftHand;

#pragma warning disable 0649
        [SerializeField] private float _leftRightAdjustment;
#pragma warning restore CS0649

        private IEffectService _effectService;

        private GameObject _sourcePlayer;
        private Transform _cylinderParentTransform;
        private Transform _cylinderTransform;
        private RaycastHit _hit;
        private DelayedAction _applyEffectsAction;
        private float _maxBeamLength;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _cylinderParentTransform = transform.GetChild(0);
            _cylinderTransform = _cylinderParentTransform.GetChild(0);

            _effectService = ModHelper.GetGameManager().GetService<IEffectService>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (SpellOrGadget == null)
            {
                Debug.LogError("No SpellOrGadget has been set");
                Destroy(gameObject);
                return;
            }

            _maxBeamLength = SpellOrGadget.Attributes.GetContinuousRange();

            _sourcePlayer = GameObjectHelper.ClosestParentWithTag(gameObject, Tags.Player);

            if (_sourcePlayer == null)
            {
                Debug.LogError("No player found in parents");
                Destroy(gameObject);
                return;
            }

            PerformGraphicsAdjustments();

            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var effectsDelay = SpellOrGadget.Attributes.GetTimeBetweenEffects();
            _applyEffectsAction = new DelayedAction(effectsDelay, () => ApplyEffects(_hit.transform.gameObject, _hit.point));
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            Vector3 targetDirection;
            float beamLength;
            if (Physics.Raycast(SourceFighter.LookTransform.position, SourceFighter.LookTransform.forward, out var hit, _maxBeamLength))
            {
                if (hit.transform.gameObject == _sourcePlayer)
                {
                    Debug.LogWarning("Beam is hitting the source player!");
                    return;
                }

                _hit = hit;

                if (NetworkManager.Singleton.IsServer)
                {
                   _applyEffectsAction.TryPerformAction();
                }

                targetDirection = (hit.point - _cylinderParentTransform.position).normalized;
                beamLength = Vector3.Distance(_cylinderParentTransform.position, hit.point);
            }
            else
            {
                targetDirection = SourceFighter.LookTransform.forward;
                beamLength = _maxBeamLength;
            }

            UpdateBeam(targetDirection, beamLength);
        }

        // ReSharper disable once UnusedMember.Global
        public void OnDestroy()
        {
            Destroy(_cylinderParentTransform.gameObject);
        }

        // ReSharper disable once UnusedMember.Global
        public void StartCasting()
        {
            //Nothing here
        }

        public void Stop()
        {
            Destroy(gameObject);
        }

        public void ApplyEffects(GameObject target, Vector3? position)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            _effectService.ApplyEffects(_sourcePlayer, SpellOrGadget, target, position);
        }

        private void PerformGraphicsAdjustments()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                return;
            }

            _cylinderParentTransform.parent = SourceFighter.LookTransform;

            if (SourceFighter.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                //Adjust for FoV
                var adjustment = (Camera.main.fieldOfView - 50) * 0.0125f;
                _cylinderParentTransform.position -= _sourcePlayer.transform.forward * adjustment;

                //Move it a little sideways
                _cylinderParentTransform.position += (IsLeftHand ? _leftRightAdjustment : -_leftRightAdjustment) * _sourcePlayer.transform.right;
            }

            //Move the tip to the middle
            _cylinderTransform.position += _cylinderTransform.up * _cylinderTransform.localScale.y;

            _cylinderTransform.gameObject.SetActive(false);
        }

        private void UpdateBeam(Vector3 targetDirection, float beamLength)
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                return;
            }

            _cylinderParentTransform.rotation = Quaternion.LookRotation(targetDirection);

            if (!Mathf.Approximately(_cylinderTransform.localScale.y * 2, beamLength))
            {
                _cylinderTransform.localScale = new Vector3(_cylinderTransform.localScale.x, beamLength / 2, _cylinderTransform.localScale.z);
                _cylinderTransform.position = _cylinderParentTransform.position + (_cylinderTransform.up * _cylinderTransform.localScale.y);
            }

            if (!_cylinderTransform.gameObject.activeInHierarchy)
            {
                _cylinderTransform.gameObject.SetActive(true);
            }
        }

    }
}