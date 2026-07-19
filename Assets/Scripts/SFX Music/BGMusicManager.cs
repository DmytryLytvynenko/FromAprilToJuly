using UnityEngine;

public class BGMusicManager : MonoBehaviour
{
    public static BGMusicManager Instance;
    [field: SerializeField] public AudioSource BGMusic { get; private set; }
    [field: SerializeField] public AudioSource BGWind { get; private set; }
    [field: SerializeField] public AudioSource SFX { get; private set; }
    [field: SerializeField] public SFXActor SFXActor { get; private set; }
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
    void Start()
    {
        BGMusic.gameObject.SetActive(false);
        BGWind.gameObject.SetActive(false);
        SFX.gameObject.SetActive(false);
        Invoke(nameof(ActivateMusicSFX), 1f);
    }
    private void ActivateMusicSFX()
    {
        BGMusic.gameObject.SetActive(true);
        BGWind.gameObject.SetActive(true);
        SFX.gameObject.SetActive(true);
    }
}
