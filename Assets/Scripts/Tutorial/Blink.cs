//
// Blink.cs
//
// Copyright RBEI/EVC 2020 all rights reserved.
//
// Authors:
//  	Karthik, Sridhara, Suraj M K, Sagar T Y
//
// Defines:
//  	[C]  <Bosch.Evc.Sequence>.Blink
//
// This script handles Blinking of the Object

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Bosch.Evc.Sequence
{ 
    public class Blink : MonoBehaviour
    {
        public bool isBlink;
        public bool isUpdateChildRenderers = false;
        public Color highlightedColor;

        private Renderer _objectRenderer;
        private Color _objectRendererColor;
        private bool _isBlinking = false;
        private Renderer[] _meshRenderers;
        private List<Color> _meshColors = new List<Color>();

        public Color setColor
        {
            set
            {
                highlightedColor = value;
            }
        }

        void Start()
        {
           // isUpdateChildRenderers = true;

            _objectRenderer = GetComponent<Renderer>();

            if (_objectRenderer == null)
            {
                _objectRenderer = GetComponentInChildren<Renderer>();
            }

            if (isUpdateChildRenderers)
            {
                _meshRenderers = GetComponentsInChildren<Renderer>();
                foreach (var meshRenderer in _meshRenderers)
                {
                    foreach (var material in meshRenderer.materials)
                    {
                        if (!material.shader.name.Contains("Outline"))
                        {
                            _meshColors.Add(material.color);
                        }
                    }
                }
                SetOutLine(0);
            }

            _objectRendererColor = _objectRenderer.material.color;
        }

        void Update()
        {
            if (isBlink && !_isBlinking)
            {
                if (isUpdateChildRenderers)
                {
                    SetOutLine(0.001f);
                }

                _isBlinking = true;
                StartCoroutine("ToggleMatirial");
            }
        }

        /// <summary>
        /// Changes the color from the default color to highlight color & vice versa
        /// </summary>
        /// <returns></returns>
        public IEnumerator ToggleMatirial()
        {
            if (_objectRenderer.material.color == highlightedColor)
            {
                _objectRenderer.material.color = _objectRendererColor;
                if (isUpdateChildRenderers)
                {
                    RevertChildMaterials();
                }
            }
            else
            {
                _objectRenderer.material.color = highlightedColor;
                if (isUpdateChildRenderers)
                {
                    UpdateChildMaterials(highlightedColor);
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (isBlink)
            {
                StartCoroutine("ToggleMatirial");
            }
            else
            {
                _objectRenderer.material.color = _objectRendererColor;
                if (isUpdateChildRenderers)
                {
                    SetOutLine(0f);
                    RevertChildMaterials();
                }

                _isBlinking = false;
            }
        }

        private void RevertChildMaterials()
        {
            int i = 0;
            foreach (var SkinnedMeshRenderer in _meshRenderers)
            {
                foreach (var material in SkinnedMeshRenderer.materials)
                {
                    if (!material.shader.name.Contains("Outline"))
                    {
                        material.color = _meshColors[i];
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// updates the child Materials
        /// </summary>
        /// <param name="_color"></param>
        public void UpdateChildMaterials(Color _color)
        {
            foreach (Renderer mr in _meshRenderers)
            {
                foreach (var material in mr.materials)
                {
                    if (!material.shader.name.Contains("Outline"))
                    {
                        material.color = _color;
                    }
                }
            }
        }

        /// <summary>
        /// Set the Shader outline value 
        /// </summary>
        /// <param name="value"></param>
        public void SetOutLine(float value)
        {
            foreach (Renderer mr in _meshRenderers)
            {
                mr.material.SetFloat("_Outline", value);
            }
        }

        /// <summary>
        /// StartBlinking the component
        /// </summary>
        public void StartBlinking()
        {
            isBlink = true;
        }

        /// <summary>
        /// StopBlinking the component
        /// </summary>
        public void StopBlinking()
        {
            isBlink = false;
        }
    }
}