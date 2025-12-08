using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour
{
    [Header("Button Action")]
    public ButtonAction action = ButtonAction.OpenInventory;
    
    public enum ButtonAction
    {
        OpenInventory,
        OpenTool,
        OpenMap,
        OpenJournal,
        Close
    }

    private void Start()
    {
        Button button = GetComponent<Button>();
        
        if (button == null)
        {
            Debug.LogError("InventoryButton: No hay componente Button en " + gameObject.name);
            return;
        }
        
        if (InventoryMenu.instance == null)
        {
            Debug.LogError("InventoryButton: InventoryMenu.instance es null!");
            return;
        }
        
        button.onClick.AddListener(OnButtonClick);
        Debug.Log("InventoryButton configurado en: " + gameObject.name);
    }
    
    private void OnButtonClick()
    {
        Debug.Log("InventoryButton: Click detectado! Action: " + action);
        
        switch (action)
        {
            case ButtonAction.OpenInventory:
                InventoryMenu.instance.OpenInventoryFromButton();
                break;
            case ButtonAction.OpenTool:
                InventoryMenu.instance.OpenToolFromButton();
                break;
            case ButtonAction.OpenMap:
                InventoryMenu.instance.OpenMapFromButton();
                break;
            case ButtonAction.OpenJournal:
                InventoryMenu.instance.OpenJournalFromButton();
                break;
            case ButtonAction.Close:
                InventoryMenu.instance.CloseMenuFromButton();
                break;
        }
    }
}