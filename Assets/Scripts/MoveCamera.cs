using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public float elevation = 200f;
    public float distance = 120f;

    GameObject LookAtObject;
    MovePiece movePieceScript;
    Vector3 cameraSide1Position;
    Vector3 cameraSide2Position;
    Vector3 cameraMiddlePosition;
    int viewSide = 1;
    int side = 1;

    public int translationFrames = 20;

    // Start is called before the first frame update
    void Start()
    {
        LookAtObject = GameObject.Find("/chessboard");
        movePieceScript = LookAtObject.GetComponent<MovePiece>();
        cameraSide1Position = new Vector3(-distance, elevation, 0);
        cameraSide2Position = new Vector3(distance, elevation, 0);
        cameraMiddlePosition = new Vector3(0, Mathf.Sqrt(distance * distance + elevation * elevation), 0);
        transform.position = cameraMiddlePosition;
        transform.rotation = Quaternion.LookRotation(LookAtObject.transform.position - transform.position);
        StartCoroutine(MoveCameraSide(1));
        // transform.rotation = Quaternion.Euler(new Vector3(30, 90, 0));
    }

    // Update is called once per frame
    void Update()
    {
        bool isSelecting = movePieceScript.ispieceSelected;
        if (side != movePieceScript.side || Input.GetMouseButtonDown(1))
        {
            side = movePieceScript.side;
            StartCoroutine(MoveCameraSide(movePieceScript.viewSide));
        }
        if (viewSide != movePieceScript.viewSide)
        {
            viewSide = movePieceScript.viewSide;
            StartCoroutine(IMoveChessboard(movePieceScript.viewSide));
        }
        if (isSelecting && (transform.position == cameraSide1Position || transform.position == cameraSide2Position))
        {
            StartCoroutine(MoveCameraAbove());
        }
    }

    IEnumerator MoveCameraAbove()
    {
        var endPosition = cameraMiddlePosition;
        var startPosition = transform.position;
        var r = (startPosition - LookAtObject.transform.position).magnitude;

        for (int i = 1; i <= 0.9 * translationFrames; i++)
        {
            var thisFramePosition = startPosition + i * (endPosition - startPosition) / translationFrames;
            thisFramePosition -= LookAtObject.transform.position;
            thisFramePosition = thisFramePosition * r / thisFramePosition.magnitude + LookAtObject.transform.position;
            transform.position = thisFramePosition;
            transform.rotation = Quaternion.LookRotation(LookAtObject.transform.position - transform.position);

            yield return new WaitForEndOfFrame();
        }

    }

    IEnumerator IMoveChessboard(int viewSide)
    {
        var endPosition = (viewSide == 1) ? cameraSide1Position : cameraSide2Position;
        var startPosition = transform.position;
        float r = (startPosition - LookAtObject.transform.position).magnitude;

        for (int i = 1; i <= translationFrames; i++)
        {
            var deltaX = i * (endPosition.x - startPosition.x) / translationFrames;
            var deltaZ = Mathf.Sqrt(distance * distance - (distance - Mathf.Abs(deltaX)) * (distance - Mathf.Abs(deltaX)));
            var thisFramePosition = startPosition + new Vector3(deltaX, 0, deltaZ);

            thisFramePosition -= LookAtObject.transform.position;
            thisFramePosition = thisFramePosition * r / thisFramePosition.magnitude + LookAtObject.transform.position;
            transform.position = thisFramePosition;
            transform.rotation = Quaternion.LookRotation(LookAtObject.transform.position - transform.position);

            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;
    }

    IEnumerator MoveCameraSide(int side)
    {
        var endPosition = (side == 1) ? cameraSide1Position : cameraSide2Position;
        var startPosition = transform.position;
        var r = (startPosition - LookAtObject.transform.position).magnitude;

        for (int i = 1; i <= translationFrames; i++)
        {
            var thisFramePosition = startPosition + i * (endPosition - startPosition) / translationFrames;
            thisFramePosition -= LookAtObject.transform.position;
            thisFramePosition = thisFramePosition * r / thisFramePosition.magnitude + LookAtObject.transform.position;
            transform.position = thisFramePosition;
            transform.rotation = Quaternion.LookRotation(LookAtObject.transform.position - transform.position);

            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;
    }
}
