using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GhostSoundController : MonoBehaviour
{
    [Header("Ghost Sounds")]
    [SerializeField] private AudioClip[] ghostSounds;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError(
                $"GHOST AUDIO FAILED | AudioSource missing on {name}");
        }
    }

    public void PlaySoundLocal(int soundIndex)
    {
        if (audioSource == null)
        {
            Debug.LogError(
                "GHOST SOUND FAILED | AudioSource is null.");

            return;
        }

        if (ghostSounds == null ||
            ghostSounds.Length == 0)
        {
            Debug.LogError(
                "GHOST SOUND FAILED | Ghost Sounds array is empty.");

            return;
        }

        if (soundIndex < 0 ||
            soundIndex >= ghostSounds.Length)
        {
            Debug.LogError(
                $"GHOST SOUND FAILED | Invalid index={soundIndex}");

            return;
        }

        AudioClip clip = ghostSounds[soundIndex];

        if (clip == null)
        {
            Debug.LogError(
                $"GHOST SOUND FAILED | Clip at index {soundIndex} is null.");

            return;
        }

        audioSource.PlayOneShot(clip);

        Debug.Log(
            $"GHOST SOUND PLAYED LOCALLY | " +
            $"Index={soundIndex} | Clip={clip.name}");
    }
}