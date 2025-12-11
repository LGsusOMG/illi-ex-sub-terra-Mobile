using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject selectFrame;
    public AudioSource selectSound;

    [Header("Referencias UI")]
    [Tooltip("Botón de continuar. Se desactiva si no hay partida.")]
    public Button continueButton;
    [Tooltip("Texto del botón continuar (opcional). Si no se asigna, se busca el hijo 0 del botón")]
    public TextMeshProUGUI continueText;

    private void Awake()
    {
        selectFrame.SetActive(false);

        // Configurar referencia a botón Continuar y su texto
        if (continueButton == null && transform.name == "Continue")
            continueButton = GetComponent<Button>();
        if (continueText == null && transform.name == "Continue" && transform.childCount > 0)
            continueText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (transform.name == "Continue")
        {
            bool hasSave = SaveSystem.CheckSaveExist();

            if (continueButton != null)
            {
                continueButton.interactable = hasSave;
            }

            if (continueText != null)
            {
                Color c = continueText.color;
                c.a = hasSave ? 1f : 0.2f;
                continueText.color = c;
            }

            if (hasSave)
            {
                EventSystem.current.firstSelectedGameObject = this.gameObject;
            }
        }
        
        // No necesitamos configuración adicional, el IPointerClickHandler manejará los toques
    }
    
    // Maneja cuando el mouse entra al botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Activar el frame de selección y sonido como si fuera seleccionado
        selectFrame.SetActive(true);
        selectSound.Play();
        
        // También seleccionar en el EventSystem para consistencia
        EventSystem.current.SetSelectedGameObject(this.gameObject);
        
        Debug.Log($"Mouse entró al botón: {transform.name}");
    }
    
    // Maneja cuando el mouse sale del botón
    public void OnPointerExit(PointerEventData eventData)
    {
        // Solo desactivar el frame si no estamos seleccionados por teclado
        // Y si no estamos en proceso de clic
        if (EventSystem.current.currentSelectedGameObject != this.gameObject)
        {
            selectFrame.SetActive(false);
        }
        
        Debug.Log($"Mouse salió del botón: {transform.name}");
    }
    
    // Maneja los toques/clics directamente
    public void OnPointerClick(PointerEventData eventData)
    {
        // Asegurarse de que el frame esté activo durante el clic
        selectFrame.SetActive(true);
        
        // Determinar qué botón es este basado en el nombre del GameObject
        string buttonName = transform.name;
        
        Debug.Log($"Botón táctil presionado: {buttonName}");
        
        // Intentar ejecutar primero el Button component (que probablemente está conectado a MainMenuController)
        Button button = GetComponent<Button>();
        if (button != null && button.interactable)
        {
            Debug.Log("Ejecutando Button.onClick...");
            button.onClick.Invoke();
        }
        else
        {
            // Si no hay Button component, usar nuestra lógica
            ExecuteButtonAction(buttonName);
        }
    }
    
    // Ejecuta la acción del botón basándose en su nombre
    private void ExecuteButtonAction(string buttonName)
    {
        // Normalizar el nombre para comparación
        string normalizedName = buttonName.ToLower().Trim();
        
        if (normalizedName.Contains("new") || normalizedName.Contains("nueva") || normalizedName.Contains("nuevo"))
        {
            Debug.Log("Ejecutando Nueva Partida");
            NewGameButton();
        }
        else if (normalizedName.Contains("continue") || normalizedName.Contains("continuar"))
        {
            Debug.Log("Ejecutando Continuar");
            ContinueButton();
        }
        else if (normalizedName.Contains("quit") || normalizedName.Contains("quitar") || normalizedName.Contains("exit") || normalizedName.Contains("salir"))
        {
            Debug.Log("Ejecutando Quitar");
            QuitButton();
        }
        else
        {
            Debug.Log($"Botón no reconocido: {buttonName}, intentando Button component");
            // Si no reconocemos el nombre, intentar ejecutar el Button component
            Button button = GetComponent<Button>();
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
            }
            else
            {
                Debug.LogWarning($"No se pudo ejecutar el botón: {buttonName}");
            }
        }
    }

    public void NewGameButton()
    {
        Debug.Log("MainMenu: Iniciando nueva partida...");
        
        // Asegurar que el tiempo esté normal
        Time.timeScale = 1f;
        
        // Limpiar el estado del LevelLoader antes de nueva partida
        if (LevelLoader.instance != null)
        {
            LevelLoader.instance.useCustomSpawnPosition = false;
            Debug.Log("MainMenu: LevelLoader reseteado para nueva partida");
        }
        
        SaveSystem.DeleteExistingSave();
        SceneManager.LoadScene("DirtCave0");
    }

    public void ContinueButton()
    {
        Debug.Log("MainMenu: Continuando partida guardada...");
        
        // Asegurar que el tiempo esté normal
        Time.timeScale = 1f;
        
        if (SaveSystem.CheckSaveExist())
        {
            // Limpiar el estado del LevelLoader antes de continuar
            if (LevelLoader.instance != null)
            {
                LevelLoader.instance.useCustomSpawnPosition = false;
                Debug.Log("MainMenu: LevelLoader reseteado para continuar partida");
            }
            
            SaveSystem.LoadPlayerData();
            SceneManager.LoadScene(GameMaster.instance.playerData.respawnScene);
        }
        else
        {
            Debug.LogWarning("MainMenu: No hay partida guardada para continuar");
        }
    }

    public void QuitButton()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void OnSelect(BaseEventData eventData)
    {
        selectFrame.SetActive(true);
        selectSound.Play();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selectFrame.SetActive(false);
    }
}
