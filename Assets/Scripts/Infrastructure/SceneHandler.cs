using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance;
    public static event Action<SceneName> SceneLoaded;

    [SerializeField] private Canvas _transitionCanvas;
    [SerializeField] private CanvasGroup _transitionBanner;
    [SerializeField] private GameObject _loadIcon;
    [SerializeField] private Animator _loadIconAnimator;
    [SerializeField] private float _transitionBannerAppearTime = 1f;
    [SerializeField] private GameObject _menuRocks;

    private AsyncOperation loadSceneOperation = null;

    private void OnEnable()
    {
        InputManager.Instance.OnJump.AddListener(OnSpacebarPressed);
    }
    private void OnDisable()
    {
        InputManager.Instance.OnJump.RemoveListener(OnSpacebarPressed);
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator LoadSceneCoroutine(SceneName name)
    {
        _loadIconAnimator.SetBool(AnimatorParams.SceneLoaded.ToString(), false);
        yield return Instance.StartCoroutine(Instance.ShowBanner());

        if (_menuRocks)
        {
            _menuRocks.SetActive(false);
            _menuRocks = null;
        } 

        loadSceneOperation = SceneManager.LoadSceneAsync(name.ToString());
        loadSceneOperation.allowSceneActivation = false;

        while (loadSceneOperation.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        SceneLoaded?.Invoke(name);
        _loadIconAnimator.SetBool(AnimatorParams.SceneLoaded.ToString(), true);
    }

    private IEnumerator ShowBanner()
    {
        _transitionBanner.alpha = 0f;
        _transitionCanvas.gameObject.SetActive(true);
        _loadIcon.gameObject.SetActive(true);

        float progress = 0f;
        float expiredTime = 0f;
        while (progress < 1f)
        {
            expiredTime += Time.deltaTime;
            progress = expiredTime / _transitionBannerAppearTime;
            _transitionBanner.alpha = progress;
            yield return null;
        }
        _transitionBanner.alpha = 1f;
    }
    private IEnumerator HideBanner()
    {
        yield return new WaitForSeconds(.5f);
        float progress = 1f;
        float expiredTime = 0f;
        while (progress > 0f)
        {
            expiredTime += Time.deltaTime;
            progress = (_transitionBannerAppearTime - expiredTime) / _transitionBannerAppearTime;
            _transitionBanner.alpha = progress;
            yield return null;
        }
        _transitionBanner.alpha = 0f;
        _transitionCanvas.gameObject.SetActive(false);
        _loadIcon.gameObject.SetActive(false);
    }

    private static void TransferToScene()
    {
        if (Instance.loadSceneOperation == null)
        {
            #if UNITY_EDITOR
                Debug.LogError("AsyncOperation is null");
            #endif
            return;
        }
        Instance.StartCoroutine(Instance.HideBanner());
        BGMusicManager.Instance.SFXActor.PlaySound(SFX.CheckPoint);
        Instance.loadSceneOperation.allowSceneActivation = true;
    }

    public static void LoadScene(SceneName name)
    {
        Instance.StartCoroutine(Instance.LoadSceneCoroutine(name));
    }
    public static void LoadSceneByName(string _name)
    {
        if (Enum.TryParse(_name, out SceneName name))
        {
            Instance.StartCoroutine(Instance.LoadSceneCoroutine(name));
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Invalid scene name");
#endif
        }
    }
    private void OnSpacebarPressed(InputAction.CallbackContext ctx)
    {
        TransferToScene();
    }
    public enum SceneName
    {
        Menu, Main
    }
}
