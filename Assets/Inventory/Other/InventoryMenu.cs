using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMenu : MonoBehaviour
{
    private Player player;
    public GameObject inventoryHolder;
    public GameObject[] inventoryMenuTabs;
    private CanvasGroup canvasGroup;
    public GameObject blockingPanel;
    
    private List<UnityEngine.UI.GraphicRaycaster> disabledRaycasters = new List<UnityEngine.UI.GraphicRaycaster>(); // Panel transparente que bloquea clics

    public AudioSource openInventorySound;
    public AudioSource closeInventorySound;
    public AudioSource movingButtonSound;
    public AudioSource pressedButtonSound;

    private int currentTabIndex;

    public static InventoryMenu instance;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            // NO usar DontDestroyOnLoad en el UI
            // El UI debe permanecer en la escena
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        player = GameObject.Find("Tenroh")?.GetComponent<Player>();

        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
            Debug.LogWarning("InventoryMenu: Jugador no encontrado por nombre, usando FindFirstObjectByType");
        }

        if (player == null)
        {
            Debug.LogError("InventoryMenu: ¡NO SE ENCONTRÓ AL JUGADOR!");
            return;
        }
        
        // Obtener o agregar CanvasGroup al inventoryHolder
        canvasGroup = inventoryHolder.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = inventoryHolder.AddComponent<CanvasGroup>();
            Debug.Log("InventoryMenu: CanvasGroup agregado automáticamente al inventoryHolder");
        }
        
        // Configurar el panel bloqueador si existe
        if (blockingPanel != null)
        {
            blockingPanel.SetActive(false);
        }
        
        inventoryHolder.SetActive(false);
    }

    private void Update()
    {
        if (player == null)
        {
            Debug.LogWarning("InventoryMenu: ¡El jugador es nulo!");
            return;
        }

        // Cerrar menú si el jugador está herido
        if (player.isHurt && player.inMenu)
        {
            CloseMenu();
        }
    }

    // ========== MÉTODOS PÚBLICOS PARA BOTONES ==========
    
    public void OpenInventoryFromButton()
    {
        if (player == null || player.disableInventoryMenu) return;
        OpenInventoryTab(0);
        Debug.Log("InventoryMenu: Abriendo Inventario desde botón");
    }

    public void OpenToolFromButton()
    {
        if (player == null || player.disableInventoryMenu) return;
        OpenInventoryTab(1);
        Debug.Log("InventoryMenu: Abriendo Talismanes desde botón");
    }

    public void OpenMapFromButton()
    {
        if (player == null || player.disableInventoryMenu) return;
        OpenInventoryTab(2);
        Debug.Log("InventoryMenu: Abriendo Mapa desde botón");
    }

    public void OpenJournalFromButton()
    {
        if (player == null || player.disableInventoryMenu) return;
        OpenInventoryTab(3);
        Debug.Log("InventoryMenu: Abriendo Diario desde botón");
    }

    public void CloseMenuFromButton()
    {
        if (player == null || !player.inMenu) return;
        CloseMenu();
        Debug.Log("InventoryMenu: Cerrando menú desde botón");
    }

    // ========== MÉTODOS INTERNOS ==========

    private void OpenInventoryTab(int tabIndex)
    {
        // Si el menú ya está abierto, solo cambia el tab
        bool menuAlreadyOpen = player.inMenu;
        
        if (!menuAlreadyOpen)
        {
            if (openInventorySound != null)
                openInventorySound.Play();
            
            // Activar panel bloqueador primero
            if (blockingPanel != null)
            {
                blockingPanel.SetActive(true);
                blockingPanel.transform.SetAsFirstSibling();
            }
            
            inventoryHolder.SetActive(true);
            inventoryHolder.transform.SetAsLastSibling();
            
            // Configurar CanvasGroup para bloquear interacciones
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
            
            player.inMenu = true;
            
            // Solo deshabilitar raycasters cuando abrimos el menú por primera vez
            DisableSceneCanvasRaycasters();
        }
        else
        {
            Debug.Log("InventoryMenu: Cambiando de tab sin deshabilitar raycasters");
        }
        
        currentTabIndex = tabIndex;
        UpdateInventoryMenuTab();
    }

    public void CloseMenu()
    {
        if (closeInventorySound != null)
            closeInventorySound.Play();
        
        // Desactivar CanvasGroup antes de ocultar
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        
        inventoryHolder.SetActive(false);
        
        // Desactivar panel bloqueador
        if (blockingPanel != null)
        {
            blockingPanel.SetActive(false);
        }
        
        player.inMenu = false;
        
        // Rehabilitar otros Canvas Raycasters
        EnableSceneCanvasRaycasters();
        
        Debug.Log("InventoryMenu: Menú cerrado completamente");
    }

    private void DisableSceneCanvasRaycasters()
    {
        disabledRaycasters.Clear();
        
        // Obtener el GraphicRaycaster del inventario
        UnityEngine.UI.GraphicRaycaster inventoryRaycaster = GetComponent<UnityEngine.UI.GraphicRaycaster>();
        
        if (inventoryRaycaster == null)
        {
            Debug.LogWarning("InventoryMenu: No se encontró GraphicRaycaster en InventoryMenu. Agregando uno...");
            inventoryRaycaster = gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Deshabilitar todos los GraphicRaycasters excepto el del inventario
        UnityEngine.UI.GraphicRaycaster[] raycasters = FindObjectsByType<UnityEngine.UI.GraphicRaycaster>(FindObjectsSortMode.None);
        foreach (var raycaster in raycasters)
        {
            // No deshabilitar el raycaster del inventario
            if (raycaster == inventoryRaycaster)
            {
                Debug.Log("InventoryMenu: Manteniendo raycaster del inventario activo");
                continue;
            }
            
            if (raycaster.enabled)
            {
                raycaster.enabled = false;
                disabledRaycasters.Add(raycaster);
            }
        }
        
        Debug.Log($"InventoryMenu: {disabledRaycasters.Count} raycasters deshabilitados");
    }

    private void EnableSceneCanvasRaycasters()
    {
        Debug.Log($"InventoryMenu: Intentando rehabilitar {disabledRaycasters.Count} raycasters");
        
        // Rehabilitar solo los que deshabilitamos
        int rehabilitados = 0;
        foreach (var raycaster in disabledRaycasters)
        {
            if (raycaster != null)
            {
                raycaster.enabled = true;
                rehabilitados++;
                Debug.Log($"InventoryMenu: Raycaster rehabilitado: {raycaster.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("InventoryMenu: Encontrado raycaster null en la lista");
            }
        }
        
        disabledRaycasters.Clear();
        Debug.Log($"InventoryMenu: {rehabilitados} raycasters rehabilitados exitosamente");
    }

    private void UpdateInventoryMenuTab()
    {
        Debug.Log($"InventoryMenu: Actualizando tabs. CurrentTabIndex = {currentTabIndex}, Total tabs = {inventoryMenuTabs.Length}");
        
        for (int i = 0; i < inventoryMenuTabs.Length; i++)
        {
            if (inventoryMenuTabs[i] == null)
            {
                Debug.LogWarning($"InventoryMenu: Tab {i} es null!");
                continue;
            }
            
            bool shouldBeActive = (i == currentTabIndex);
            inventoryMenuTabs[i].SetActive(shouldBeActive);
            Debug.Log($"InventoryMenu: Tab {i} ({inventoryMenuTabs[i].name}) = {shouldBeActive}");
        }
    }
}