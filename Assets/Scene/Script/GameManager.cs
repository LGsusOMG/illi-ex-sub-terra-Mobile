using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Estado del Juego")]
    public bool isPaused = false;

    [Header("Pantalla de Carga")]
    public GameObject loadingScreen;  // Tu panel con la imagen y animator

    public float minLoadingTime = 2f;

    // Control de escenas
    private string currentSceneName;
    private bool isChangingScene = false;
    private bool isLoading = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            currentSceneName = SceneManager.GetActiveScene().name;

            // Ocultar pantalla de carga al inicio
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ResetGameState();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Escena cargada: {scene.name}");
        currentSceneName = scene.name;
        isChangingScene = false;
        isLoading = false;

        // Ocultar pantalla de carga
        HideLoadingScreen();

        // LIMPIAR completamente el estado anterior
        ResetGameState();

        // Actualizar UI al cargar escena de gameplay
        if (IsGameplayScene(scene.name))
        {
            // Pequeño delay para asegurar que la escena esté completamente cargada
            Invoke(nameof(EnsureSingleEventSystem), 0.01f);
        }
    }

    // Método mejorado para manejar EventSystem
    private void EnsureSingleEventSystem()
    {
        UnityEngine.EventSystems.EventSystem[] eventSystems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);

        // Si hay más de un EventSystem, eliminar los extras
        if (eventSystems.Length > 1)
        {
            Debug.Log($"Encontrados {eventSystems.Length} EventSystems. Eliminando extras...");

            for (int i = 1; i < eventSystems.Length; i++)
            {
                Debug.Log($"Destruyendo EventSystem duplicado: {eventSystems[i].gameObject.name}");
                Destroy(eventSystems[i].gameObject);
            }
        }

        // Asegurar que al menos hay uno
        UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();

        if (eventSystem == null)
        {
            Debug.Log("Creando EventSystem nuevo");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        else
        {
            // Asegurar que esté activo y funcional
            if (!eventSystem.gameObject.activeInHierarchy)
                eventSystem.gameObject.SetActive(true);
            if (!eventSystem.enabled)
                eventSystem.enabled = true;

            // Asegurar los módulos de input
            UnityEngine.EventSystems.StandaloneInputModule inputModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (inputModule == null)
            {
                eventSystem.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            else if (!inputModule.enabled)
            {
                inputModule.enabled = true;
            }
        }

        Debug.Log($"EventSystem verificado: {eventSystem.gameObject.name}");
    }

    private void ResetGameState()
    {
        Time.timeScale = 1f;
        isPaused = false;

        

        Debug.Log("Estado del juego reseteado - Time.timeScale: " + Time.timeScale);
    }

    private bool IsGameplayScene(string sceneName)
    {
        return sceneName.StartsWith("Level_") || !sceneName.Contains("Menu");
    }

    private void Update()
    {
        // Solo procesar input si no estamos cambiando de escena
        if (isChangingScene || isLoading) return;

        if (IsKeyDown(KeyCode.Escape))
        {
            if (IsGameplayScene(currentSceneName))
            {
                TogglePause();
            }
        }
    }

    private bool IsKeyDown(KeyCode key)
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(key);
#else
        if (Keyboard.current == null) return false;
        switch (key)
        {
            case KeyCode.Escape:
                return Keyboard.current.escapeKey.wasPressedThisFrame;
            default:
                return false;
        }
#endif
    }

    // CORRUTINA PRINCIPAL DE CARGA
    private IEnumerator LoadSceneWithLoadingScreen(string sceneName)
    {
        isChangingScene = true;
        isLoading = true;
        ResetGameState();

        // MOSTRAR pantalla de carga (tu panel con animación)
        ShowLoadingScreen();

        float loadingStartTime = Time.time;

        // Cargar la escena de forma asíncrona
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Actualizar progreso (si tienes barra de progreso)
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Cuando la carga esté casi completa
            if (asyncLoad.progress >= 0.9f)
            {
                // Esperar el tiempo mínimo restante
                float elapsedTime = Time.time - loadingStartTime;
                float remainingTime = Mathf.Max(0, minLoadingTime - elapsedTime);

                if (remainingTime > 0)
                {
                    yield return new WaitForSeconds(remainingTime);
                }

                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // La pantalla se oculta automáticamente en OnSceneLoaded
    }

    // MÉTODOS PARA LA PANTALLA DE CARGA
    private void ShowLoadingScreen()
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }
    }

    private void HideLoadingScreen()
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
    }

    // MÉTODOS DE CAMBIO DE ESCENA (ACTUALIZADOS)
    public void LoadLevel(int levelNumber)
    {
        if (isChangingScene || isLoading) return;

        string sceneName = $"Level_{levelNumber}";
        StartCoroutine(LoadSceneWithLoadingScreen(sceneName));
    }

    public void GoToMenu()
    {
        Debug.Log("GoToMenu llamado");

        if (isChangingScene || isLoading) return;

        StartCoroutine(LoadSceneWithLoadingScreen("MainMenu"));
    }

    public void RestartGame()
    {
        if (isChangingScene || isLoading) return;

        StartCoroutine(LoadSceneWithLoadingScreen(SceneManager.GetActiveScene().name));
    }

    // Sistema de pausa simplificado
    public void TogglePause()
    {
        if (!IsGameplayScene(currentSceneName) || isLoading) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    private int GetCurrentLevelNumber()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Level_"))
        {
            string levelStr = sceneName.Replace("Level_", "");
            int levelNumber;
            if (int.TryParse(levelStr, out levelNumber))
            {
                return levelNumber;
            }
        }
        return 1;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // NUEVO: Método para verificar si se puede interactuar
    public bool CanInteract()
    {
        return !isPaused && !isChangingScene && !isLoading;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadNextLevel()
    {
        int currentLevel = GetCurrentLevelNumber();
        int nextLevel = currentLevel + 1;
        LoadLevel(nextLevel);
    }
}