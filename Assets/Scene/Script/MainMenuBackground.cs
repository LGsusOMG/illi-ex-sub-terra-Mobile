using UnityEngine;
using UnityEngine.UI;

public class MainMenuBackground : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Image del fondo del menú principal")]
    public Image backgroundImage;
    
    [Tooltip("Sprites de fondo disponibles (MainMenu1, MainMenu2, etc.)")]
    public Sprite[] backgroundSprites;
    
    [Tooltip("Cambiar fondo automáticamente al iniciar")]
    public bool changeOnStart = true;

    private void Start()
    {
        if (changeOnStart)
        {
            SetRandomBackground();
        }
    }
    
    public void SetRandomBackground()
    {
        if (backgroundImage == null)
        {
            Debug.LogError("MainMenuBackground: No hay Image asignado!");
            return;
        }
        
        if (backgroundSprites == null || backgroundSprites.Length == 0)
        {
            Debug.LogWarning("MainMenuBackground: No hay sprites de fondo configurados");
            return;
        }
        
        int randomIndex = Random.Range(0, backgroundSprites.Length);
        Sprite selectedSprite = backgroundSprites[randomIndex];
        
        if (selectedSprite != null)
        {
            backgroundImage.sprite = selectedSprite;
            Debug.Log($"MainMenuBackground: Fondo seleccionado = {selectedSprite.name}");
        }
        else
        {
            Debug.LogWarning($"MainMenuBackground: El sprite en índice {randomIndex} es null");
        }
    }
    
    public void SetBackgroundByIndex(int index)
    {
        if (backgroundImage == null || backgroundSprites == null) return;
        
        if (index >= 0 && index < backgroundSprites.Length)
        {
            backgroundImage.sprite = backgroundSprites[index];
        }
        else
        {
            Debug.LogWarning($"MainMenuBackground: Índice {index} fuera de rango");
        }
    }
}