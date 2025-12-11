using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Tiempo mínimo que se mostrará la pantalla")]
    public float minimumDisplayTime = 1f;
    
    [Tooltip("Tiempo de fade out al terminar")]
    public float fadeOutDuration = 0.5f;
    
    [Header("Referencias")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI loadingText;
    public Image logoImage; // Imagen fija que siempre será la misma
    
    [Header("Animación de Texto (Opcional)")]
    public bool animateLoadingText = true;
    public string baseText = "Cargando";
    public float dotAnimationSpeed = 0.5f;
    
    private Canvas canvas;
    private int dotCount = 0;

    private void Awake()
    {
        // Configurar el Canvas para que aparezca encima de todo
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvas.overrideSorting = true;
        }
        
        // Obtener CanvasGroup si no está asignado
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Iniciar visible
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;
        
        // No destruir al cargar nueva escena
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Iniciar animación de texto si está habilitada
        if (animateLoadingText && loadingText != null)
        {
            StartCoroutine(AnimateLoadingText());
        }
        
        // Iniciar el proceso de carga
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        // Esperar el tiempo mínimo
        yield return new WaitForSeconds(minimumDisplayTime);
        
        // Esperar a que el jugador esté listo
        yield return new WaitUntil(() => IsPlayerReady());
        
        // Pequeña pausa adicional
        yield return new WaitForSeconds(0.2f);
        
        // Hacer fade out
        yield return StartCoroutine(FadeOut());
        
        // Destruir la pantalla de carga
        Destroy(gameObject);
    }

    private bool IsPlayerReady()
    {
        // Verificar si el jugador existe y está listo
        Player player = FindFirstObjectByType<Player>();
        return player != null;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
    
    private IEnumerator AnimateLoadingText()
    {
        if (loadingText == null) yield break;
        
        while (canvasGroup.alpha > 0)
        {
            dotCount = (dotCount + 1) % 4;
            loadingText.text = baseText + new string('.', dotCount);
            yield return new WaitForSeconds(dotAnimationSpeed);
        }
    }
    
    // Método público para ocultar la pantalla manualmente
    public void HideLoadingScreen()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
        StartCoroutine(DestroyAfterFade());
    }
    
    private IEnumerator DestroyAfterFade()
    {
        yield return new WaitForSeconds(fadeOutDuration + 0.1f);
        Destroy(gameObject);
    }
    
    // Actualizar el texto de carga
    public void SetLoadingText(string text)
    {
        if (loadingText != null)
        {
            baseText = text;
            loadingText.text = text;
        }
    }
}