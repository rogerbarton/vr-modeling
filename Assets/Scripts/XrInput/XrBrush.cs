using System.Collections.Generic;
using System.Linq;
using Libigl;
using UnityEngine;

namespace XrInput
{
    public class XrBrush : MonoBehaviour
    {
        public Transform center;
        private SphereCollider _brushCollider;

        // min and max radius
        public static Vector2 RadiusRange = new Vector2(0.025f, 1f);
        public const float ResizeSpeed = 0.5f;

        private bool _isRight;

        private List<LibiglMesh> _currentLibiglMeshes = new List<LibiglMesh>();
        private bool _insideActiveMeshBounds;
        
        public void SetRadius(float value)
        {
            transform.localScale = new Vector3(value, value, value);
        }

        public void Initialize(bool isRight)
        {
            _isRight = isRight;
            OnActiveToolChanged();
            MeshManager.ActiveMeshSet += OnActiveMeshSet;
        }

        public void OnActiveToolChanged()
        {
            switch (InputManager.State.ActiveTool)
            {
                case ToolType.Transform:
                    gameObject.SetActive(_isRight);
                    break;
                case ToolType.Select:
                    gameObject.SetActive(_isRight);
                    break;
            }
        }

        #region Active mesh selection & bounding box visuals

        /// <summary>
        /// Will set the active mesh as the first hovered, if we are not hovering over the active mesh.
        /// Hovering is detected and visualized by the bounding boxes.
        /// </summary>
        /// <returns>True if the active mesh has been set</returns>
        public bool SetActiveMesh()
        {
            if (_insideActiveMeshBounds || _currentLibiglMeshes.Count == 0) return false;

            MeshManager.SetActiveMesh(_currentLibiglMeshes.First());

            return true;
        }

        /// <summary>
        /// Called when the brush bubble enters a trigger collider. Standard Unity callback.
        /// We use this to set the hovering status of the individual meshes.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            var libiglMesh = other.transform.parent.GetComponent<LibiglMesh>();
            if (!libiglMesh) return;

            if (_currentLibiglMeshes.Contains(libiglMesh))
            {
                Debug.LogWarning("XrBrush: Bounding box entered, but we were already inside.");
                return;
            }

            _currentLibiglMeshes.Add(libiglMesh);
            if (libiglMesh == MeshManager.ActiveMesh)
            {
                _insideActiveMeshBounds = true;
                RepaintBoundingBoxes();
            }
            else
                libiglMesh.RepaintBounds(!_insideActiveMeshBounds, _currentLibiglMeshes.Count == 1);
        }

        /// <summary>
        /// Called when the mesh leaves the trigger and updates the hovering status of a mesh.
        /// Implementation note: split into separate function so when deactivating we leave all triggers.
        /// </summary>
        private void MeshLeftTrigger(LibiglMesh libiglMesh)
        {
            _currentLibiglMeshes.Remove(libiglMesh);
            if (libiglMesh == MeshManager.ActiveMesh)
            {
                _insideActiveMeshBounds = false;
                RepaintBoundingBoxes();
            }
            else
            {
                libiglMesh.RepaintBounds(false, false);
                if(_currentLibiglMeshes.Count > 0)
                    _currentLibiglMeshes[0].RepaintBounds(!_insideActiveMeshBounds, true);
            }
            
        }

        private void OnTriggerExit(Collider other)
        {
            var libiglMesh = other.transform.parent.GetComponent<LibiglMesh>();
            if (!libiglMesh) return;

            MeshLeftTrigger(libiglMesh);
        }

        /// <summary>
        /// Repaint bounding boxes based on the hovering status.
        /// Bounds are hidden if we are hovering over the active mesh.
        /// The first hovered mesh is set as the primary one.
        /// </summary>
        private void RepaintBoundingBoxes()
        {
            for (var i = 0; i < _currentLibiglMeshes.Count; i++)
                _currentLibiglMeshes[i].RepaintBounds(!_insideActiveMeshBounds, i == 0);
        }

        private void OnActiveMeshSet()
        {
            _insideActiveMeshBounds = _currentLibiglMeshes.Contains(MeshManager.ActiveMesh);
            RepaintBoundingBoxes();
        }
        
        private void OnDisable()
        {
            while (_currentLibiglMeshes.Count > 0)
                MeshLeftTrigger(_currentLibiglMeshes[0]);
        }

        #endregion
    }
}
