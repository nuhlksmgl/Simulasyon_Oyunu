using UnityEngine;
using System.Collections;

public class AmbienceManager : MonoBehaviour
{
    private AudioSource audioSource;
    private float targetVolume;
    private float fadeSpeed = 1f; // Fade hýzý (saniyede deðiþim)

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component’i bulunamadý!", this);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Baþlangýçta sesi yumuþak bir þekilde çal
        StartCoroutine(FadeIn());
    }

    public void PlayAmbience()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            StartCoroutine(FadeIn());
        }
    }

    public void StopAmbience()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeIn()
    {
        audioSource.volume = 0f;
        audioSource.Play();
        targetVolume = 0.5f; // Hedef ses seviyesi

        while (audioSource.volume < targetVolume)
        {
            audioSource.volume += fadeSpeed * Time.deltaTime;
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    private IEnumerator FadeOut()
    {
        targetVolume = 0f;

        while (audioSource.volume > 0f)
        {
            audioSource.volume -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        audioSource.Stop();
    }
}