﻿using System;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Utilities;
using FullPotential.Standard.SpellsAndGadgets.Behaviours;
using UnityEngine;

namespace FullPotential.Standard.SpellsAndGadgets.Shapes
{
    public class Wall : IShape
    {
        public Guid TypeId => new Guid("4aad2866-5903-4b79-bda2-e3dcab920d9e");

        public string TypeName => nameof(Wall);

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Wall.prefab";

        public void SpawnGameObject(SpellOrGadgetItemBase spellOrGadget, IFighter sourceFighter, Vector3 startPosition, Quaternion rotation)
        {
            var gameManager = DependenciesContext.Dependencies.GetService<IModHelper>().GetGameManager();
            DependenciesContext.Dependencies.GetService<ITypeRegistry>().LoadAddessable(
                spellOrGadget.Shape.PrefabAddress,
                prefab =>
                {
                    var spellObject = UnityEngine.Object.Instantiate(prefab, startPosition, rotation);
                    gameManager.GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(startPosition, spellObject.transform);

                    var spellScript = spellObject.GetComponent<SogWallBehaviour>();
                    spellScript.SpellOrGadget = spellOrGadget;
                    spellScript.SourceFighter = sourceFighter;

                    spellObject.transform.parent = gameManager.GetSceneBehaviour().GetTransform();
                }
            );
        }

    }
}
