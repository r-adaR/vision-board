using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class PointIndicator : MonoBehaviour
{

    [SerializeField] TMP_Text _text;

    public void SetVisuals(string text, Color color)
    {
        _text.text = text;
        _text.color = color;
    }

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    public void PlayAnimation()
    {
        transform.DOLocalMoveY(transform.localPosition.y + 60f, 0.5f).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InExpo).OnComplete(() => Destroy(gameObject));
        });
    }

}
