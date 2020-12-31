using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Ui.Crafting
{
    public class ResultFactory
    {
        private readonly Random _random = new Random();

        private int ComputeAttribute(List<Attributes> components, Func<Attributes, int> getProp)
        {
            var min = components.Min(getProp);
            var max = components.Max(getProp);

            var topEndSkew = max - ((max - min) / 10);

            int result;
            if (_random.Next(1, 11) < 9)
            {
                result = (int)Math.Round(topEndSkew - (0.009 * topEndSkew), MidpointRounding.AwayFromZero);
            }
            else
            {
                result = topEndSkew;
            }

            //Debug.Log($"{getProp.Method.Name} = Min:{min}, Max:{max}, Skew:{topEndSkew}, Result:{result}");

            return result;
        }

        private int GetValue(int rarityThreshold)
        {
            return _random.Next(0, 100) > rarityThreshold ? _random.Next(1, 100) : 0;
        }

        internal Attributes GetRandom()
        {
            return new Attributes
            {
                IsActivated = _random.Next(0, 100) > 50,
                IsMultiShot = _random.Next(0, 100) > 75,
                Type = GetValue(25),
                Strength = GetValue(25),
                Cost = GetValue(25),
                Range = GetValue(25),
                Accuracy = GetValue(25),
                Speed = GetValue(25),
                Recovery = GetValue(25),
                Duration = GetValue(25)
            };
        }

        internal Attributes Spell(List<Attributes> components)
        {
            return new Attributes
            {
                IsActivated = true,
                Strength = ComputeAttribute(components, x => x.Strength),
                Cost = ComputeAttribute(components, x => x.Cost),
                Range = ComputeAttribute(components, x => x.Range),
                Accuracy = ComputeAttribute(components, x => x.Accuracy),
                Speed = ComputeAttribute(components, x => x.Speed),
                Recovery = ComputeAttribute(components, x => x.Recovery),
                Duration = ComputeAttribute(components, x => x.Duration)
            };
        }

        internal Attributes Dagger(List<Attributes> components)
        {
            return new Attributes
            {
                IsActivated = components.Any(x => x.IsActivated),
                Strength = ComputeAttribute(components, x => x.Strength),
                Accuracy = ComputeAttribute(components, x => x.Accuracy),
                Speed = ComputeAttribute(components, x => x.Speed)
            };
        }

        internal Attributes Axe(List<Attributes> components)
        {
            return new Attributes
            {
                IsActivated = components.Any(x => x.IsActivated),
                Strength = ComputeAttribute(components, x => x.Strength),
                Accuracy = ComputeAttribute(components, x => x.Accuracy),
                Speed = ComputeAttribute(components, x => x.Speed)
            };
        }


        internal Attributes Sword(List<Attributes> components)
        {
            return new Attributes
            {
                IsActivated = components.Any(x => x.IsActivated),
                Strength = ComputeAttribute(components, x => x.Strength),
                Accuracy = ComputeAttribute(components, x => x.Accuracy),
                Speed = ComputeAttribute(components, x => x.Speed)
            };
        }

        internal Attributes Hammer(List<Attributes> components)
        {
            return new Attributes
            {
                IsActivated = components.Any(x => x.IsActivated),
                Strength = ComputeAttribute(components, x => x.Strength),
                Accuracy = ComputeAttribute(components, x => x.Accuracy),
                Speed = ComputeAttribute(components, x => x.Speed)
            };
        }

        internal Attributes Spear(List<Attributes> components)
        {
            return new Attributes
            {
                IsActivated = components.Any(x => x.IsActivated),
                Strength = ComputeAttribute(components, x => x.Strength),
                Accuracy = ComputeAttribute(components, x => x.Accuracy),
                Speed = ComputeAttribute(components, x => x.Speed)
            };
        }

        internal Attributes Bow(List<Attributes> components)
        {
            return new Attributes
            {
                IsActivated = components.Any(x => x.IsActivated),
                IsMultiShot = components.Any(x => x.IsMultiShot),
                Type = ComputeAttribute(components, x => x.Type),
                Strength = ComputeAttribute(components, x => x.Strength),
                Cost = ComputeAttribute(components, x => x.Cost),
                Range = ComputeAttribute(components, x => x.Range),
                Accuracy = ComputeAttribute(components, x => x.Accuracy),
                Speed = ComputeAttribute(components, x => x.Speed),
                Recovery = ComputeAttribute(components, x => x.Recovery)
            };
        }
        internal Attributes Crossbow(List<Attributes> components)
        {
            return new Attributes
            {
                IsMultiShot = components.Any(x => x.IsMultiShot),
                Type = ComputeAttribute(components, x => x.Type),
                Strength = ComputeAttribute(components, x => x.Strength),
                Cost = ComputeAttribute(components, x => x.Cost),
                Range = ComputeAttribute(components, x => x.Range),
                Accuracy = ComputeAttribute(components, x => x.Accuracy),
                Speed = ComputeAttribute(components, x => x.Speed),
                Recovery = ComputeAttribute(components, x => x.Recovery)
            };
        }

        internal Attributes Gun(List<Attributes> components)
        {
            return new Attributes
            {
                IsMultiShot = components.Any(x => x.IsMultiShot),
                Type = ComputeAttribute(components, x => x.Type),
                Strength = ComputeAttribute(components, x => x.Strength),
                Cost = ComputeAttribute(components, x => x.Cost),
                Range = ComputeAttribute(components, x => x.Range),
                Accuracy = ComputeAttribute(components, x => x.Accuracy),
                Speed = ComputeAttribute(components, x => x.Speed),
                Recovery = ComputeAttribute(components, x => x.Recovery)
            };
        }

        internal Attributes Shield(List<Attributes> components)
        {
            return new Attributes
            {
                IsActivated = components.Any(x => x.IsActivated),
                Strength = ComputeAttribute(components, x => x.Strength),
                Speed = ComputeAttribute(components, x => x.Speed),
                Recovery = ComputeAttribute(components, x => x.Recovery)
            };
        }

        internal Attributes Helm(List<Attributes> components)
        {
            return new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Strength)
            };
        }
        internal Attributes Chest(List<Attributes> components)
        {
            return new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Strength)
            };
        }

        internal Attributes Legs(List<Attributes> components)
        {
            return new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Strength)
            };
        }
        internal Attributes Feet(List<Attributes> components)
        {
            return new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Strength)
            };
        }

        internal Attributes Gloves(List<Attributes> components)
        {
            return new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Strength)
            };
        }

        internal Attributes Barrier(List<Attributes> components)
        {
            return new Attributes
            {
                IsActivated = components.Any(x => x.IsActivated),
                Strength = ComputeAttribute(components, x => x.Strength),
                Cost = ComputeAttribute(components, x => x.Cost),
                Speed = ComputeAttribute(components, x => x.Speed),
                Recovery = ComputeAttribute(components, x => x.Recovery)
            };
        }

        internal Attributes Amulet(List<Attributes> components)
        {
            return new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Strength)
            };
        }

        internal Attributes Ring(List<Attributes> components)
        {
            return new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Strength)
            };
        }

        internal Attributes Belt(List<Attributes> components)
        {
            return new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Strength)
            };
        }

    }
}
