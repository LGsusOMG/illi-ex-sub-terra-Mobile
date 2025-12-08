using System.Collections;
using UnityEngine;

/// <summary>
/// Manager para controlar la pantalla de carga desde cualquier lugar
/// </summary>
public class LoadingManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Prefab de la pantalla de carga")]
    public GameObject loadingScreenPrefab;
    
    private static LoadingManager instance;
    private LoadingScreen currentLoadingScreen;
    
    public static LoadingManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("LoadingManager");
                instance = go.AddComponent<LoadingManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // Mostrar pantalla de carga simple
    public void ShowLoadingScreen(string text = "Cargando", string specificAnimation = "")
    {
        if (loadingScreenPrefab == null)
        {
            Debug.LogError("LoadingManager: No hay prefab asignado!");
            return;
        }
        
        // Destruir pantalla anterior si existe
        if (currentLoadingScreen != null)
        {
            Destroy(currentLoadingScreen.gameObject);
        }
        
        // Instanciar nueva pantalla
        GameObject loadingObj = Instantiate(loadingScreenPrefab);
        currentLoadingScreen = loadingObj.GetComponent<LoadingScreen>();
        
        if (currentLoadingScreen != null)
        {
            currentLoadingScreen.SetLoadingText(text);
            
            // Si se especifica una animación, usarla en lugar de aleatoria
            if (!string.IsNullOrEmpty(specificAnimation))
            {
                currentLoadingScreen.SetSpecificAnimation(specificAnimation);
            }
        }
    }
    
    // Ocultar la pantalla de carga actual
    public void HideLoadingScreen()
    {
        if (currentLoadingScreen != null)
        {
            currentLoadingScreen.HideLoadingScreen();
            currentLoadingScreen = null;
        }
    }
    
    // Mostrar pantalla de carga por tiempo específico
    public void ShowLoadingScreenForDuration(float duration, string text = "Cargando", string specificAnimation = "")
    {
        StartCoroutine(ShowForDurationRoutine(duration, text, specificAnimation));
    }
    
    private IEnumerator ShowForDurationRoutine(float duration, string text, string specificAnimation)
    {
        ShowLoadingScreen(text, specificAnimation);
        yield return new WaitForSeconds(duration);
        HideLoadingScreen();
    }
    
    // ========================================
    // MÉTODOS PARA BOTONES DE UI
    // ========================================
    
    // Botón Nueva Partida - Muestra pantalla y carga el nivel inicial
    public void NuevaPartida()
    {
        StartCoroutine(CargarNivelConPantalla("DirtCave0", "Iniciando aventura..."));
    }
    
    // Botón Continuar - Muestra pantalla y carga el nivel guardado
    public void Continuar()
    {
        // Cargar los datos guardados primero
        if (SaveSystem.CheckSaveExist())
        {
            SaveSystem.LoadPlayerData();
            // Usar la escena guardada en respawnScene
            string sceneName = GameMaster.instance.playerData.respawnScene;
            StartCoroutine(CargarNivelConPantalla(sceneName, "Continuando..."));
        }
        else
        {
            Debug.LogWarning("LoadingManager: No hay partida guardada para continuar");
        }
    }

    // Volver al MainMenu - Muestra pantalla de carga
    public void VolverAlMainMenu()
    {
        StartCoroutine(CargarNivelConPantalla("MainMenu", "Volviendo al menú..."));
    }
    
    private IEnumerator CargarNivelConPantalla(string sceneName, string loadingText)
    {
        // 1. Mostrar pantalla de carga
        ShowLoadingScreen(loadingText);
        
        // 2. Esperar un frame para que se muestre
        yield return null;
        
        // 3. Cargar la escena
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}