using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Utilidad para convertir automáticamente elementos UI existentes en táctiles
/// para dispositivos móviles. Se ejecuta automáticamente al inicio.
/// </summary>
public class TouchableUIConverter : MonoBehaviour
{
    [Header("Auto-Conversion Settings")]
    public bool convertOnStart = true;
    public bool includeInactiveObjects = false;
    
    [Header("Conversion Targets")]
    public bool convertButtons = true;
    public bool convertSelectables = true;
    public bool convertTabTexts = true;
    
    [Header("Tab Detection")]
    public string[] tabKeywords = { "tab", "Tab", "TAB", "pestana", "pestaña" };
    public string[] leftTabKeywords = { "left", "Left", "izquierda", "anterior", "prev" };
    public string[] rightTabKeywords = { "right", "Right", "derecha", "siguiente", "next" };
    
    private void Start()
    {
        if (convertOnStart)
        {
            ConvertUIElements();
        }
    }
    
    /// <summary>
    /// Convierte todos los elementos UI encontrados en el Canvas a táctiles
    /// </summary>
    public void ConvertUIElements()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("TouchableUIConverter: No Canvas encontrado en " + gameObject.name);
            return;
        }
        
        Debug.Log("Iniciando conversión de elementos UI a táctiles en " + gameObject.name);
        
        // Obtener todos los elementos UI
        GameObject[] allObjects = includeInactiveObjects ? 
            Resources.FindObjectsOfTypeAll<GameObject>() : 
            FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            
        int convertedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Solo procesar objetos que estén en este Canvas
            if (!IsInThisCanvas(obj, canvas))
                continue;
                
            // Solo procesar si no tiene TouchableUI ya
            if (obj.GetComponent<TouchableUI>() != null)
                continue;
                
            if (TryConvertElement(obj))
            {
                convertedCount++;
            }
        }
        
        Debug.Log($"TouchableUIConverter: {convertedCount} elementos convertidos a táctiles");
    }
    
    private bool IsInThisCanvas(GameObject obj, Canvas canvas)
    {
        Transform parent = obj.transform;
        while (parent != null)
        {
            if (parent == canvas.transform)
                return true;
            parent = parent.parent;
        }
        return false;
    }
    
    private bool TryConvertElement(GameObject obj)
    {
        bool converted = false;
        
        // Convertir Buttons
        if (convertButtons && obj.GetComponent<Button>() != null)
        {
            TouchableUI touchable = obj.AddComponent<TouchableUI>();
            touchable.SetupAsClickable();
            Debug.Log($"Convertido Button: {obj.name}");
            converted = true;
        }
        // Convertir otros Selectables
        else if (convertSelectables && obj.GetComponent<Selectable>() != null)
        {
            TouchableUI touchable = obj.AddComponent<TouchableUI>();
            touchable.SetupAsSelectable();
            Debug.Log($"Convertido Selectable: {obj.name}");
            converted = true;
        }
        // Convertir textos que parecen tabs
        else if (convertTabTexts && IsTabElement(obj))
        {
            TouchableUI.TabDirection direction = GetTabDirection(obj);
            if (direction != TouchableUI.TabDirection.None)
            {
                TouchableUI touchable = obj.AddComponent<TouchableUI>();
                touchable.SetupAsTab(direction);
                Debug.Log($"Convertido Tab {direction}: {obj.name}");
                converted = true;
            }
        }
        
        return converted;
    }
    
    private bool IsTabElement(GameObject obj)
    {
        string objName = obj.name.ToLower();
        
        foreach (string keyword in tabKeywords)
        {
            if (objName.Contains(keyword.ToLower()))
                return true;
        }
        
        // También revisar si tiene componente Text o TextMeshPro
        return obj.GetComponent<Text>() != null || obj.GetComponent<TMPro.TextMeshProUGUI>() != null;
    }
    
    private TouchableUI.TabDirection GetTabDirection(GameObject obj)
    {
        string objName = obj.name.ToLower();
        
        // Verificar left/izquierda
        foreach (string keyword in leftTabKeywords)
        {
            if (objName.Contains(keyword.ToLower()))
                return TouchableUI.TabDirection.Left;
        }
        
        // Verificar right/derecha
        foreach (string keyword in rightTabKeywords)
        {
            if (objName.Contains(keyword.ToLower()))
                return TouchableUI.TabDirection.Right;
        }
        
        return TouchableUI.TabDirection.None;
    }
    
    /// <summary>
    /// Método público para convertir un elemento específico
    /// </summary>
    public bool ConvertSpecificElement(GameObject obj, TouchableUI.TouchAction action, TouchableUI.TabDirection tabDirection = TouchableUI.TabDirection.None)
    {
        if (obj.GetComponent<TouchableUI>() != null)
        {
            Debug.LogWarning($"El objeto {obj.name} ya tiene TouchableUI");
            return false;
        }
        
        TouchableUI touchable = obj.AddComponent<TouchableUI>();
        
        switch (action)
        {
            case TouchableUI.TouchAction.Click:
                touchable.SetupAsClickable();
                break;
            case TouchableUI.TouchAction.Select:
                touchable.SetupAsSelectable();
                break;
            case TouchableUI.TouchAction.TabNavigation:
                touchable.SetupAsTab(tabDirection);
                break;
        }
        
        Debug.Log($"Convertido manualmente: {obj.name} como {action}");
        return true;
    }
}