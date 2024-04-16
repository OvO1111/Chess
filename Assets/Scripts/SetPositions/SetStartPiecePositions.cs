using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetStartPiecePositions : MonoBehaviour
{
    // this.gameObject == Camera.main
    // Start is called before the first frame update

    public List<GameObject> whitePiecePrefabs;  // pawn, rook, knight, bishop, queen, king
    public List<GameObject> blackPiecePrefabs;
    public GameObject chessboard;

    Dictionary<string, GameObject> allPieces = new();

    public void InstantiatePiece(GameObject g, string name, Vector2Int position)
    {
        var piece = Instantiate(g);
        piece.name = name;
        if (!name.Contains("bishop"))
        {
            var collider = piece.AddComponent<MeshCollider>();
            collider.convex = true;
        }
        else
        {
            var collider = piece.AddComponent<CapsuleCollider>();
            collider.radius = 0.5f;
            collider.height = 3.35f;
            collider.center = new Vector3(0, 0, 1.5f);
            collider.direction = 2;
        }
        piece.AddComponent<Rigidbody>();
        var script = piece.AddComponent<PieceBaseCtrl>();
        script.position = new Vector2Int(position.y, position.x);
        script.startPosition = new Vector3((position.x - 3.5f) * 10, 0, (position.y - 3.5f) * 10);
        script.ClearState();
        piece.layer = (script.side == 1) ? LayerMask.NameToLayer("whitePiece") : LayerMask.NameToLayer("blackPiece");
        piece.gameObject.transform.position = new Vector3((position.x - 3.5f) * 10, 0, (position.y - 3.5f) * 10);

        if (name.Contains("knight")) { piece.transform.rotation = (script.side == 1) ? Quaternion.Euler(new Vector3(-90, 90, 90)) : Quaternion.Euler(new Vector3(90, 90, 90)); }
        if (name.Contains("king")) { piece.transform.rotation = Quaternion.Euler(new Vector3(-90, 90, 0)); }

        allPieces[script.pieceName] = piece;
    }

    public void InstantiateAll()
    {

        for (int i = 0; i < 8; i++)
        {
            InstantiatePiece(whitePiecePrefabs[0], string.Format("piece_pawn_1_{0}", i), new Vector2Int(1, i));
            InstantiatePiece(blackPiecePrefabs[0], string.Format("piece_pawn_2_{0}", i), new Vector2Int(6, i));
        }
        for (int i = 0; i < 8; i++)
        {
            switch (i)
            {
                case 0:
                case 7:
                    {
                        InstantiatePiece(whitePiecePrefabs[1], string.Format("piece_rook_1_{0}", i), new Vector2Int(0, i));
                        InstantiatePiece(blackPiecePrefabs[1], string.Format("piece_rook_2_{0}", i), new Vector2Int(7, i)); break;
                    }
                case 1:
                case 6:
                    {
                        InstantiatePiece(whitePiecePrefabs[2], string.Format("piece_knight_1_{0}", i), new Vector2Int(0, i));
                        InstantiatePiece(blackPiecePrefabs[2], string.Format("piece_knight_2_{0}", i), new Vector2Int(7, i)); break;
                    }
                case 2:
                case 5:
                    {
                        InstantiatePiece(whitePiecePrefabs[3], string.Format("piece_bishop_1_{0}", i), new Vector2Int(0, i));
                        InstantiatePiece(blackPiecePrefabs[3], string.Format("piece_bishop_2_{0}", i), new Vector2Int(7, i)); break;
                    }
                case 3:
                    {
                        InstantiatePiece(whitePiecePrefabs[4], string.Format("piece_king_1_{0}", i), new Vector2Int(0, i));
                        InstantiatePiece(blackPiecePrefabs[4], string.Format("piece_king_2_{0}", i), new Vector2Int(7, i)); break;
                    }
                case 4:
                    {
                        InstantiatePiece(whitePiecePrefabs[5], string.Format("piece_queen_1_{0}", i), new Vector2Int(0, i));
                        InstantiatePiece(blackPiecePrefabs[5], string.Format("piece_queen_2_{0}", i), new Vector2Int(7, i)); break;
                    }
            }
        }
    }


    public void ResetAllPositions()
    {
        chessboard.transform.position = new Vector3(0, -2, 0);
        foreach (var g in allPieces.Values)
        {
            var script = g.GetComponent<PieceBaseCtrl>();
            script.ClearState();
        }
    }

    private void Awake()
    {
        InstantiateAll();
        ResetAllPositions();
    }

    void Start()
    {
        Application.targetFrameRate = 300;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
