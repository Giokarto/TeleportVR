using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace DisplayExtensions
{
    /// <summary>
    /// Base class for overlays to the video stream from the robot.
    /// Renders the extra information (e.g. names of people) as a second material on the eye plane.
    /// </summary>
    public abstract class FullScreenOverlay : MonoBehaviour
    {
        public Renderer leftRenderer;
        public Renderer rightRenderer;

        private Material overlayMat;

        /// <summary>
        /// Variable to hold a texture from each update.
        /// </summary>
        protected Texture currentTexture;

        /// <summary>
        /// Each overlay adds a material to the renderer. This retrieves its index. The index can get changed
        /// if an earlier overlay is destroyed. We assume the index is the same for left and right plane.
        /// </summary>
        private static Dictionary<FullScreenOverlay, int> overlayToMatIndex = new Dictionary<FullScreenOverlay, int>();

        /// <summary>
        /// Add a new material to the renderer and note its index.
        /// </summary>
        protected void RegisterOverlay()
        {
            overlayMat = new Material(Shader.Find("Sprites/Default"));

            int materialIndex = leftRenderer.materials.Length;
            overlayToMatIndex[this] = materialIndex;
            
            leftRenderer.AddMaterial(overlayMat);
            rightRenderer.AddMaterial(overlayMat);
        }

        /// <summary>
        /// Removes the material and has to update material index of other overlays.
        /// </summary>
        protected void OnDisable()
        {
            int index = overlayToMatIndex[this];
            
            var leftMaterials = leftRenderer.materials;
            var newMaterials = leftMaterials.Where((item, i) => i != index).ToArray();
            leftRenderer.materials = newMaterials;
            
            var rightMaterials = rightRenderer.materials;
            newMaterials = rightMaterials.Where((item, i) => i != index).ToArray();
            rightRenderer.materials = newMaterials;

            foreach (var (overlay, i) in overlayToMatIndex.Select(x => (x.Key, x.Value)))
            {
                if (i > index)
                {
                    overlayToMatIndex[overlay] -= 1;
                }
            }
        }

        protected void Awake()
        {
            RegisterOverlay();
        }

        /// <summary>
        /// Get information to show in overlay and return them as a texture (with transparent background).
        /// </summary>
        public abstract Texture GetOverlayTexture();

        protected void Update()
        {
            currentTexture = GetOverlayTexture();
            // in child classes: assign currentTexture where it belongs to
        }
    }
}