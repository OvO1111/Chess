using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBoxBaseCtrl : MonoBehaviour
{
    public bool hasPiece;
    [HideInInspector]
    public GameObject steppingObject;
    public Vector2Int position;

    [HideInInspector]
    public Material[] rawMaterial;

    bool set = false;

    // Start is called before the first frame update
    void Start()
    {
        set = true;
        hasPiece = false;
        steppingObject = null;
        rawMaterial = GetComponent<MeshRenderer>().materials;
    }

    public void ClearState()
    {
        if (set)
            GetComponent<MeshRenderer>().materials = rawMaterial;
        hasPiece = false;
        steppingObject = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
