using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{

    [SerializeField]
    private Image m_graphic;

    [SerializeField]
    private float m_tweenTime = 0.5f;

    private Tween m_transformTween;


    public static LoadingScreen instance {  get; private set; }
    

    public enum Scene
    {
        TITLE = 0,
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
        m_transformTween = m_graphic.transform.DOScaleX(0f, m_tweenTime).SetEase(Ease.OutExpo);

    }

    
    public void LoadScene(Scene index)
    {
        if (m_transformTween.IsActive())
            m_transformTween.Kill();

        m_graphic.transform.localScale = new Vector3(1, 0, 1);
        m_transformTween = m_graphic.transform.DOScaleY(1f, m_tweenTime).SetEase(Ease.OutExpo);

        m_transformTween.OnComplete(() => SceneManager.LoadSceneAsync((int)index));

    }


}
