using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelUIManager : MonoBehaviour
{
    [Header("Botones")]
    public GameObject botonPausa;
    [Header("UI Panels")]
    public GameObject pausePanel;

    // Singleton para acceso desde MobileControls
    public static LevelUIManager instance;
    private bool isPaused = false;

    private void Awake()
    {
        // Configurar singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Buscar paneles automáticamente si no están asignados
        FindUIPanelsIfNeeded();

        // Asegurar que los paneles estén desactivados al inicio
        if (pausePanel != null) pausePanel.SetActive(false);

        // Configurar botones
        SetupButtonListeners();
    }

    private void FindUIPanelsIfNeeded()
    {
        // Buscar panel de pausa
        if (pausePanel == null)
        {
            // Buscar primero en el nivel raíz
            pausePanel = GameObject.Find("PausePanel 1");
            if (pausePanel == null)
                pausePanel = GameObject.Find("Pause Panel");
            if (pausePanel == null)
                pausePanel = GameObject.Find("Panel de Pausa");
            if (pausePanel == null)
                pausePanel = GameObject.Find("PAUSE");

            // Si no se encuentra, buscar recursivamente en toda la jerarquía (incluyendo dentro de Canvas)
            if (pausePanel == null)
            {
                GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "PausePanel 1" || obj.name == "Pause Panel" || 
                        obj.name == "Panel de Pausa" || obj.name == "PAUSE")
                    {
                        pausePanel = obj;
                        break;
                    }
                }
            }

            if (pausePanel != null)
                Debug.Log($"LevelUIManager: Panel de pausa encontrado automáticamente: {pausePanel.name}");
            else
                Debug.LogWarning("LevelUIManager: No se encontró el panel de pausa. Asígnalo manualmente en el Inspector.");
        }


        // Buscar botón de pausa
        if (botonPausa == null)
        {
            // Buscar primero en el nivel raíz
            botonPausa = GameObject.Find("Pausa");
            if (botonPausa == null)
                botonPausa = GameObject.Find("BotonPausa");
            if (botonPausa == null)
                botonPausa = GameObject.Find("PauseButton");
            if (botonPausa == null)
                botonPausa = GameObject.Find("Pause Button");

            // Si no se encuentra, buscar recursivamente en toda la jerarquía
            if (botonPausa == null)
            {
                GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "Pausa" || obj.name == "BotonPausa" || 
                        obj.name == "PauseButton" || obj.name == "Pause Button")
                    {
                        botonPausa = obj;
                        break;
                    }
                }
            }

            if (botonPausa != null)
                Debug.Log($"LevelUIManager: Botón de pausa encontrado automáticamente: {botonPausa.name}");
        }
    }

    private void SetupButtonListeners()
    {
        // Configurar botón de pausa
        if (botonPausa != null)
        {
            Button pauseButton = botonPausa.GetComponent<Button>();
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(PauseGame);
            }
        }

        // Configurar botones del panel de pausa
        if (pausePanel != null)
        {
            Button[] buttons = pausePanel.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                if (button.name.Contains("REANUDAR"))
                {
                    button.onClick.AddListener(ResumeGame);
                }
                else if (button.name.Contains("REINICIAR"))
                {
                    button.onClick.AddListener(RestartGame);
                }
                else if (button.name.Contains("MENU"))
                {
                    button.onClick.AddListener(GoToMenu);
                }
                else if (button.name.Contains("SALIR"))
                {
                    button.onClick.AddListener(QuitGame);
                }
            }
        }
    }

    public void ResumeGame()
    {
        Debug.Log("LevelUIManager: Reanudando juego");
        Time.timeScale = 1f; // Reanudar tiempo
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        isPaused = false;
    }

    public void PauseGame()
    {
        Debug.Log("LevelUIManager: Pausando juego");
        Time.timeScale = 0f; // Pausar tiempo
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        isPaused = true;
    }

    // Método público para toggle pausa - llamado desde MobileButton
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }

        Debug.Log($"LevelUIManager: Pausa toggled - isPaused: {isPaused}");
    }

    // Método público para pausar desde controles móviles
    public void PauseFromMobile()
    {
        if (!isPaused)
        {
            isPaused = true;
            PauseGame();
            Debug.Log("LevelUIManager: Juego pausado desde móvil");
        }
    }

    // Método público para reanudar desde controles móviles
    public void ResumeFromMobile()
    {
        if (isPaused)
        {
            isPaused = false;
            ResumeGame();
            Debug.Log("LevelUIManager: Juego reanudado desde móvil");
        }
    }

    // Métodos para manejar acciones de botones
    public void RestartGame()
    {
        Debug.Log("Reiniciando juego...");
        Time.timeScale = 1f; // Asegurar que el tiempo esté normal
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    public void GoToMenu()
    {
        Debug.Log("Yendo al menú principal...");
        Time.timeScale = 1f; // Asegurar que el tiempo esté normal

        // Limpiar singleton antes de cambiar escena
        if (instance == this)
        {
            instance = null;
        }

        // Usar LoadingManager si está disponible
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.VolverAlMainMenu();
        }
        else
        {
            // Fallback: cargar directamente sin pantalla de carga
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }
}