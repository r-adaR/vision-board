using DG.Tweening;
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

    [SerializeField] private GameObject gameUI;

    public Piece[] pieces;
    public GameObject bonusIndicator;
    private ParticleSystem bonusParticles;

    private Vector3 bonusScale;

    public void UpdateBonusLocation()
    {
        bonusIndicator.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutExpo).OnComplete(() => { 

            Tuple<int, int> bonusLoc = game_instance.bonusLoc;
            if (bonusLoc == null)
            {
                // hide it
                HideBonus();
                return;
            }
            bonusIndicator.SetActive(true);
            bonusIndicator.transform.position = pieces[(bonusLoc.Item2 * 5) + bonusLoc.Item1].transform.position + Vector3.up * 0.5f;

            // bonus tween
            bonusIndicator.transform.localScale = Vector3.zero;
            bonusIndicator.transform.DOScale(bonusScale, 0.7f).SetEase(Ease.OutElastic);

        });
    }

    // 0 <= y, x <= 4
    public void SetPiece(int y, int x, Side side)
    {
        pieces[(x * 5) + y].ShowSide(side);
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

    public void HideUI()
    {
        gameUI.SetActive(false);
    }

    public void EmitBonusParticles()
    {
        bonusParticles.Stop();
        bonusParticles.Play();
    }

    public void ClearErrors()
    {
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                ShowError(y, x, false);
            }
        }
    }

    public void HideBonus()
    {
        bonusIndicator.transform.localScale = Vector3.zero;
    }

    public void SpawnPointsAbovePiece(int y, int x, int score)
    {
        Piece piece = pieces[(x * 5) + y];
        PointIndicationSpawner.Instance.Spawn(
            score, 
            game_instance.currentPlayer == Side.X ? new Color(.25f, .72f, 1f) : new Color(1f, .78f, .25f), 
            piece.transform.position
            );
    }


    // highlight a specific tile as containing an "error"
    private void ShowError(int y, int x, bool enabled)
    {
        pieces[(x * 5) + y].ShowErrorTile(enabled);
    }


    // DEBUG

    private void Start()
    {
        bonusScale = bonusIndicator.transform.localScale;
        bonusParticles = bonusIndicator.GetComponentInChildren<ParticleSystem>();
        UpdateBonusLocation();
    }

}
