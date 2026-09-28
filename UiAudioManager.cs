using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    [Header("UI Click Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayClick()
    {
        if (audioSource == null)
        {
            Debug.LogError("UI AUDIO FAILED | AudioSource is missing.");
            return;
        }

        if (clickSound == null)
        {
            Debug.LogError("UI AUDIO FAILED | Click sound is missing.");
            return;
        }

        audioSource.PlayOneShot(clickSound, clickVolume);
    }
}