using FullPotential.Api.Unity;
using FullPotential.Api.Unity.Constants;

using UnityEngine;
using UnityEngine.Rendering;

namespace FullPotential.Core.Unity
{
    public class ShaderUtilities : IShaderUtilities
    {
        public void ChangeRenderMode(
            Material material,
            ShaderRenderMode renderMode)
        {
            switch (renderMode)
            {
                case ShaderRenderMode.Opaque:
                    material.SetFloat("_Surface", 0);
                    material.SetFloat("_AlphaClip", 0);

                    material.SetInt("_SrcBlend", (int)BlendMode.One);
                    material.SetInt("_DstBlend", (int)BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);

                    material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.DisableKeyword("_ALPHATEST_ON");

                    material.renderQueue = -1;
                    break;

                case ShaderRenderMode.Cutout:
                    material.SetFloat("_Surface", 0);
                    material.SetFloat("_AlphaClip", 1);

                    material.SetInt("_SrcBlend", (int)BlendMode.One);
                    material.SetInt("_DstBlend", (int)BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);

                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");

                    material.renderQueue = (int)RenderQueue.AlphaTest;
                    break;

                case ShaderRenderMode.Fade:
                    material.SetFloat("_Surface", 1);
                    material.SetFloat("_Blend", 0);

                    material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);

                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.DisableKeyword("_ALPHATEST_ON");

                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;

                case ShaderRenderMode.Transparent:
                    material.SetFloat("_Surface", 1);
                    material.SetFloat("_Blend", 1);

                    material.SetInt("_SrcBlend", (int)BlendMode.One);
                    material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);

                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.DisableKeyword("_ALPHATEST_ON");

                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;
            }
        }
    }
}