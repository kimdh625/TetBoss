using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop(); // ������ ����ϴ�.
        }
    }

    public void PlayMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play(); // ������ ����մϴ�.
        }
    }
}
