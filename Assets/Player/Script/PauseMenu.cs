using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private Player player;
    public GameObject pauseHolder;
    
    [Header("Botones Específicos")]
    public GameObject goToMenuButtonHolder;
    public GameObject resumeButtonHolder;
    
    private InputMaster inputMaster;
    private InputAction pauseMenuAction;

    private void Awake()
    {
        // Inicializar inputMaster en Awake para asegurar que esté listo
        inputMaster = new InputMaster();
    }

    private void Start()
    {
        player = GameObject.Find("Tenroh").GetComponent<Player>();
        
        // Asegurar que el menú esté desactivado al inicio
        if (pauseHolder != null)
        {
            pauseHolder.SetActive(false);
            Debug.Log("PauseMenu: Menu de pausa desactivado al inicio");
        }
    }

    private void OnEnable()
    {
        if (inputMaster != null)
        {
            inputMaster.Enable();
            // Configurar la acción de pausa después de habilitar inputMaster
            pauseMenuAction = inputMaster.Gameplay.Pause;
        }
    }

    private void OnDisable()
    {
        if (inputMaster != null)
            inputMaster.Disable();
    }

    private void Update()
    {
        bool pausePressed = false;
        
        // Verificar input de teclado/gamepad
        if (pauseMenuAction != null && pauseMenuAction.WasPressedThisFrame())
        {
            pausePressed = true;
        }
        
        // Verificar input móvil si está disponible
        if (player != null && player.useMobileControls)
        {
            MobileControls mobileControls = MobileControls.instance;
            if (mobileControls != null && mobileControls.WasPausePressedThisFrame())
            {
                pausePressed = true;
            }
        }
        
        // Pause
        if (pausePressed && !pauseHolder.activeSelf)
        {
            pauseHolder.SetActive(true);
            player.disableControlCounter += 1;
            Time.timeScale = 0f;
            
            // Configurar botones táctiles cuando se muestre el menú
            SetupTouchableButtons();
            Debug.Log("Menú de pausa activado");
        }
        // Unpause
        else if (pausePressed && pauseHolder.activeSelf)
        {
            pauseHolder.SetActive(false);
            player.disableControlCounter -= 1;
            Time.timeScale = 1f;
        }
    }
    
    /// <summary>
    /// Configura automáticamente los botones del menú de pausa como táctiles
    /// </summary>
    private void SetupTouchableButtons()
    {
        if (pauseHolder == null) 
        {
            Debug.LogWarning("PauseMenu: pauseHolder es null, no se pueden configurar botones táctiles");
            return;
        }
        
        if (!pauseHolder.activeSelf)
        {
            Debug.Log("PauseMenu: pauseHolder no está activo, omitiendo configuración táctil");
            return;
        }
        
        // Configurar botón específico de ir a menú
        SetupSpecificButton(goToMenuButtonHolder, "GoToMenu");
        
        // Configurar botón específico de reanudar
        SetupSpecificButton(resumeButtonHolder, "Resume");
        
        // Configurar otros botones en el pauseHolder
        UnityEngine.UI.Button[] buttons = pauseHolder.GetComponentsInChildren<UnityEngine.UI.Button>(false);
        
        Debug.Log($"PauseMenu: Encontrados {buttons.Length} botones adicionales para configurar como táctiles");
        
        foreach (UnityEngine.UI.Button button in buttons)
        {
            // Solo agregar TouchableUI si no lo tiene ya
            if (button.GetComponent<TouchableUI>() == null)
            {
                TouchableUI touchable = button.gameObject.AddComponent<TouchableUI>();
                touchable.SetupAsClickable();
                Debug.Log($"PauseMenu: Botón {button.name} configurado como táctil");
            }
            else
            {
                Debug.Log($"PauseMenu: Botón {button.name} ya tenía TouchableUI");
            }
        }
    }
    
    /// <summary>
    /// Configura un botón específico como táctil
    /// </summary>
    private void SetupSpecificButton(GameObject buttonHolder, string buttonType)
    {
        if (buttonHolder == null)
        {
            Debug.LogWarning($"PauseMenu: {buttonType} buttonHolder es null");
            return;
        }
        
        // Buscar el botón en el holder específico
        UnityEngine.UI.Button button = buttonHolder.GetComponent<UnityEngine.UI.Button>();
        if (button == null)
        {
            button = buttonHolder.GetComponentInChildren<UnityEngine.UI.Button>();
        }
        
        if (button != null)
        {
            // Solo agregar TouchableUI si no lo tiene ya
            if (button.GetComponent<TouchableUI>() == null)
            {
                TouchableUI touchable = button.gameObject.AddComponent<TouchableUI>();
                touchable.SetupAsClickable();
                Debug.Log($"PauseMenu: Botón {buttonType} ({button.name}) configurado como táctil");
            }
            else
            {
                Debug.Log($"PauseMenu: Botón {buttonType} ({button.name}) ya tenía TouchableUI");
            }
        }
        else
        {
            Debug.LogWarning($"PauseMenu: No se encontró Button component en {buttonType} buttonHolder");
        }
    }
    
    /// <summary>
    /// Método público para reanudar el juego (para usar en botones UI)
    /// </summary>
    public void ResumeGame()
    {
        if (pauseHolder.activeSelf)
        {
            pauseHolder.SetActive(false);
            player.disableControlCounter -= 1;
            Time.timeScale = 1f;
            Debug.Log("Juego reanudado desde botón");
        }
    }
    
    /// <summary>
    /// Método público para ir al menú principal (para usar en botones UI)
    /// </summary>
    public void GoToMainMenu()
    {
        // Restaurar timeScale antes de cambiar escena
        Time.timeScale = 1f;
        
        // Cargar escena del menú principal
        SceneManager.LoadScene("MainMenu"); // Cambia "MainMenu" por el nombre real de tu escena
        
        Debug.Log("Cargando menú principal desde botón");
    }
    
    /// <summary>
    /// Método público alternativo para salir del juego (para usar en botones UI)
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego desde botón");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}