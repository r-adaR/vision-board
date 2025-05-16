using UnityEngine;
using DG.Tweening;

public class Piece : MonoBehaviour
{
    [SerializeField]
    private GameObject X_PIECE;

    [SerializeField]
    private GameObject O_PIECE;

    [SerializeField]
    private GameObject ERROR_TILE;

    public GameState.Side displayedSide = GameState.Side.NONE;

    private Tween scaleTween;
    private AudioSource _audio;

    private void Start()
    {
        // presents as "NONE" when initialized
        X_PIECE.SetActive(false);
        O_PIECE.SetActive(false);
        ERROR_TILE.SetActive(false);

        _audio = GetComponent<AudioSource>();
    }

    public void ShowSide(GameState.Side side)
    {
        if (displayedSide == side) return;
        if (displayedSide != GameState.Side.NONE)
        {
            HidePiece();
        }

        // shows piece
        displayedSide = side;
        if (side == GameState.Side.NONE) return; // can return, since HidePiece() was already called

        scaleTween?.Kill();
        _audio.time = 0f;
        _audio.Play();

        if (side == GameState.Side.O)
        {
            O_PIECE.SetActive(true);
            transform.localScale = Vector3.zero;
            scaleTween = transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutElastic);
        }
        else if (side == GameState.Side.X)
        {
            X_PIECE.SetActive(true);
            transform.localScale = Vector3.zero;
            scaleTween = transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutElastic);
        }
    }

    public void HidePiece()
    {
        // hide this piece
        displayedSide = GameState.Side.NONE;
        X_PIECE.SetActive(false);
        O_PIECE.SetActive(false);
    }

    public void ShowErrorTile(bool enabled)
    {
        ERROR_TILE.SetActive(enabled);
    }
}
