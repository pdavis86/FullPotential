using System;
using System.Collections.Generic;
using System.Linq;

namespace FullPotential.Api.Utilities
{
    public class ActionQueue<T>
    {
        private readonly List<Action<T>> _actions = new List<Action<T>>();

        public void Queue(Action<T> action)
        {
            _actions.Add(action);
        }

        public void PlayForwards(T value)
        {
            foreach (var action in _actions)
            {
                action(value);
            }
        }

        public void PlayBackwards(T value)
        {
            for (var i = _actions.Count - 1; i >= 0; i--)
            {
                _actions.ElementAt(i)(value);
            }
        }

    }
}
