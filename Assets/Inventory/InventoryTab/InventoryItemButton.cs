using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryItemButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject selectFrame;
    public InventoryItem itemData;
    private Button button;
    private static InventoryItemButton currentlySelected;
    private bool isHovering = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
            Debug.Log("InventoryItemButton: Componente Button agregado a " + gameObject.name);
        }
        
        // IMPORTANTE: Activar Raycast Target en la imagen para que detecte clics
        Image itemImage = GetComponent<Image>();
        if (itemImage != null && !itemImage.raycastTarget)
        {
            itemImage.raycastTarget = true;
            Debug.Log($"InventoryItemButton: Raycast Target activado en {gameObject.name}");
        }
    }

    private void OnEnable()
    {
        selectFrame = transform.Find("SelectFrame")?.gameObject;
        
        if (selectFrame == null)
        {
            Debug.LogWarning("InventoryItemButton: No se encontró 'SelectFrame' en " + gameObject.name);
        }
        else
        {
            selectFrame.SetActive(false);
        }

        // Update amount / quantity text
        if (transform.Find("Amount"))
        {
            TextMeshProUGUI amountText = transform.Find("Amount").GetComponent<TextMeshProUGUI>();
            int amount = GameMaster.instance.playerData.inventoryItemAmount[(int)itemData.thisItemName];
            if (amount > 1)
            {
                amountText.gameObject.SetActive(true);
                amountText.text = amount.ToString();
            }
            else
            {
                amountText.gameObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        if (selectFrame != null)
            selectFrame.SetActive(false);
        
        if (currentlySelected == this)
        {
            currentlySelected = null;
            ClearInfoBox();
        }
        
        isHovering = false;
    }

    // MÓVIL & PC: Cuando se toca/hace clic en el botón
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"InventoryItemButton: Click detectado en {gameObject.name}");
        SelectItem(true);
    }

    // PC: Cuando el mouse entra al botón (hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Solo en PC/mouse - En móvil esto no se activa normalmente
        if (!Application.isMobilePlatform || Input.mousePresent)
        {
            isHovering = true;
            ShowItemInfo(false);
        }
    }

    // PC: Cuando el mouse sale del botón
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        
        // Solo ocultar si este NO es el item seleccionado
        if (currentlySelected != this)
        {
            HideItemInfo();
        }
    }

    public void SelectItem(bool playSound = false)
    {
        // Deseleccionar el item anterior si existe
        if (currentlySelected != null && currentlySelected != this)
        {
            currentlySelected.DeselectItem();
        }

        // Seleccionar este item
        currentlySelected = this;
        
        ShowItemInfo(playSound);
    }

    private void ShowItemInfo(bool playSound)
    {
        if (selectFrame != null)
            selectFrame.SetActive(true);
        
        // Reproducir sonido solo cuando se solicita (al hacer clic)
        if (playSound && InventoryMenu.instance != null && InventoryMenu.instance.pressedButtonSound != null)
        {
            InventoryMenu.instance.pressedButtonSound.Play();
        }
        
        // Mostrar información del item
        if (itemData != null && InventoryInfoBox.instance != null)
        {
            InventoryInfoBox.instance.SetInfo(itemData.displayName, itemData.description);
        }
        else
        {
            Debug.LogWarning("InventoryItemButton: itemData o InventoryInfoBox.instance es null en " + gameObject.name);
        }
    }

    private void HideItemInfo()
    {
        if (selectFrame != null)
            selectFrame.SetActive(false);
        
        ClearInfoBox();
    }

    private void DeselectItem()
    {
        // Solo deseleccionar si no está en hover (para mantener preview en PC)
        if (!isHovering)
        {
            if (selectFrame != null)
                selectFrame.SetActive(false);
        }
    }

    private void ClearInfoBox()
    {
        if (InventoryInfoBox.instance != null)
        {
            InventoryInfoBox.instance.ClearInfo();
        }
    }

    // Método público para seleccionar desde código (útil para el primer item)
    public void SelectFromCode()
    {
        SelectItem(false);
    }
}