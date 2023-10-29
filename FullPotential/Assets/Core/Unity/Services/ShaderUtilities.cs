using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Services;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable StringLiteralTypo

namespace FullPotential.Core.Unity.Services
{
    public class ShaderUtilities : IShaderUtilities
    {
        public void ChangeRenderMode(Material standardShaderMaterial, ShaderRenderMode renderMode)
        {
            switch (renderMode)
            {
                case ShaderRenderMode.Opaque:
                    standardShaderMaterial.SetFloat("_Mode", 0);
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    standardShaderMaterial.SetInt("_ZWrite", 1);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = -1;
                    break;

                case ShaderRenderMode.Cutout:
                    standardShaderMaterial.SetFloat("_Mode", 1);
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    standardShaderMaterial.SetInt("_ZWrite", 1);
                    standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 2450;
                    break;

                case ShaderRenderMode.Fade:
                    standardShaderMaterial.SetFloat("_Mode", 2);
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    standardShaderMaterial.SetInt("_ZWrite", 0);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 3000;
                    break;

                case ShaderRenderMode.Transparent:
                    standardShaderMaterial.SetFloat("_Mode", 3);
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    standardShaderMaterial.SetInt("_ZWrite", 0);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 3000;
                    break;
            }
        }

        //public static Dictionary<string, ShaderPropertyType> GetShaderProperties(Shader shader)
        //{
        //    var props = new Dictionary<string, ShaderPropertyType>();
        //    for (var i = 0; i < shader.GetPropertyCount(); i++)
        //    {
        //        props.Add(
        //            shader.GetPropertyName(i),
        //            shader.GetPropertyType(i));
        //    }

        //    return props;
        //}
    }
}