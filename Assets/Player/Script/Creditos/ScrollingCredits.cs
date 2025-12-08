using UnityEngine;
using System.Collections;

public class ScrollingCredits : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de subida del texto (ej: 50.0f).")]
    public float scrollSpeed = 50.0f; 

    [Tooltip("La posición Y final donde el texto se detiene (opcional, 0 = auto).")]
    public float endPositionY = 0f; 

    [Header("Configuración de Fade In")]
    [Tooltip("Duración del efecto de aparición (fade in) en segundos.")]
    public float fadeInDuration = 2.0f;

    [Header("Configuración de Fade Out")]
    [Tooltip("Duración del efecto de oscurecimiento (fade out) en segundos.")]
    public float fadeOutDuration = 2.0f; 

    [Tooltip("Arrastra aquí el Panel_Creditos (el objeto padre con Canvas Group).")]
    public CanvasGroup canvasGroupToFade;

    [Header("Inicio Automático")]
    [Tooltip("¿Comenzar automáticamente cuando el objeto se active?")]
    public bool autoStart = false;

    [Header("Música de Fondo")]
    [Tooltip("AudioSource con la música de los créditos (opcional).")]
    public AudioSource creditsMusic;

    [Tooltip("Duración del fade out de la música en segundos.")]
    public float musicFadeOutDuration = 3.0f;

    [Tooltip("¿Detener la música del juego al iniciar los créditos?")]
    public bool stopGameMusic = true;

    private RectTransform rectTransform;
    private bool isScrolling = false;
    private float calculatedEndY;
    private float originalMusicVolume;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (canvasGroupToFade == null)
        {
            Debug.LogWarning("Canvas Group no asignado. Buscando en el padre...");
            canvasGroupToFade = GetComponentInParent<CanvasGroup>();
        }

        // Calcular automáticamente la posición final si endPositionY es 0
        if (endPositionY == 0f)
        {
            calculatedEndY = rectTransform.rect.height + 500f;
        }
        else
        {
            calculatedEndY = endPositionY;
        }

        if (autoStart)
        {
            StartCredits();
        }
    }

    void Update()
    {
        if (isScrolling)
        {
            // Mover el contenedor hacia arriba
            float movement = scrollSpeed * Time.deltaTime;
            rectTransform.anchoredPosition += new Vector2(0, movement);

            // Verificar si se alcanzó la posición final
            if (rectTransform.anchoredPosition.y >= calculatedEndY)
            {
                StartCoroutine(FadeOutAndDeactivate());
                isScrolling = false;
            }
        }
    }

    /// Método público para iniciar los créditos con fade in
    public void StartCredits()
    {
        if (canvasGroupToFade != null)
        {
            canvasGroupToFade.gameObject.SetActive(true);
        }
        
        // Reinicia la posición inicial (abajo de la pantalla)
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -Screen.height); 
        
        // Detener música del juego si está configurado
        if (stopGameMusic)
        {
            AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (AudioSource audio in allAudioSources)
            {
                if (audio != creditsMusic && audio.isPlaying)
                {
                    audio.Stop();
                }
            }
        }

        // Reproducir música de créditos
        if (creditsMusic != null)
        {
            originalMusicVolume = creditsMusic.volume;
            creditsMusic.volume = 0f; // Empezar desde volumen 0
            creditsMusic.Play();
            StartCoroutine(FadeMusicIn());
        }
        
        // Iniciar el fade in visual
        StartCoroutine(FadeInAndStartScrolling());
    }

    /// Detener los créditos manualmente
    public void StopCredits()
    {
        isScrolling = false;
        StopAllCoroutines();
        
        // Detener la música
        if (creditsMusic != null && creditsMusic.isPlaying)
        {
            creditsMusic.Stop();
        }
        
        if (canvasGroupToFade != null)
        {
            canvasGroupToFade.gameObject.SetActive(false);
        }
    }

    /// Corrutina para aparecer gradualmente (fade in) y luego iniciar el scroll
    private IEnumerator FadeInAndStartScrolling()
    {
        Debug.Log($"Iniciando Fade In de {fadeInDuration}s.");

        if (canvasGroupToFade == null)
        {
            isScrolling = true;
            yield break;
        }

        // Empezar con opacidad 0 (transparente)
        canvasGroupToFade.alpha = 0f;
        float elapsed = 0f;

        // Aumentar gradualmente la opacidad de 0 a 1
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroupToFade.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        // Asegurar que llegue a opacidad completa
        canvasGroupToFade.alpha = 1f;
        
        // Ahora sí, iniciar el scroll
        isScrolling = true;
        Debug.Log("Fade In completado. Créditos iniciados.");
    }
    
    /// Corrutina para desvanecer (fade out) y desactivar
    private IEnumerator FadeOutAndDeactivate()
    {
        Debug.Log($"Créditos finalizados. Iniciando Fade Out de {fadeOutDuration}s.");

        // Fade out de la música
        if (creditsMusic != null && creditsMusic.isPlaying)
        {
            StartCoroutine(FadeMusicOut());
        }

        if (canvasGroupToFade == null) yield break;

        float startAlpha = canvasGroupToFade.alpha;
        float elapsed = 0f;

        // Disminuir gradualmente la opacidad de 1 a 0
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroupToFade.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        // Asegurar que llegue a transparente
        canvasGroupToFade.alpha = 0f; 
        canvasGroupToFade.gameObject.SetActive(false);
        Debug.Log("Panel de créditos desactivado.");
    }

    /// Fade in de la música
    private IEnumerator FadeMusicIn()
    {
        if (creditsMusic == null) yield break;

        float elapsed = 0f;
        float duration = fadeInDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            creditsMusic.volume = Mathf.Lerp(0f, originalMusicVolume, elapsed / duration);
            yield return null;
        }

        creditsMusic.volume = originalMusicVolume;
    }

    /// Fade out de la música
    private IEnumerator FadeMusicOut()
    {
        if (creditsMusic == null || !creditsMusic.isPlaying) yield break;

        float startVolume = creditsMusic.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            creditsMusic.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeOutDuration);
            yield return null;
        }

        creditsMusic.volume = 0f;
        creditsMusic.Stop();
    }
}