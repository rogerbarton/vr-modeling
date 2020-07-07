using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Libigl
{
    /// <summary>
    /// Handles loading EditableMeshes and stores references, notably to the <see cref="ActiveMesh"/>.
    /// </summary>
    public class MeshManager : MonoBehaviour
    {
        public static MeshManager get;

        [Tooltip("List of all meshes that can be loaded and edited with libigl")]
        public GameObject[] meshPrefabs;

        [Tooltip("Where to place newly loaded meshes and how to scale them. Bottom of bounding box is placed here.")]
        public Transform meshSpawnPoint;

        /// <summary>
        /// The mesh currently loaded and being modified
        /// </summary>
        public static LibiglMesh ActiveMesh;

        /// <summary>
        /// Invoked when the <see cref="ActiveMesh"/> is changed.
        /// Called after initialization, if the mesh is newly instantiated.
        /// </summary>
        public static event Action OnActiveMeshChanged = delegate { };

        /// <summary>
        /// List of all LibiglMeshes that are instantiated
        /// </summary>
        [NonSerialized] public readonly List<LibiglMesh> AllMeshes = new List<LibiglMesh>();

        public Material wireframeMaterial;
        public Material wireframeMaterialPrimary;
        public Material wireframeMaterialActive;
        
        private static int _defaultLayer;
        private static int _holographicLayer;

        public GameObject boundingBoxPrefab;

        private void Awake()
        {
            if (get)
            {
                Debug.LogWarning("MeshManager instance already exists.");
                enabled = false;
                return;
            }

            get = this;
        }

        private void Start()
        {
            _defaultLayer = LayerMask.NameToLayer("Default");
            _holographicLayer = LayerMask.NameToLayer("HolographicHighlight");

            // Instantiate the first mesh, skip invalid ones
            var i = 0;
            while (i < meshPrefabs.Length &&
                   LoadMesh(meshPrefabs[i], false, true, true) == null)
                i++;
        }

        #region Mesh Creation

        /// <summary>
        /// Instantiate a mesh that can be used with libigl
        /// </summary>
        /// <param name="prefab">Prefab to be created, see <see cref="meshPrefabs"/></param>
        /// <param name="unloadActiveMesh">Delete the active mesh if it exists</param>
        /// <param name="setAsActiveMesh">Set this mesh as the active one</param>
        /// <param name="performValidityChecks">Check that the prefab can be properly used with libigl.</param>
        /// <remarks>Meshes from the <see cref="meshPrefabs"/> list are checked during Start and do not need to be checked again.</remarks>
        /// <returns>LibiglMesh component on the new instance, null if there was an error</returns>
        public LibiglMesh LoadMesh(GameObject prefab, bool unloadActiveMesh = true, bool setAsActiveMesh = true,
            bool performValidityChecks = false)
        {
            if (performValidityChecks && !CheckPrefabValidity(prefab)) return null;

            if (unloadActiveMesh)
                DestroyActiveMesh();

            var go = Instantiate(prefab, transform);
            go.transform.parent = transform;

            var libiglMesh = go.GetComponent<LibiglMesh>();
            if (!libiglMesh)
                libiglMesh = go.AddComponent<LibiglMesh>();

            libiglMesh.Initialize();

            if (setAsActiveMesh)
                SetActiveMesh(libiglMesh);

            return libiglMesh;
        }

        /// <summary>
        /// Checks if a prefab can be loaded and modified with libigl. Does not modify the prefab only logs errors.
        /// </summary>
        /// <returns>True if the prefab can be used with libigl</returns>
        public static bool CheckPrefabValidity(GameObject prefab)
        {
            var meshFilter = prefab.GetComponent<MeshFilter>();
            if (!meshFilter)
            {
                Debug.LogError($"Prefab {prefab.name} does not have a MeshFilter attached. " +
                               " \nLoading this mesh will be disabled.");
                return false;
            }

            var mesh = meshFilter.sharedMesh;
            if (!mesh)
            {
                Debug.LogError($"Prefab {prefab.name} does not have a Mesh attached. " +
                               " \nLoading this mesh will be disabled.");
                return false;
            }


            if (!mesh.isReadable)
            {
                Debug.LogError(
                    $"Prefab {prefab.name} mesh is not read/writeable. You must check 'Read/Write Enabled' in the model import settings. " +
                    " \nLoading this mesh will be disabled.");
                return false;
            }

            var meshRenderer = prefab.GetComponent<MeshRenderer>();
            if (!meshRenderer)
            {
                Debug.LogError(
                    $"Prefab {prefab.name} does not have a MeshRenderer attached. You will not be able to see your mesh. " +
                    " \nLoading this mesh will be disabled. The mesh renderer must be on a child (e.g. for .obj files).");
                return false;
            }

            return true;
        }

        private static void DestroyActiveMesh()
        {
            if (!ActiveMesh) return;

            Destroy(ActiveMesh.gameObject);
            ActiveMesh = null;
        }

        #endregion

        public static void SetActiveMesh(LibiglMesh libiglMesh)
        {
            if (ActiveMesh == libiglMesh) return;

            if (ActiveMesh) ActiveMesh.gameObject.layer = _defaultLayer;
            libiglMesh.gameObject.layer = _holographicLayer;

            ActiveMesh = libiglMesh;
            OnActiveMeshChanged();
        }

        /// <summary>
        /// Use this to delete a mesh safely.
        /// Handles case when mesh is the active one, <see cref="ActiveMesh"/>
        /// </summary>
        public void DestroyMesh(LibiglMesh libiglMesh)
        {
            if (ActiveMesh == libiglMesh)
            {
                // Assign new active mesh
                foreach (var mesh in AllMeshes.Where(mesh => mesh != ActiveMesh))
                {
                    SetActiveMesh(mesh);
                    break;
                }
            }

            if (ActiveMesh == libiglMesh)
                LoadMesh(meshPrefabs[0]);

            Destroy(libiglMesh.gameObject);
        }

        public void RegisterMesh(LibiglMesh libiglMesh)
        {
            AllMeshes.Add(libiglMesh);
        }

        public void UnregisterMesh(LibiglMesh libiglMesh)
        {
            if (AllMeshes.Contains(libiglMesh))
                AllMeshes.Remove(libiglMesh);
        }
    }
}
