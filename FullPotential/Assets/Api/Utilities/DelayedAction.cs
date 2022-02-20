using System;
using UnityEngine;

namespace FullPotential.Api.Utilities
{
    public class DelayedAction
    {
        private readonly Action _actionToDo;

        private float _timeBetweenActions;
        private float _timeSinceLastAction;

        public DelayedAction(float delay, Action actionToDo, bool noDelayForFirstCall = true)
        {
            _timeBetweenActions = delay;
            _timeSinceLastAction = noDelayForFirstCall ? _timeBetweenActions : 0;

            _actionToDo = actionToDo;
        }

        // ReSharper disable once UnusedMember.Global
        public void SetTimeBetweenEffects(float delay)
        {
            _timeBetweenActions = delay;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool TryPerformAction()
        {
            if (_timeSinceLastAction < _timeBetweenActions)
            {
                _timeSinceLastAction += Time.deltaTime;
                return false;
            }

            _actionToDo();

            _timeSinceLastAction = 0;

            return true;
        }
    }
}
