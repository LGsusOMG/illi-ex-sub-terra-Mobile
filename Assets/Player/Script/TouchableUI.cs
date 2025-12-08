using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Componente universal para hacer elementos UI táctiles en dispositivos móviles.
/// Convierte elementos UI existentes (que funcionan con teclado/gamepad) en táctiles
/// sin afectar su funcionalidad original.
/// </summary>
public class TouchableUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Touch Configuration")]
    public TouchAction touchAction = TouchAction.Click;
    
    [Header("Tab Navigation (solo si es tab)")]
    public TabDirection tabDirection = TabDirection.None;
    
    [Header("Visual Feedback")]
    public bool enableVisualFeedback = true;
    public float pressScale = 0.95f;
    public Color pressedTint = new Color(0.8f, 0.8f, 0.8f, 1f);
    
    private Vector3 originalScale;
    private Color originalColor;
    private Image imageComponent;
    private Button buttonComponent;
    private bool isPressed = false;
    
    public enum TouchAction
    {
        Click,          // Simula un click normal (para botones, etc.)
        Select,         // Simula selección (para elementos navegables)
        TabNavigation   // Para navegación entre tabs
    }
    
    public enum TabDirection
    {
        None,
        Left,
        Right
    }
    
    private void Start()
    {
        originalScale = transform.localScale;
        
        imageComponent = GetComponent<Image>();
        if (imageComponent != null)
            originalColor = imageComponent.color;
            
        buttonComponent = GetComponent<Button>();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Solo procesar si estamos usando controles móviles
        Player player = FindFirstObjectByType<Player>();
        if (player == null || !player.useMobileControls)
            return;
            
        switch (touchAction)
        {
            case TouchAction.Click:
                HandleClick();
                break;
            case TouchAction.Select:
                HandleSelect();
                break;
            case TouchAction.TabNavigation:
                HandleTabNavigation();
                break;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // Solo procesar si estamos usando controles móviles
        Player player = FindFirstObjectByType<Player>();
        if (player == null || !player.useMobileControls)
            return;
            
        if (enableVisualFeedback && !isPressed)
        {
            isPressed = true;
            
            // Escalar
            transform.localScale = originalScale * pressScale;
            
            // Cambiar color si hay Image
            if (imageComponent != null)
                imageComponent.color = originalColor * pressedTint;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        // Solo procesar si estamos usando controles móviles
        Player player = FindFirstObjectByType<Player>();
        if (player == null || !player.useMobileControls)
            return;
            
        if (enableVisualFeedback && isPressed)
        {
            isPressed = false;
            
            // Restaurar escala
            transform.localScale = originalScale;
            
            // Restaurar color
            if (imageComponent != null)
                imageComponent.color = originalColor;
        }
    }
    
    private void HandleClick()
    {
        // Si hay un Button component, ejecutar su onClick
        if (buttonComponent != null && buttonComponent.interactable)
        {
            buttonComponent.onClick.Invoke();
        }
        
        // Si no hay Button pero es un Selectable, simular selección
        Selectable selectable = GetComponent<Selectable>();
        if (selectable != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
    
    private void HandleSelect()
    {
        // Simular selección en el EventSystem
        EventSystem.current.SetSelectedGameObject(gameObject);
        
        // Si tiene ISelectHandler, llamarlo
        ISelectHandler selectHandler = GetComponent<ISelectHandler>();
        if (selectHandler != null)
        {
            selectHandler.OnSelect(new BaseEventData(EventSystem.current));
        }
    }
    
    private void HandleTabNavigation()
    {
        MobileControls mobileControls = MobileControls.instance;
        if (mobileControls == null)
            return;
            
        switch (tabDirection)
        {
            case TabDirection.Left:
                mobileControls.OnTabLeftPressed();
                break;
            case TabDirection.Right:
                mobileControls.OnTabRightPressed();
                break;
        }
    }
    
    /// <summary>
    /// Método público para configurar este TouchableUI para navegación de tabs
    /// </summary>
    public void SetupAsTab(TabDirection direction)
    {
        touchAction = TouchAction.TabNavigation;
        tabDirection = direction;
    }
    
    /// <summary>
    /// Método público para configurar este TouchableUI para clicks simples
    /// </summary>
    public void SetupAsClickable()
    {
        touchAction = TouchAction.Click;
        tabDirection = TabDirection.None;
    }
    
    /// <summary>
    /// Método público para configurar este TouchableUI para selección
    /// </summary>
    public void SetupAsSelectable()
    {
        touchAction = TouchAction.Select;
        tabDirection = TabDirection.None;
    }
}