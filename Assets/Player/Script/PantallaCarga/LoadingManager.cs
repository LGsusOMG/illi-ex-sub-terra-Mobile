using System.Collections;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Prefab de la pantalla de carga")]
    public GameObject loadingScreenPrefab;
    
    [Header("Textos Personalizados")]
    public string textoNuevaPartida = "Iniciando viaje...";
    public string textoContinuar = "Continuando aventura...";
    public string textoVolverMenu = "Volviendo al menú...";
    public string textoCargando = "Cargando...";
    
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
    
    // Mostrar pantalla de carga con texto personalizado
    public void ShowLoadingScreen(string text = "Cargando")
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
    public void ShowLoadingScreenForDuration(float duration, string text = "Cargando")
    {
        StartCoroutine(ShowForDurationRoutine(duration, text));
    }
    
    private IEnumerator ShowForDurationRoutine(float duration, string text)
    {
        ShowLoadingScreen(text);
        yield return new WaitForSeconds(duration);
        HideLoadingScreen();
    }
    
    // ========================================
    // MÉTODOS PARA BOTONES DE UI
    // ========================================
    
    // Botón Nueva Partida
    public void NuevaPartida()
    {
        StartCoroutine(CargarNivelConPantalla("DirtCave0", textoNuevaPartida));
    }
    
    // Botón Continuar
    public void Continuar()
    {
        if (SaveSystem.CheckSaveExist())
        {
            SaveSystem.LoadPlayerData();
            string sceneName = GameMaster.instance.playerData.respawnScene;
            StartCoroutine(CargarNivelConPantalla(sceneName, textoContinuar));
        }
        else
        {
            Debug.LogWarning("LoadingManager: No hay partida guardada");
        }
    }

    // Volver al MainMenu
    public void VolverAlMainMenu()
    {
        StartCoroutine(CargarNivelConPantalla("MainMenu", textoVolverMenu));
    }
    
    // Método genérico para cargar cualquier nivel con texto personalizado
    public void CargarNivel(string sceneName, string textoPersonalizado = "")
    {
        string texto = string.IsNullOrEmpty(textoPersonalizado) ? textoCargando : textoPersonalizado;
        StartCoroutine(CargarNivelConPantalla(sceneName, texto));
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