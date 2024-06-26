﻿using System;
using FullPotential.Api.Registry.Elements;

namespace FullPotential.Standard.Effects.Elements
{
    public class Earth : IElement
    {
        private static readonly Guid Id = new Guid("533bfd89-696d-497c-9e7f-d0629bfbc0d0");

        public Guid TypeId => Id;

        public Type Opposite => typeof(Air);
    }
}
