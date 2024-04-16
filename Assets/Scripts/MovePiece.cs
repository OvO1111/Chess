using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePiece : MonoBehaviour
{
    Vector3 moveStep = new Vector3(10f, 0f, 10f);

    // this.gameObject == chessboard
    public Material groundSelectMaterial;
    public Material groundCaptureMaterial;
    public Material groundCandidateMaterial;
    public Material pieceSelectMaterial;
    public Material pieceMouseHoverMaterial;

    [HideInInspector]
    public int viewSide;
    [HideInInspector]
    public int side;
    public int isCheckmate;

    // king castling
    bool KingCastlingChoice;
    List<Vector2Int> KingCastling;

    // pawn En Passant
    GameObject gridBoxEP;
    Vector2Int PawnEnPassant;
    bool PawnEnPassantChoice;
    List<List<GameObject>> childrenGridBoxes = new();

    [HideInInspector]
    public bool ispieceSelected;
    [HideInInspector]
    public List<GameObject> captureList;

    string MaskName;
    GameObject focusPiece;
    GameObject moveToGrid;
    GameGUICtrl GameLogic;
    int retrieveRayCastObjectIndex;
    Material groundDeselectMaterial;
    List<GameObject> movableGridBoxes;

    public void ClearState()
    {
        foreach (var gs in childrenGridBoxes)
        {
            foreach (var g in gs) { g.GetComponent<GridBoxBaseCtrl>().ClearState(); }
        }
        if (captureList.Count > 0) captureList = new();

        side = 1;
        viewSide = 1;
        isCheckmate = -1;
        KingCastlingChoice = false;
        KingCastling = new(2) { new Vector2Int(-1, -1), new Vector2Int(-1, -1) };

        gridBoxEP = null;
        PawnEnPassantChoice = false;
        PawnEnPassant = new Vector2Int(-1, -1);

        ispieceSelected = false;
        captureList = new List<GameObject>();

        focusPiece = null;
        moveToGrid = null;
        MaskName = "whitePiece";
        groundDeselectMaterial = null;
        retrieveRayCastObjectIndex = 0;
        GameLogic = Camera.main.GetComponent<GameGUICtrl>();
        movableGridBoxes = new List<GameObject>();
    }


    // Start is called before the first frame update
    void Awake()
    {
        for (int j=0; j < 8; j++)
        {
            string prefix, gname;
            List<GameObject> chessboardRow = new List<GameObject>();
            for (int i=0; i < 8; i++)
            {
                prefix = (i + j) % 2 == 1 ? "Black" : "White";
                gname = prefix + string.Format("GridBox.{0:D3}", (i * 8 + j) / 2);
                GameObject gridbox = transform.Find(gname).gameObject;
                gridbox.name = string.Format("{0}{1}", j, i);
                gridbox.GetComponent<GridBoxBaseCtrl>().position = new Vector2Int(j, i);

                chessboardRow.Add(gridbox);
            }
            childrenGridBoxes.Add(chessboardRow);
        }
        ClearState();
    }

    // Update is called once per frame
    void Update()
    {
        var mousePosition = Input.mousePosition;
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit[] hitAll;
            hitAll = Physics.RaycastAll(ray, 1000, LayerMask.GetMask(MaskName));
            bool isRayOnFocus = (hitAll.Length > 0);

            if (ispieceSelected)
            {
                if (!isRayOnFocus)
                {
                    if (moveToGrid != null)
                    {
                        CheckPieceCaptureLogic();
                        var translation = moveToGrid.GetComponent<GridBoxBaseCtrl>().position;
                        StartCoroutine(Move(translation - focusPiece.GetComponent<PieceBaseCtrl>().position));
                    }
                }
            }

            if (isRayOnFocus)
            {
                hit = hitAll[(retrieveRayCastObjectIndex++) % hitAll.Length];
                if (focusPiece != null && focusPiece.name != hit.collider.gameObject.name)
                {
                    retrieveRayCastObjectIndex = 0;
                    focusPiece.GetComponent<MeshRenderer>().materials = focusPiece.GetComponent<PieceBaseCtrl>().rawMaterial;
                    focusPiece.GetComponent<PieceBaseCtrl>().isSelected = false;
                }
                else if (focusPiece != null && focusPiece.name == hit.collider.gameObject.name)
                {
                    if (hitAll.Length > 1)
                    {
                        focusPiece.GetComponent<MeshRenderer>().materials = focusPiece.GetComponent<PieceBaseCtrl>().rawMaterial;
                        focusPiece.GetComponent<PieceBaseCtrl>().isSelected = false;
                    }
                }

                ispieceSelected = true;
                focusPiece = hit.collider.gameObject;

                ResetMaterial();
                moveToGrid = null;
                CheckPieceMoveLogic();
                var materials = focusPiece.GetComponent<MeshRenderer>().materials;
                materials[0] = pieceSelectMaterial;
                focusPiece.GetComponent<MeshRenderer>().materials = materials;
                focusPiece.GetComponent<PieceBaseCtrl>().isSelected = true;
            }
            
        }

        if (ispieceSelected)
        {
            if (Input.GetMouseButtonDown(1))
            {
                ResetAfterMove();
            }

            if (movableGridBoxes.Count > 0)
            {
                int ibox = -1;
                float j = float.MaxValue;
                for (int i=0; i < movableGridBoxes.Count; i ++)
                {
                    var j_ = Vector3.Distance(Camera.main.WorldToScreenPoint(movableGridBoxes[i].transform.position),
                        Input.mousePosition);
                    if (j_ < j)
                    {
                        ibox = i;
                        j = j_;
                    }

                }

                if (ibox == -1) { ResetAfterMove(true); }
                else
                {
                    if (moveToGrid != null && moveToGrid.name != movableGridBoxes[ibox].name)
                    {
                        var materials_ = moveToGrid.GetComponent<MeshRenderer>().materials;
                        materials_[0] = groundDeselectMaterial;
                        moveToGrid.GetComponent<MeshRenderer>().materials = materials_;
                        if (PawnEnPassantChoice && ibox != 1)
                        {
                            gridBoxEP.GetComponent<MeshRenderer>().materials = gridBoxEP.GetComponent<GridBoxBaseCtrl>().rawMaterial;
                        }
                    }
                    if (moveToGrid == null || moveToGrid.name != movableGridBoxes[ibox].name)
                    {
                        moveToGrid = movableGridBoxes[ibox];
                        var materials = moveToGrid.GetComponent<MeshRenderer>().materials;
                        groundDeselectMaterial = materials[0];
                        materials[0] = groundSelectMaterial;
                        moveToGrid.GetComponent<MeshRenderer>().materials = materials;
                        if (PawnEnPassantChoice && ibox == 1)
                        {
                            materials = gridBoxEP.GetComponent<MeshRenderer>().materials;
                            materials[0] = groundCaptureMaterial;
                            gridBoxEP.GetComponent<MeshRenderer>().materials = materials;
                        }
                    }
                }
            }
        }
    }

    void ResetAfterMove(bool preserveFocus=false)
    {
        ResetMaterial(preserveFocus);
        ispieceSelected = false;
        moveToGrid = null;
        movableGridBoxes = new List<GameObject>();
        if (!preserveFocus)
        {
            focusPiece.GetComponent<PieceBaseCtrl>().isSelected = false;
            focusPiece = null;
        }
    }

    void ResetMaterial(bool preserveFocus = false)
    {
        foreach (GameObject g in movableGridBoxes)
        {
            g.GetComponent<MeshRenderer>().materials = g.GetComponent<GridBoxBaseCtrl>().rawMaterial;
        }
        groundDeselectMaterial = null;
        movableGridBoxes = new List<GameObject>();
        if (!preserveFocus) focusPiece.GetComponent<MeshRenderer>().materials = focusPiece.GetComponent<PieceBaseCtrl>().rawMaterial;
    }

    int ShineGround(Vector2 coord, int PawnAttackFlag = 0, int KingCastlingFlag = 0)
    {
        // print(focusPiece.GetComponent<PieceBaseCtrl>().pieceName);
        int x = (int)coord.x, y = (int)coord.y;

        if (x < 0 || x > 7 || y < 0 || y > 7)
            return 0;

        var focusPiecePCBScript = focusPiece.GetComponent<PieceBaseCtrl>();
        // if (isCheckmate == side) { if (focusPiecePCBScript.name != "king") return 0; }

        GameObject gridBoxUnderPiece = childrenGridBoxes[x][y];
        var materials = gridBoxUnderPiece.GetComponent<MeshRenderer>().materials;

        if (gridBoxUnderPiece.GetComponent<GridBoxBaseCtrl>().hasPiece)
        {
            var steppingPiece = gridBoxUnderPiece.GetComponent<GridBoxBaseCtrl>().steppingObject;
            var typeofPiece = focusPiecePCBScript.pieceName;
            var steppingPiecePCBScript = steppingPiece.GetComponent<PieceBaseCtrl>();

            if ((steppingPiecePCBScript.side != side && typeofPiece != "pawn") ||
                (steppingPiecePCBScript.side != side && PawnAttackFlag > 0))
            {
                if (steppingPiecePCBScript.pieceName == "king")
                {
                    print("checkmate");
                    isCheckmate = (side == steppingPiecePCBScript.side) ? -1: steppingPiecePCBScript.side;
                    if (isCheckmate != -1)
                    {
                        materials[0] = pieceSelectMaterial;
                        gridBoxUnderPiece.GetComponent<MeshRenderer>().materials = materials;
                        movableGridBoxes.Add(gridBoxUnderPiece);
                    }
                    return 1;
                }

                materials[0] = groundCaptureMaterial;
                gridBoxUnderPiece.GetComponent<MeshRenderer>().materials = materials;
                movableGridBoxes.Add(gridBoxUnderPiece);
                return 1;
            }
        } else if (PawnAttackFlag == 0)
        {
            if (focusPiecePCBScript.pieceName == "king" && KingCastlingFlag > 0)
            {
                print("castling choice");
                KingCastlingChoice = true;
                KingCastling[KingCastlingFlag - 1] = gridBoxUnderPiece.GetComponent<GridBoxBaseCtrl>().position;
                materials[0] = groundCandidateMaterial;
                gridBoxUnderPiece.GetComponent<MeshRenderer>().materials = materials;
                movableGridBoxes.Add(gridBoxUnderPiece);
                return 2;
            }

            materials[0] = groundCandidateMaterial;
            gridBoxUnderPiece.GetComponent<MeshRenderer>().materials = materials;
            movableGridBoxes.Add(gridBoxUnderPiece);
            return 2;
        }

        if (PawnAttackFlag == 2)
        {
            // en passant
            PawnEnPassantChoice = true;
            materials[0] = groundCandidateMaterial;
            gridBoxUnderPiece.GetComponent<MeshRenderer>().materials = materials;
            gridBoxEP = childrenGridBoxes[x][y + ((side == 1) ? -1 : 1)];
            
            movableGridBoxes.Add(gridBoxUnderPiece);
            return 1;
        }
        
        return 0;
    }

    void CheckCastling()
    {
        if (focusPiece.GetComponent<PieceBaseCtrl>().moves == 0)
        {
            int y = (side == 1) ? 0 : 7;

            var rook1 = childrenGridBoxes[0][y].GetComponent<GridBoxBaseCtrl>().steppingObject;
            if (rook1 && rook1.GetComponent<PieceBaseCtrl>().moves == 0)
            {
                if (!childrenGridBoxes[1][y].GetComponent<GridBoxBaseCtrl>().hasPiece &&
                    !childrenGridBoxes[2][y].GetComponent<GridBoxBaseCtrl>().hasPiece)
                { ShineGround(new Vector2(1, y), KingCastlingFlag: 1); }
            }

            var rook2 = childrenGridBoxes[7][y].GetComponent<GridBoxBaseCtrl>().steppingObject;
            if (rook2 && rook1.GetComponent<PieceBaseCtrl>().moves == 0)
            {
                if (!childrenGridBoxes[4][y].GetComponent<GridBoxBaseCtrl>().hasPiece &&
                    !childrenGridBoxes[5][y].GetComponent<GridBoxBaseCtrl>().hasPiece &&
                    !childrenGridBoxes[6][y].GetComponent<GridBoxBaseCtrl>().hasPiece)
                { ShineGround(new Vector2(5, y), KingCastlingFlag: 1); }
            }
        }
    }

    void CheckPromotion()
    {
        int y = (side == 1) ? 6 : 1;
        if (focusPiece.GetComponent<PieceBaseCtrl>().position.y == y)
        {
            StartCoroutine(Promotion());
        }
    }

    IEnumerator Promotion()
    {
        GameObject promotedPiece = null;
        var focusPiecePBCScript = focusPiece.GetComponent<PieceBaseCtrl>();
        print("promotion");
        focusPiece.SetActive(false);

        while (promotedPiece == null)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                promotedPiece = Instantiate(GameObject.Find(string.Format("piece_bishop_{0}", side))) as GameObject;
                promotedPiece.name = string.Format("piece_bishop_{0}_promoted", side);
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                promotedPiece = Instantiate(GameObject.Find(string.Format("piece_queen_{0}", side))) as GameObject;
                promotedPiece.name = string.Format("piece_queen_{0}_promoted", side);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                promotedPiece = Instantiate(GameObject.Find(string.Format("piece_rook_{0}", side))) as GameObject;
                promotedPiece.name = string.Format("piece_bishop_{0}_promoted", side);
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                promotedPiece = Instantiate(GameObject.Find(string.Format("piece_knight_{0}", side))) as GameObject;
                promotedPiece.name = string.Format("piece_knight_{0}_promoted", side);
            }
            yield return null;
        }
        promotedPiece.transform.position = focusPiece.transform.position;
        PieceBaseCtrl promotedPiecePBCScript;
        if (!promotedPiece.TryGetComponent<PieceBaseCtrl>(out promotedPiecePBCScript)) { print(1);  promotedPiece.AddComponent<PieceBaseCtrl>(); }

        promotedPiecePBCScript = promotedPiece.GetComponent<PieceBaseCtrl>();
        promotedPiecePBCScript.position = focusPiecePBCScript.position;
        promotedPiecePBCScript.side = focusPiecePBCScript.side;
        promotedPiecePBCScript.pieceName = promotedPiece.name.Split('_')[1];
        promotedPiecePBCScript.rawMaterial = focusPiecePBCScript.rawMaterial;

        yield return new WaitForSeconds(1f);
    }

    void CheckPieceMoveLogic()
    {
        Vector2Int piecePosition = focusPiece.GetComponent<PieceBaseCtrl>().position;
        int x = piecePosition.x, y = piecePosition.y;

        switch (focusPiece.GetComponent<PieceBaseCtrl>().pieceName)
        {
            case "pawn":
                {
                    int flag;
                    int sign = (side == 1) ? 1 : -1;
                    flag = ShineGround(piecePosition + new Vector2Int(0, sign));
                    if (flag == 2 && focusPiece.GetComponent<PieceBaseCtrl>().moves == 0)
                        ShineGround(piecePosition + new Vector2Int(0, 2 * sign));

                    if (y == PawnEnPassant.y)
                    {
                        if (x == PawnEnPassant.x - 1) ShineGround(piecePosition + new Vector2Int(1, sign), PawnAttackFlag: 2);
                        else if (x == PawnEnPassant.x + 1) ShineGround(piecePosition + new Vector2Int(-1, sign), PawnAttackFlag: 2);
                        print("ep");
                    }
                    ShineGround(piecePosition + new Vector2Int(1, sign), PawnAttackFlag:1);
                    ShineGround(piecePosition + new Vector2Int(-1, sign), PawnAttackFlag:1);
                    break;
                }
            case "knight":
                {
                    ShineGround(piecePosition + new Vector2Int(1, 2));
                    ShineGround(piecePosition + new Vector2Int(-1, 2));
                    ShineGround(piecePosition + new Vector2Int(1, -2));
                    ShineGround(piecePosition + new Vector2Int(-1, -2));
                    ShineGround(piecePosition + new Vector2Int(2, 1));
                    ShineGround(piecePosition + new Vector2Int(-2, 1));
                    ShineGround(piecePosition + new Vector2Int(2, -1));
                    ShineGround(piecePosition + new Vector2Int(-2, -1));
                    break;
                }
            case "rook":
                {
                    List<int> flags = new(4) { 2, 2, 2, 2 };
                    for (int i = 1; i <= 7; i++)
                    {
                        if (flags[0] == 2) { flags[0] = ShineGround(new Vector2Int(x + i, y)); }
                        if (flags[1] == 2) { flags[1] = ShineGround(new Vector2Int(x - i, y)); }
                        if (flags[2] == 2) { flags[2] = ShineGround(new Vector2Int(x, y + i)); }
                        if (flags[3] == 2) { flags[3] = ShineGround(new Vector2Int(x, y - i)); }
                    }
                    break;
                }
            case "bishop":
                {
                    List<int> flags = new(4) { 2, 2, 2, 2 };
                    for (int i = 1; i <= 7; i++)
                    {
                        if (flags[0] == 2) { flags[0] = ShineGround(new Vector2Int(x + i, y + i)); }
                        if (flags[1] == 2) { flags[1] = ShineGround(new Vector2Int(x - i, y + i)); }
                        if (flags[2] == 2) { flags[2] = ShineGround(new Vector2Int(x + i, y - i)); }
                        if (flags[3] == 2) { flags[3] = ShineGround(new Vector2Int(x - i, y - i)); }
                    }
                    break;
                }
            case "queen":
                {
                    List<int> flags = new(8) { 2, 2, 2, 2, 2, 2, 2, 2 };
                    for (int i = 1; i <= 7; i++)
                    {
                        if (flags[0] == 2) { flags[0] = ShineGround(new Vector2Int(x + i, y)); }
                        if (flags[1] == 2) { flags[1] = ShineGround(new Vector2Int(x - i, y)); }
                        if (flags[2] == 2) { flags[2] = ShineGround(new Vector2Int(x, y + i)); }
                        if (flags[3] == 2) { flags[3] = ShineGround(new Vector2Int(x, y - i)); }
                        if (flags[4] == 2) { flags[4] = ShineGround(new Vector2Int(x + i, y + i)); }
                        if (flags[5] == 2) { flags[5] = ShineGround(new Vector2Int(x - i, y + i)); }
                        if (flags[6] == 2) { flags[6] = ShineGround(new Vector2Int(x + i, y - i)); }
                        if (flags[7] == 2) { flags[7] = ShineGround(new Vector2Int(x - i, y - i)); }
                    }
                    break;
                }
            case "king":
                {
                    ShineGround(piecePosition + new Vector2Int(0, 1));
                    ShineGround(piecePosition + new Vector2Int(0, -1));
                    ShineGround(piecePosition + new Vector2Int(1, 0));
                    ShineGround(piecePosition + new Vector2Int(-1, 0));
                    ShineGround(piecePosition + new Vector2Int(1, 1));
                    ShineGround(piecePosition + new Vector2Int(1, -1));
                    ShineGround(piecePosition + new Vector2Int(-1, 1));
                    ShineGround(piecePosition + new Vector2Int(-1, -1));
                    CheckCastling();
                    break;
                }
            default:
                {
                    Debug.Log(focusPiece.GetComponent<PieceBaseCtrl>().pieceName);
                    break;
                }
        }
    }

    void CheckPieceCaptureLogic()
    {
        var capturePiece = moveToGrid.GetComponent<GridBoxBaseCtrl>().steppingObject;

        if (moveToGrid.GetComponent<GridBoxBaseCtrl>().hasPiece && capturePiece.GetComponent<PieceBaseCtrl>().side != side)
        {
            CapturePiece(capturePiece);
        }
    }

    void CapturePiece(GameObject g)
    {
        var PCBScriptOnG = g.GetComponent<PieceBaseCtrl>();
        if (PCBScriptOnG.pieceName == "king")
        {
            if (PCBScriptOnG.side == 1) { GameLogic.EndOfGame(2); }
            if (PCBScriptOnG.side == 2) { GameLogic.EndOfGame(1); }
        }
        g.SetActive(false);
        childrenGridBoxes[PCBScriptOnG.position.x][PCBScriptOnG.position.y].GetComponent<GridBoxBaseCtrl>().hasPiece = false;
        childrenGridBoxes[PCBScriptOnG.position.x][PCBScriptOnG.position.y].GetComponent<GridBoxBaseCtrl>().steppingObject = null;
        captureList.Add(g);
    }

    IEnumerator Move(Vector2Int movePosition)
    {
        var focusPiecePCBScript = focusPiece.GetComponent<PieceBaseCtrl>();
        if (PawnEnPassant != new Vector2Int(-1, -1))
        {
            PawnEnPassant = new Vector2Int(-1, -1);
        } 
        if (focusPiecePCBScript.pieceName == "pawn")
        {
            if (Mathf.Abs(movePosition.y) == 2) { PawnEnPassant = focusPiecePCBScript.position + movePosition; }
            if (PawnEnPassantChoice && (gridBoxEP.GetComponent<GridBoxBaseCtrl>().position - moveToGrid.GetComponent<GridBoxBaseCtrl>().position).x == 0)
            {
                gridBoxEP.GetComponent<MeshRenderer>().materials = gridBoxEP.GetComponent<GridBoxBaseCtrl>().rawMaterial;
                CapturePiece(gridBoxEP.GetComponent<GridBoxBaseCtrl>().steppingObject);
                gridBoxEP = null;
                PawnEnPassantChoice = false;
                print("eat by ep");
            }
            else if (PawnEnPassantChoice)
            {
                gridBoxEP.GetComponent<MeshRenderer>().materials = gridBoxEP.GetComponent<GridBoxBaseCtrl>().rawMaterial;
                gridBoxEP = null;
                PawnEnPassantChoice = false;
                print("passed ep");
            }
        }

        if (KingCastlingChoice)
        {
            if (moveToGrid.GetComponent<GridBoxBaseCtrl>().position == KingCastling[0])
            {
                var rookPosition = new Vector2Int(-(int)Mathf.Sign(movePosition.x), 0) + KingCastling[0];
                var rookRawPosition = new Vector2Int((Mathf.Sign(movePosition.x) > 0) ? 7 : 0, KingCastling[0].y);
                var rook = childrenGridBoxes[rookRawPosition.x][rookRawPosition.y].GetComponent<GridBoxBaseCtrl>().steppingObject;

                var translation2DRook = moveStep;
                translation2DRook.x *= (rookPosition - rookRawPosition).y;
                translation2DRook.y = 0;
                translation2DRook.z *= (rookPosition - rookRawPosition).x;
                rook.transform.position += translation2DRook;
            }
            else if (moveToGrid.GetComponent<GridBoxBaseCtrl>().position == KingCastling[1])
            {
                var rookPosition = new Vector2Int((int)Mathf.Sign(movePosition.x), 0) + KingCastling[1];
                var rookRawPosition = new Vector2Int(((int)Mathf.Sign(movePosition.x) == 1) ? 7 : 0, KingCastling[1].y);
                var rook = childrenGridBoxes[rookRawPosition.x][rookRawPosition.y].GetComponent<GridBoxBaseCtrl>().steppingObject;

                var translation2DRook = moveStep;
                translation2DRook.x *= (rookPosition - rookRawPosition).y;
                translation2DRook.y = 0;
                translation2DRook.z *= (rookPosition - rookRawPosition).x;
                rook.transform.position += translation2DRook;
            }
            KingCastlingChoice = false;
        }

        var translation2D = moveStep;
        translation2D.x *= movePosition.y;
        translation2D.y = 0;
        translation2D.z *= movePosition.x;

        int NumberOfFramesForCoroutine = 1;
        for (int i = 0; i < NumberOfFramesForCoroutine; i++)
        {
            focusPiece.transform.position += translation2D;
            yield return new WaitForEndOfFrame();
        }

        focusPiecePCBScript.moves += 1;
        focusPiecePCBScript.isSelected = false;

        if (focusPiecePCBScript.pieceName == "pawn") { CheckPromotion(); }
        
        ResetMaterial();
        ResetAfterMove();

        ispieceSelected = false;
        side = (side == 1) ? 2 : 1;
        MaskName = (side == 1) ? "whitePiece" : "blackPiece";
    }
}
