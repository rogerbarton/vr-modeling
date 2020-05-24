using System;
using System.Collections.Generic;
using libigl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MeshManager : MonoBehaviour
{
    public static MeshManager get;
    
    [Tooltip("List of all meshes that can be loaded and edited with libigl")]
    public GameObject[] meshPrefabs;
    [Tooltip("Where to place newly loaded meshes and how to scale them. Bottom of bounding box is placed here.")]
    public Transform meshSpawnPoint;
    
    [Tooltip("Parent for UI buttons to load each mesh, usually the 'Content' view of a scroll list.")]
    public Transform uiListItemParent;
    [Tooltip("If null then the first child button found in uiListItemParent will be used.")]
    [SerializeField] private GameObject uiListItemPrefab;
    
    /// <summary>
    /// The mesh currently loaded and being modified
    /// </summary>
    public static LibiglMesh ActiveMesh;

    public static event Action ActiveMeshChanged = delegate {};
    public Material wireframeMaterial;

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
        if(meshPrefabs.Length > 0)
            ActiveMesh = LoadMesh(meshPrefabs[0]);

        // Convention: Use the first child as the prefab
        if (!uiListItemPrefab && uiListItemParent.childCount > 0)
            uiListItemPrefab = uiListItemParent.GetComponentInChildren<Button>(true).gameObject;
        
        // Create listitem foreach 
        foreach (var prefab in meshPrefabs)
        {
            var isValid = CheckPrefabValidity(prefab);
            SetupUi(prefab, isValid);
        }
    }

    /// <summary>
    /// Generates UI for loading the <paramref name="meshPrefab"/>
    /// </summary>
    /// <param name="isValid">Whether the prefab is a valid mesh prefab. See <see cref="CheckPrefabValidity"/></param>
    private void SetupUi(GameObject meshPrefab, bool isValid)
    {
        // Parenting, layout, ui
        var go = Instantiate(uiListItemPrefab, uiListItemParent);
        go.SetActive(true);
        var textField = go.GetComponentInChildren<TMP_Text>();
        textField.text = meshPrefab.name;
            
        // setup callbacks/events
        var button = go.GetComponent<Button>();
        button.onClick.AddListener(() => LoadMesh(meshPrefab));
        button.interactable = isValid;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            LoadMeshById(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            LoadMeshById(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            LoadMeshById(2);
    }

    #region Mesh Creation

    /// <summary>
    /// Instantiate a mesh that can be used with libigl
    /// </summary>
    /// <param name="prefab">Prefab to be created</param>
    /// <param name="setAsActiveMesh">Set this mesh as the active one</param>
    /// <param name="performValidityChecks">Check that the prefab can be properly used with libigl.
    /// Meshes from the <see cref="meshPrefabs"/> list are checked during Start and do not need to be checked again.</param>
    /// <returns>LibiglMesh component on the new instance, null if there was an error</returns>
    public LibiglMesh LoadMesh(GameObject prefab, bool setAsActiveMesh = true, bool performValidityChecks = false)
    {
        if(performValidityChecks && !CheckPrefabValidity(prefab)) return null;

        UnloadActiveMesh();

        // Details of spawnPoint
        
        var go = Instantiate(prefab, transform);
        go.transform.parent = transform;
        
        var lmesh = go.GetComponent<LibiglMesh>();
        if (!lmesh)
        {
            // Debug.LogWarning($"Prefab does not have a {nameof(LibiglMesh)} component, it will be added.");
            lmesh = go.AddComponent<LibiglMesh>();
        }
        
        lmesh.ResetTransformToSpawn();

        if (setAsActiveMesh)
            ActiveMesh = lmesh;

        return lmesh;
    }

    /// <param name="prefabIndex">Index in <see cref="meshPrefabs"/></param>
    public void LoadMeshById(int prefabIndex)
    {
        LoadMesh(meshPrefabs[prefabIndex]);
    }

    /// <summary>
    /// Checks if a prefab can be loaded and modified with libigl. Does not modify the prefab only logs errors.
    /// </summary>
    /// <returns>True if the prefab can be used with libigl</returns>
    private bool CheckPrefabValidity(GameObject prefab)
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
            Debug.LogError($"Prefab {prefab.name} mesh is not read/writeable. You must check 'Read/Write Enabled' in the model import settings. " +
                           " \nLoading this mesh will be disabled.");
            return false;
        }
        
        var meshRenderer = prefab.GetComponent<MeshRenderer>();
        if (!meshRenderer)
        {
            Debug.LogError($"Prefab {prefab.name} does not have a MeshRenderer attached. You will not be able to see your mesh. " +
                           " \nLoading this mesh will be disabled.");
            return false;
        }
        
        return true;
    }

    private void UnloadActiveMesh()
    {
        if (!ActiveMesh) return;
        
        Destroy(ActiveMesh.gameObject);
        ActiveMesh = null;
    }
    #endregion

    public static void SetActiveMesh(LibiglMesh libiglMesh)
    {
        if(ActiveMesh == libiglMesh) return;
        
        ActiveMesh = libiglMesh;
        ActiveMeshChanged();
    }
}
