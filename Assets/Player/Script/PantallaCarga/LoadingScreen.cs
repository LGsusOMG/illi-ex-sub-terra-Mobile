using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Tiempo mínimo que se mostrará la pantalla de carga")]
    public float minimumDisplayTime = 1f;
    
    [Tooltip("Tiempo de fade out al terminar")]
    public float fadeOutDuration = 0.5f;
    
    [Header("Referencias")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI loadingText;
    
    [Header("Fondo Aleatorio")]
    [Tooltip("Image del panel de fondo")]
    public Image backgroundImage;
    
    [Tooltip("Sprites de fondo disponibles (MainMenu1, MainMenu2, etc.)")]
    public Sprite[] backgroundSprites;
    
    [Tooltip("Usar fondo aleatorio al iniciar")]
    public bool useRandomBackground = true;
    
    [Header("Animación del Personaje")]
    [Tooltip("Animator del personaje en la pantalla de carga")]
    public Animator characterAnimator;
    
    [Tooltip("Nombre del parámetro de estado en el Animator (ej: 'state')")]
    public string stateParameterName = "state";
    
    [System.Serializable]
    public class AnimationState
    {
        public string name;        // Nombre descriptivo (ej: "Idle", "Run")
        public int stateValue;     // Valor numérico del estado en el Animator
        public bool isBoolParameter; // Si es un parámetro Bool en lugar de state
        public string boolParameterName; // Nombre del parámetro Bool
    }
    
    [Tooltip("Lista de estados de animación disponibles")]
    public AnimationState[] possibleStates = new AnimationState[]
    {
        new AnimationState { name = "Idle", stateValue = 0, isBoolParameter = false },
        new AnimationState { name = "Run", stateValue = 1, isBoolParameter = false },
        new AnimationState { name = "Jump", stateValue = 2, isBoolParameter = false },
        new AnimationState { name = "Fall", stateValue = 3, isBoolParameter = false },
        new AnimationState { name = "Dash", stateValue = 4, isBoolParameter = false },
        new AnimationState { name = "Hurt", stateValue = 5, isBoolParameter = false }
    };
    
    [Tooltip("Usar animación aleatoria al iniciar")]
    public bool useRandomAnimation = true;
    
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
        
        // Configurar fondo aleatorio
        if (useRandomBackground && backgroundImage != null && backgroundSprites != null && backgroundSprites.Length > 0)
        {
            SetRandomBackground();
        }
        
        // Configurar animación aleatoria del personaje
        if (useRandomAnimation && characterAnimator != null)
        {
            SetRandomAnimation();
        }
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
    
    // Establece un fondo aleatorio
    private void SetRandomBackground()
    {
        if (backgroundSprites == null || backgroundSprites.Length == 0)
        {
            Debug.LogWarning("LoadingScreen: No hay sprites de fondo configurados");
            return;
        }
        
        // Seleccionar un sprite aleatorio
        int randomIndex = Random.Range(0, backgroundSprites.Length);
        Sprite selectedSprite = backgroundSprites[randomIndex];
        
        // Asignar al Image
        if (selectedSprite != null)
        {
            backgroundImage.sprite = selectedSprite;
            Debug.Log($"LoadingScreen: Fondo seleccionado = {selectedSprite.name}");
        }
    }
    
    // Establece una animación aleatoria del personaje
    private void SetRandomAnimation()
    {
        if (possibleStates == null || possibleStates.Length == 0)
        {
            Debug.LogWarning("LoadingScreen: No hay estados configurados");
            return;
        }
        
        // Seleccionar un estado aleatorio
        int randomIndex = Random.Range(0, possibleStates.Length);
        AnimationState selectedState = possibleStates[randomIndex];
        
        // Resetear todos los parámetros Bool primero
        foreach (AnimationState state in possibleStates)
        {
            if (state.isBoolParameter && !string.IsNullOrEmpty(state.boolParameterName))
            {
                if (HasParameter(characterAnimator, state.boolParameterName))
                {
                    characterAnimator.SetBool(state.boolParameterName, false);
                }
            }
        }
        
        // Establecer el estado seleccionado
        if (selectedState.isBoolParameter)
        {
            // Es un parámetro Bool (como "attack")
            if (HasParameter(characterAnimator, selectedState.boolParameterName))
            {
                characterAnimator.SetBool(selectedState.boolParameterName, true);
                Debug.Log($"LoadingScreen: Animación seleccionada = {selectedState.name} (bool {selectedState.boolParameterName})");
            }
        }
        else
        {
            // Es un parámetro de state (integer)
            if (HasParameter(characterAnimator, stateParameterName))
            {
                characterAnimator.SetInteger(stateParameterName, selectedState.stateValue);
                Debug.Log($"LoadingScreen: Animación seleccionada = {selectedState.name} (state {selectedState.stateValue})");
            }
        }
    }
    
    // Establece una animación específica por nombre
    public void SetSpecificAnimation(string animationName)
    {
        if (characterAnimator == null) return;
        
        // Resetear todos los parámetros Bool primero
        foreach (AnimationState state in possibleStates)
        {
            if (state.isBoolParameter && !string.IsNullOrEmpty(state.boolParameterName))
            {
                if (HasParameter(characterAnimator, state.boolParameterName))
                {
                    characterAnimator.SetBool(state.boolParameterName, false);
                }
            }
        }
        
        // Buscar el estado por nombre
        foreach (AnimationState state in possibleStates)
        {
            if (state.name.Equals(animationName, System.StringComparison.OrdinalIgnoreCase))
            {
                if (state.isBoolParameter)
                {
                    // Es un parámetro Bool
                    if (HasParameter(characterAnimator, state.boolParameterName))
                    {
                        characterAnimator.SetBool(state.boolParameterName, true);
                        Debug.Log($"LoadingScreen: Estableciendo animación {state.name} (bool {state.boolParameterName})");
                    }
                }
                else
                {
                    // Es un parámetro de state
                    if (HasParameter(characterAnimator, stateParameterName))
                    {
                        characterAnimator.SetInteger(stateParameterName, state.stateValue);
                        Debug.Log($"LoadingScreen: Estableciendo animación {state.name} (state {state.stateValue})");
                    }
                }
                return;
            }
        }
        
        Debug.LogWarning($"LoadingScreen: No se encontró el estado '{animationName}'");
    }
    
    // Establece una animación específica por valor de estado
    public void SetSpecificAnimationByValue(int stateValue)
    {
        if (characterAnimator == null) return;
        
        if (HasParameter(characterAnimator, stateParameterName))
        {
            characterAnimator.SetInteger(stateParameterName, stateValue);
            Debug.Log($"LoadingScreen: Estableciendo state = {stateValue}");
        }
    }
    
    // Verifica si el Animator tiene un parámetro específico
    private bool HasParameter(Animator animator, string parameterName)
    {
        if (animator == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == parameterName)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator LoadingRoutine()
    {
        // Esperar el tiempo mínimo
        yield return new WaitForSeconds(minimumDisplayTime);
        
        // Esperar a que el jugador esté listo
        yield return new WaitUntil(() => IsPlayerReady());
        
        // Pequeña pausa adicional para asegurar que todo esté listo
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
    
    // Actualizar el texto de carga manualmente
    public void SetLoadingText(string text)
    {
        if (loadingText != null)
        {
            baseText = text;
            loadingText.text = text;
        }
    }
}