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

    public void UpdateBonusLocation()
    {
        Tuple<int, int> bonusLoc = game_instance.bonusLoc;
        bonusIndicator.SetActive(true);
        bonusIndicator.transform.position = pieces[(bonusLoc.Item1 * 5) + bonusLoc.Item2].transform.position + Vector3.up * 0.5f;
    }

    // 0 <= y, x <= 4
    public void SetPiece(int y, int x, Side side)
    {
        pieces[(y * 5) + x].ShowSide(side);
    }

    public void UpdateErrors(bool[,] errorArray)
    {
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                ShowError(y, x, errorArray[y, x]);
            }
        }
    }


    // highlight a specific tile as containing an "error"
    private void ShowError(int y, int x, bool enabled)
    {
        pieces[(y * 5) + x].ShowErrorTile(enabled);
    }


    // DEBUG

    private void Start()
    {
        UpdateBonusLocation();
    }

}
