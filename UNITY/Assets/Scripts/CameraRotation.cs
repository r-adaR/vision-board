using UnityEngine;
using DG.Tweening;

public class CameraRotation : MonoBehaviour
{

    [SerializeField] private Transform pivot;
    private Tween rotTween;

    public void StartRotation()
    {
        rotTween = pivot.DOLocalRotate(Vector3.up * 180f, 1.4f, RotateMode.FastBeyond360).SetEase(Ease.InExpo).OnComplete(() =>
        {
            pivot.DOLocalRotate(Vector3.up * 360f, 1.4f, RotateMode.FastBeyond360).SetEase(Ease.OutExpo).OnComplete(() => pivot.localRotation = Quaternion.Euler(Vector3.zero));
        });
    }

    public void Rotate90()
    {
        if (rotTween != null && rotTween.IsActive()) return;
        rotTween = pivot.DOLocalRotate(pivot.localRotation.eulerAngles + Vector3.up * 90f, 0.4f, RotateMode.FastBeyond360).SetEase(Ease.OutExpo);
    }

}
