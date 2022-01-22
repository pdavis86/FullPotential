using System;
using UnityEngine;

namespace FullPotential.Core.Utilities
{
    public class DelayedAction
    {
        private readonly float _timeBetweenEffects;
        private readonly Action _actionToDo;

        private float _timeSinceLastEffective;

        public DelayedAction(float delay, Action actionToDo)
        {
            _timeBetweenEffects = delay;
            _timeSinceLastEffective = _timeBetweenEffects;

            _actionToDo = actionToDo;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool TryPerformAction()
        {
            if (_timeSinceLastEffective < _timeBetweenEffects)
            {
                _timeSinceLastEffective += Time.deltaTime;
                return false;
            }

            _timeSinceLastEffective = 0;

            _actionToDo();

            return true;
        }
    }
}
