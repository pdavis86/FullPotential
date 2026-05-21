using FullPotential.Api.Unity.Constants;
using UnityEngine;

namespace FullPotential.Api.Unity
{
    public interface IShaderUtilities
    {
        void ChangeRenderMode(Material standardShaderMaterial, ShaderRenderMode renderMode);
    }
}
