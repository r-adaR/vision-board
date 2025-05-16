using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class LoadingScreen : MonoBehaviour
{

    [SerializeField] private RectTransform m_blackRect;
    [SerializeField] private RectTransform m_whiteRect;

    // event function that will run once the loading scene intro is completed!
    [SerializeField] private UnityEvent onLoadingFinished;

    [SerializeField]
    private float m_tweenTime = 0.5f;

    private Sequence m_transformTween;


    public static LoadingScreen instance {  get; private set; }
    

    public enum Scene
    {
        INIT,
        TITLE,
        GAME
    }


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (VariableStorage.instance != null && !VariableStorage.instance.TryGet<bool>("loadWithAnimation"))
        {
            m_blackRect.gameObject.SetActive(false); // disable this object in the hierarchy
            m_whiteRect.gameObject.SetActive(false); // disable this object in the hierarchy
            onLoadingFinished.Invoke();

            // from now on, load scenes with the transition animation. This was only disabled for the title screen's initial startup.
            VariableStorage.instance.Put("loadWithAnimation", true);
        }
        else
        {
            m_transformTween = DOTween.Sequence();
            m_transformTween.Append(m_blackRect.transform.DOScaleX(0f, m_tweenTime).SetEase(Ease.OutExpo));
            m_transformTween.Insert(m_tweenTime / 5, m_whiteRect.transform.DOScaleX(0f, 3 * m_tweenTime / 4).SetEase(Ease.OutExpo));
            m_transformTween.Play();

            m_transformTween.OnComplete(() =>
            {
                m_blackRect.gameObject.SetActive(false); // disable this object in the hierarchy
                m_whiteRect.gameObject.SetActive(false); // disable this object in the hierarchy
                onLoadingFinished.Invoke();
            });
        }
    }

    
    public void LoadScene(Scene index)
    {
        if (m_transformTween.IsActive())
            m_transformTween.Kill();

        m_blackRect.transform.localScale = new Vector3(1, 0, 1);    // set initial positions for loading transition graphics
        m_whiteRect.transform.localScale = new Vector3(1, 0, 1);
        m_blackRect.gameObject.SetActive(true);                     // re-enable the graphics for the loading screen
        m_whiteRect.gameObject.SetActive(true);

        m_transformTween = DOTween.Sequence();
        m_transformTween.Append(m_whiteRect.transform.DOScaleY(1f, m_tweenTime).SetEase(Ease.OutExpo));
        m_transformTween.Insert(m_tweenTime/5, m_blackRect.transform.DOScaleY(1f, 3 * m_tweenTime / 4).SetEase(Ease.OutExpo));
        m_transformTween.Play();

        m_transformTween.OnComplete(() => SceneManager.LoadSceneAsync((int)index));

    }


}
