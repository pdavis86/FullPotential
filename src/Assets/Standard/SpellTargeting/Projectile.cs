using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.SpellTargeting
{
    public class Projectile : ISpellTargeting
    {
        public Guid TypeId => new Guid("96941433-c786-46df-b2c8-7c10876c68e9");

        public string TypeName => nameof(Projectile);

        public bool HasShape => throw new NotImplementedException();
    }
}
