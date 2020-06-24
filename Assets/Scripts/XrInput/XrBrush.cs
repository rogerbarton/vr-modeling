using System;
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

        public bool SetActiveMesh()
        {
            if (_insideActiveMeshBounds || _currentLibiglMeshes.Count == 0) return false;
            
            MeshManager.SetActiveMesh(_currentLibiglMeshes.First());
            
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var libiglMesh = other.transform.parent.GetComponent<LibiglMesh>();
            if (!libiglMesh) return;

            if (!_currentLibiglMeshes.Contains(libiglMesh))
            {
                _currentLibiglMeshes.Add(libiglMesh);
                if (libiglMesh == MeshManager.ActiveMesh)
                    _insideActiveMeshBounds = true;
                else
                    libiglMesh.BoundingBox.GetComponent<MeshRenderer>().enabled = true;
            }
            else
                Debug.LogWarning("Bounding box entered but we were already inside.");
        }

        private void OnTriggerExit(Collider other)
        {
            var libiglMesh = other.transform.parent.GetComponent<LibiglMesh>();
            if (!libiglMesh) return;

            MeshLeftTrigger(libiglMesh);
            _currentLibiglMeshes.Remove(libiglMesh);
        }

        private void MeshLeftTrigger(LibiglMesh libiglMesh)
        {
            if (libiglMesh == MeshManager.ActiveMesh)
                _insideActiveMeshBounds = false;
            else
                libiglMesh.BoundingBox.GetComponent<MeshRenderer>().enabled = false;
        }

        private void OnDisable()
        {
            foreach (var libiglMesh in _currentLibiglMeshes)
                MeshLeftTrigger(libiglMesh);
            _currentLibiglMeshes.Clear();
        }

        private void OnActiveMeshSet()
        {
            _insideActiveMeshBounds = _currentLibiglMeshes.Contains(MeshManager.ActiveMesh);
        }
    }
}
