using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class CameraRotation : MonoBehaviour
{

    [SerializeField] private Transform pivot;
    private Tween rotTween;

    public UnityEvent OnIntroComplete = new UnityEvent();

    public void StartRotation()
    {
        rotTween = pivot.DOLocalRotate(Vector3.up * 180f, 1.4f, RotateMode.FastBeyond360).SetEase(Ease.InExpo).OnComplete(() =>
        {
            pivot.DOLocalRotate(Vector3.up * 360f, 1.4f, RotateMode.FastBeyond360).SetEase(Ease.OutExpo).OnComplete(() => pivot.localRotation = Quaternion.Euler(Vector3.zero)).OnComplete(() => OnIntroComplete.Invoke());
        });
    }

    public void Rotate90()
    {
        if (rotTween != null && rotTween.IsActive()) return;
        GameFlow.flow_instance.canScan = false;
        rotTween = pivot.DOLocalRotate(pivot.localRotation.eulerAngles + Vector3.up * 90f, 0.4f, RotateMode.FastBeyond360).SetEase(Ease.OutExpo)
            .OnComplete(() => GameFlow.flow_instance.canScan = true);
    }

}
