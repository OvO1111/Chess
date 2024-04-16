using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class GameGUICtrl : MonoBehaviour
{
    SetStartPiecePositions SSPPScript;
    MovePiece MPScript;

    // Start is called before the first frame update
    void Awake()
    {
        SSPPScript = Camera.main.GetComponent<SetStartPiecePositions>();
        MPScript = GameObject.Find("/chessboard").GetComponent<MovePiece>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) { ResetGame(); }
        if (Input.GetKeyDown(KeyCode.Q)) { QuitGame(); }
    }

    public void ResetGame()
    {
        Time.timeScale = 1;
        Camera.main.GetComponent<SetStartPiecePositions>().ResetAllPositions();
        GameObject.Find("/chessboard").GetComponent<MovePiece>().ClearState();
    }

    public void QuitGame()
    {
        #if UNITY_STANDALONE
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ReverseViewport()
    {
        MPScript.viewSide = (MPScript.viewSide == 1) ? 2 : 1;
    }

    public void EndOfGame(int winnerIndex)
    {
        string winner = (winnerIndex == 1)? "White": "Black";
        string loser = (winnerIndex == 2)? "White": "Black";
        Time.timeScale = 0;
        QuitGame();
    }
}
