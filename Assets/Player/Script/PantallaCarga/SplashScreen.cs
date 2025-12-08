using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
    [Header("Configuración de Logos")]
    public Image[] logos; // Arrastra tus imágenes de logos aquí
    public float fadeInDuration = 1f; // Tiempo de aparición
    public float displayDuration = 2f; // Tiempo visible
    public float fadeOutDuration = 1f; // Tiempo de desaparición
    
    [Header("Configuración de Escena")]
    public string nextSceneName = "MainMenu"; // Nombre de tu escena principal
    public float delayBeforeNextScene = 0.5f; // Pausa antes de cambiar escena
    
    [Header("Fondo (Opcional)")]
    public Image background; // Si quieres animar el fondo también

    void Start()
    {
        // Hacer todos los logos transparentes al inicio
        foreach (Image logo in logos)
        {
            SetAlpha(logo, 0f);
        }
        
        StartCoroutine(ShowLogosSequence());
    }

    IEnumerator ShowLogosSequence()
    {
        // Mostrar cada logo con transiciones
        foreach (Image logo in logos)
        {
            // Fade In
            yield return StartCoroutine(FadeImage(logo, 0f, 1f, fadeInDuration));
            
            // Mantener visible
            yield return new WaitForSeconds(displayDuration);
            
            // Fade Out
            yield return StartCoroutine(FadeImage(logo, 1f, 0f, fadeOutDuration));
        }
        
        // Esperar un poco antes de cambiar de escena
        yield return new WaitForSeconds(delayBeforeNextScene);
        
        // Cargar siguiente escena
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetAlpha(image, alpha);
            yield return null;
        }
        
        SetAlpha(image, endAlpha);
    }

    void SetAlpha(Image image, float alpha)
    {
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    // Opcional: Permitir saltar la intro con clic o tecla
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            SkipIntro();
        }
    }

    void SkipIntro()
    {
        StopAllCoroutines();
        SceneManager.LoadScene(nextSceneName);
    }
}