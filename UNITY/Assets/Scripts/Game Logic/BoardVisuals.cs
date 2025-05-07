using System;
using UnityEngine;
using static GameState;

public class BoardVisuals : MonoBehaviour
{
    // pieces on the board
    /* with each index corresponding to these pieces:
     *  0  1  2  3  4
     *  5  6  7  8  9
     *  10 11 12 13 14
     *  15 16 17 18 19
     *  20 21 22 23 24
     */

    public Piece[] pieces;
    public GameObject bonusIndicator;

    void BeginGame()
    {
        Tuple<int, int> bonusLoc = GameState.instance.bonusLoc;
        bonusIndicator.SetActive(true);
        bonusIndicator.transform.position = pieces[(bonusLoc.Item1 * 5) + bonusLoc.Item2].transform.position + Vector3.up * 0.5f;
    }

    // 0 <= y, x <= 4
    public void SetPiece(int y, int x, GameState.Side side)
    {
        pieces[(y * 5) + x].ShowSide(side);
    }

    // highlight a specific tile as containing an "error"
    public void ShowError(int y, int x, bool enabled)
    {
        pieces[(y * 5) + x].ShowErrorTile(enabled);
    }


    // DEBUG

    private void Start()
    {
        BeginGame();
    }

}
