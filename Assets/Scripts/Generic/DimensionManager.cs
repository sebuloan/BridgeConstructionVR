// <DimensionManager.cs>
//
// Copyright SX/EDA3 2024 all rights reserved.
//
// Authors:
//  Nikhitha Sannapuneni

//   [C] Bosch.DigiGear.DimensionManager
// This script will manage dimensions of the component selected by the user

using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bosch.DigiGear
{
    public class DimensionManager : MonoBehaviour
    {
        #region public Variables
        public GameObject linePrefab;
        public GameObject heightPrefab;
        public GameObject lengthPrefab;
        public GameObject widthPrefab;
        public bool isDimensionEnabled;
        public dimension componentDimension;
        #endregion

        #region private Variables
        private GameObject _length;
        private GameObject _height;
        private GameObject _width;
        private LineRenderer _lineRenderer;
        private GameObject _lineObj;
        private Vector3[] _positions = new Vector3[8];
        private Vector3 _lengthMidPoint;
        private Vector3 _widthMidPoint;
        private Vector3 _heighthMidpoint;
        private GameObject _selectedComponent;
        private Vector3 _size;
        #endregion

        /// <summary>
        /// Called when Component is grabbed
        /// </summary>
        /// <param name="selectedObject">This is grabbedObject</param>
        public void EnableDimensions(GameObject selectedObject, dimension dimension)
        {
            if (isDimensionEnabled == false)
            {
                componentDimension = dimension;
                _size = selectedObject.GetComponent<BoxCollider>().size;
                _lineObj = Instantiate(linePrefab);
                _lineRenderer = _lineObj.GetComponent<LineRenderer>();
                _lineRenderer.transform.SetParent(selectedObject.transform);
                _lineRenderer.positionCount = 16;
                _selectedComponent = selectedObject;
                CreateBoxRenderer();
                _length = Instantiate(lengthPrefab);
                _width = Instantiate(widthPrefab);
                _height = Instantiate(heightPrefab);
                UpdatePosition(selectedObject);
                isDimensionEnabled = true;
            }
        }

        /// <summary>
        /// Destroy the instantiated objects line rendered and dimensions when user grab another component
        /// </summary>
        public void DisableDimensions()
        {
            if (isDimensionEnabled)
            {
                DestroyImmediate(_lineObj);
                DestroyImmediate(_length);
                DestroyImmediate(_height);
                DestroyImmediate(_width);
                isDimensionEnabled = false;
            }
        }

        void Update()
        {
            if (isDimensionEnabled)
            {
                CreateBoxRenderer();
            }
        }

        /// <summary>
        /// Updates the positions of the line renderer to outline the box;
        /// </summary>
        void CreateBoxRenderer()
        {
            BoxcolliderVertices();
        }

        /// <summary>
        /// This will get the all the vertices of a boxcollider of selectedComponent
        /// </summary>
        public void BoxcolliderVertices()
        {
            _positions[0] = _selectedComponent.transform.TransformPoint(new Vector3(_size.x / 2.0f, _size.y / 2.0f, _size.z / 2.0f));
            _positions[1] = _selectedComponent.transform.TransformPoint(new Vector3(-_size.x / 2.0f, _size.y / 2.0f, _size.z / 2.0f));
            _positions[2] = _selectedComponent.transform.TransformPoint(new Vector3(-_size.x / 2.0f, -_size.y / 2.0f, _size.z / 2.0f));
            _positions[3] = _selectedComponent.transform.TransformPoint(new Vector3(_size.x / 2.0f, -_size.y / 2.0f, _size.z / 2.0f));

            _positions[4] = _selectedComponent.transform.TransformPoint(new Vector3(_size.x / 2.0f, _size.y / 2.0f, -_size.z / 2.0f));
            _positions[5] = _selectedComponent.transform.TransformPoint(new Vector3(-_size.x / 2.0f, _size.y / 2.0f, -_size.z / 2.0f));
            _positions[6] = _selectedComponent.transform.TransformPoint(new Vector3(-_size.x / 2.0f, -_size.y / 2.0f, -_size.z / 2.0f));
            _positions[7] = _selectedComponent.transform.TransformPoint(new Vector3(_size.x / 2.0f, -_size.y / 2.0f, -_size.z / 2.0f));

            DrawCubeOutline();
        }

        /// <summary>
        /// Draws a cube outline using line renderer based on vertex.
        /// </summary>
        public void DrawCubeOutline()
        {
            _lineRenderer.SetPosition(0, _positions[0]);
            _lineRenderer.SetPosition(1, _positions[1]);
            _lineRenderer.SetPosition(2, _positions[2]);
            _lineRenderer.SetPosition(3, _positions[3]);
            _lineRenderer.SetPosition(4, _positions[0]);

            _lineRenderer.SetPosition(5, _positions[4]);
            _lineRenderer.SetPosition(6, _positions[5]);
            _lineRenderer.SetPosition(7, _positions[1]);
            _lineRenderer.SetPosition(8, _positions[5]);

            _lineRenderer.SetPosition(9, _positions[6]);
            _lineRenderer.SetPosition(10, _positions[2]);
            _lineRenderer.SetPosition(11, _positions[6]);
            _lineRenderer.SetPosition(12, _positions[7]);
            _lineRenderer.SetPosition(13, _positions[3]);
            _lineRenderer.SetPosition(14, _positions[7]);
            _lineRenderer.SetPosition(15, _positions[4]);

        }

        /// <summary>
        /// Based on the midpoints dymanically update length, width and height positions
        /// </summary>
        /// <param name="componentName">Grabbed component</param>
        public void UpdatePosition(GameObject componentName)
        {
            Vector3 _side1Midpoint = (_positions[4] + _positions[5]) / 2;
            Vector3 _side2Midpoint = (_positions[3] + _positions[7]) / 2;
            _heighthMidpoint = (_positions[4] + _positions[7]) / 2;

            //Finding distnace between two points
            float side1 = Vector3.Distance(_positions[4], _positions[5]);
            float side2 = Vector3.Distance(_positions[3], _positions[7]);

            //Finding longer side
            if (side1 >= side2)
            {
                _length.transform.GetChild(0).GetComponent<TMP_Text>().text = "L:"+componentDimension.length;
                _length.transform.position = new Vector3(_side1Midpoint.x, _side1Midpoint.y + 0.01f, _side1Midpoint.z);
                _length.transform.SetParent(componentName.transform);
                _length.transform.localRotation = Quaternion.identity;

                _width.transform.GetChild(0).GetComponent<TMP_Text>().text = "W:"+componentDimension.width;
                _width.transform.position = new Vector3(_side2Midpoint.x, _side2Midpoint.y - 0.04f, _side2Midpoint.z);
                _width.transform.SetParent(componentName.transform);
                _width.transform.localRotation = Quaternion.Euler(0, -90, 0);

            }
            else
            {
                _length.transform.GetChild(0).GetComponent<TMP_Text>().text = "L:"+componentDimension.length;
                _length.transform.position = new Vector3(_side2Midpoint.x, _side2Midpoint.y - 0.04f, _side2Midpoint.z);
                _length.transform.SetParent(componentName.transform);
                _length.transform.localRotation = Quaternion.Euler(0, -90, 0);

                _width.transform.GetChild(0).GetComponent<TMP_Text>().text = "W:"+componentDimension.width;
                _width.transform.position = new Vector3(_side1Midpoint.x, _side1Midpoint.y + 0.01f, _side1Midpoint.z);
                _width.transform.SetParent(componentName.transform);
                _width.transform.localRotation = Quaternion.identity;
            }

            _height.transform.GetChild(0).GetComponent<TMP_Text>().text = "H:"+componentDimension.height;
            _height.transform.position = new Vector3(_heighthMidpoint.x + 0.01f, _heighthMidpoint.y, _heighthMidpoint.z);
            _height.transform.SetParent(componentName.transform);
            _height.transform.localRotation = Quaternion.Euler(0, 0, -90);

        }
    }
}