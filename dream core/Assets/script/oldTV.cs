using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class oldTV : MonoBehaviour
{
        public Shader shader;
        [Range(0, 1)] public float Expand = 0.7f;
        [Range(0, 1)] public float NoiseIntensity = 0.3f;
        public int StripeIntensity = 500;


        protected void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Material material = new Material(shader);
            material.SetFloat("_Expand", Expand);
            material.SetFloat("_NoiseIntensity", NoiseIntensity);
            material.SetInt("_StripeIntensity", StripeIntensity);
            Graphics.Blit(source, destination, material, 0);
        }
    }

