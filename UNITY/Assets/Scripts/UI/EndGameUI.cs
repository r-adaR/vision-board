using DG.Tweening;
using TMPro;
using UnityEngine;

public class EndGameUI : MonoBehaviour
{

    [SerializeField] private RectTransform _mask;
    [SerializeField] private Transform _UI;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Transform button;

    private void OnEnable()
    {
        GameFlow.flow_instance.canScan = false;

        transform.localScale = Vector3.zero;
        PlayAnimation();
        if (GameState.game_instance.x_score > GameState.game_instance.o_score)
            _text.text = $"WINNER: X\n{GameState.game_instance.x_score} POINTS";
        else _text.text = $"WINNER: O\n{GameState.game_instance.o_score} POINTS";
        _text.maxVisibleCharacters = 0;
    }


    public void PlayAnimation()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.one * 1.5f, 0.4f).SetEase(Ease.OutElastic)); // bounce end UI in

        seq.Insert(1f, _UI.DOScale(Vector3.one * 0.7f, 0.3f).SetEase(Ease.OutExpo)); // move menu up to make room for other menu
        seq.Insert(1f, _UI.DOLocalMoveY(_UI.localPosition.y + 76, 0.3f).SetEase(Ease.OutExpo));

        seq.Insert(1f, _mask.DOSizeDelta(new Vector2(_mask.rect.width, 700f), 0.7f).SetEase(Ease.OutExpo)); // make mask reveal text box, then type out text
        seq.Insert(1.5f, DOTween.To(() => _text.maxVisibleCharacters, (int val) => _text.maxVisibleCharacters = val, _text.text.Length, 1f).SetEase(Ease.InOutCubic));
        
        
        seq.Append(button.DOLocalMoveY(button.transform.localPosition.y - 100, 0.5f).SetEase(Ease.OutExpo)); // show button to go to title screen

        AudioPlayer.instance.PlaySound("bell");
        seq.Play();
    }

    public void ReturnToTitleScreen()
    {
        LoadingScreen.instance.LoadScene(LoadingScreen.Scene.TITLE);
    }

}
