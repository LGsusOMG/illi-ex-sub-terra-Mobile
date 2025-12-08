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

    private void Awake()
    {
        selectFrame.SetActive(false);
    }

    private void Start()
    {
        if (transform.name == "Continue")
        {
            TextMeshProUGUI continueText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (!SaveSystem.CheckSaveExist())
            {
                Color noSaveColor = continueText.color;
                noSaveColor.a = 0.2f;
                continueText.color = noSaveColor;
            }
            else
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
        
        //SaveSystem.DeleteExistingSave();
        SceneManager.LoadScene("DirtCave0");
    }

    public void ContinueButton()
    {
        Debug.Log("MainMenu: Continuando partida guardada...");
        
        // Asegurar que el tiempo esté normal
        Time.timeScale = 1f;
        
        if (SaveSystem.CheckSaveExist())
        {
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
