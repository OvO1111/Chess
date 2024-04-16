using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceBaseCtrl : MonoBehaviour
{

    public string gridBoxName;

    public Vector2Int position;
    public bool isSelected;
    [HideInInspector]
    public string pieceName;
    [HideInInspector]
    public int side;
    [HideInInspector]
    public int moves;

    [HideInInspector]
    public Vector3 startPosition;
    public Material[] rawMaterial;

    bool set = false;
    Material MouseSelectMaterial;
    Material MouseHoverMaterial;

    // Start is called before the first frame update
    void Start()
    {
        set = true;
        moves = 0;
        isSelected = false;
        pieceName = name.Split('_')[1];
        side = (name.Split('_')[2][0] == '1') ? 1 : 2;
        rawMaterial = GetComponent<MeshRenderer>().materials;
        startPosition = transform.position;

        var chessboard = GameObject.Find("/chessboard");
        MouseHoverMaterial = chessboard.gameObject.GetComponent<MovePiece>().pieceMouseHoverMaterial;
        MouseSelectMaterial = chessboard.gameObject.GetComponent<MovePiece>().pieceSelectMaterial;
    }

    // Update is called once per frame
    public void ClearState()
    {
        moves = 0;
        isSelected = false;
        gameObject.SetActive(true);
        pieceName = name.Split('_')[1];
        side = (name.Split('_')[2][0] == '1') ? 1 : 2;
        if (set)
        {
            GetComponent<MeshRenderer>().materials = rawMaterial;
            transform.position = startPosition;
        }

        var chessboard = GameObject.Find("/chessboard");
        MouseHoverMaterial = chessboard.gameObject.GetComponent<MovePiece>().pieceMouseHoverMaterial;
        MouseSelectMaterial = chessboard.gameObject.GetComponent<MovePiece>().pieceSelectMaterial;
    }

    private void OnMouseEnter()
    {
        int side = GameObject.Find("/chessboard").GetComponent<MovePiece>().side;
        string MaskName = (side == 1) ? "whitePiece" : "blackPiece";

        if (LayerMask.GetMask(MaskName) == 1 << this.gameObject.layer)
        {
            var materials = GetComponent<MeshRenderer>().materials;
            materials[0] = MouseHoverMaterial;
            GetComponent<MeshRenderer>().materials = materials;
        }

    }

    private void OnMouseExit()
    {
        string MaskName = (side == 1) ? "whitePiece" : "blackPiece";
        if (LayerMask.GetMask(MaskName) == 1 << this.gameObject.layer)
        {
            var materials = GetComponent<MeshRenderer>().materials;
            materials[0] = (isSelected) ? MouseSelectMaterial : rawMaterial[0];
            GetComponent<MeshRenderer>().materials = materials;
        }
            
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("collision detected");
        if (!collision.gameObject.name.Contains("piece"))
        {
            gridBoxName = collision.gameObject.name;
            collision.gameObject.GetComponent<GridBoxBaseCtrl>().hasPiece = true;
            position = collision.gameObject.GetComponent<GridBoxBaseCtrl>().position;
            collision.gameObject.GetComponent<GridBoxBaseCtrl>().steppingObject = this.gameObject;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.name.Contains("piece"))
        {
            gridBoxName = null;
            collision.gameObject.GetComponent<GridBoxBaseCtrl>().hasPiece = false;
            collision.gameObject.GetComponent<GridBoxBaseCtrl>().steppingObject = this.gameObject;
        }
    }
}
