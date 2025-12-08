using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToolButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject selectFrame;
    private Image image;
    private Button button;
    
    public enum ToolType
    { red, blue, yellow };
    public ToolType toolType;

    [DrawIf("toolType", ToolType.red)]
    public RedTool.ToolName redToolName;
    [DrawIf("toolType", ToolType.blue)]
    public BlueTool.ToolName blueToolName;
    [DrawIf("toolType", ToolType.yellow)]
    public YellowTool.ToolName yellowToolName;

    public Sprite emptyToolSprite;
    private bool foundItem;
    private static ToolButton currentlySelected;
    private bool isHovering = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        
        // Activar Raycast Target para detectar clics
        image = GetComponent<Image>();
        if (image != null && !image.raycastTarget)
        {
            image.raycastTarget = true;
        }
    }

    public void OnEnable()
    {
        selectFrame = transform.GetChild(0).gameObject;
        if (selectFrame != null)
        {
            selectFrame.SetActive(false);
        }
        
        UpdateEquipState();
        GameMaster.instance.OnTalismanChange += UpdateEquipState;
        CheckIfToolFound();
    }

    private void OnDisable()
    {
        if (selectFrame != null)
            selectFrame.SetActive(false);
        
        if (button != null)
            button.onClick.RemoveAllListeners();
        
        GameMaster.instance.OnTalismanChange -= UpdateEquipState;
        
        if (currentlySelected == this)
        {
            currentlySelected = null;
            ClearInfoBox();
        }
        
        isHovering = false;
    }

    // MÓVIL & PC: Click/toque
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"ToolButton: Click detectado en {gameObject.name}");
        
        // Si el tool no ha sido encontrado, solo seleccionar para ver info
        if (!foundItem)
        {
            SelectTool(true);
            return;
        }
        
        // Si ya está seleccionado, equipar/desequipar
        if (currentlySelected == this)
        {
            EquipTool();
        }
        else
        {
            // Primera vez: seleccionar
            SelectTool(true);
        }
    }

    // PC: Hover con mouse
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Application.isMobilePlatform || Input.mousePresent)
        {
            isHovering = true;
            ShowToolInfo(false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        
        if (currentlySelected != this)
        {
            HideToolInfo();
        }
    }

    public void SelectTool(bool playSound = false)
    {
        // Deseleccionar el anterior
        if (currentlySelected != null && currentlySelected != this)
        {
            currentlySelected.DeselectTool();
        }

        currentlySelected = this;
        ShowToolInfo(playSound);
        ScrollPanel();
    }

    private void DeselectTool()
    {
        if (!isHovering)
        {
            if (selectFrame != null)
                selectFrame.SetActive(false);
        }
    }

    private void ShowToolInfo(bool playSound)
    {
        if (selectFrame != null)
            selectFrame.SetActive(true);
        
        if (playSound && InventoryMenu.instance != null && InventoryMenu.instance.movingButtonSound != null)
        {
            InventoryMenu.instance.movingButtonSound.Play();
        }

        if (foundItem)
        {
            switch (toolType)
            {
                case ToolType.red:
                    ToolInfoBox.instance.SetInfo(
                        GameMaster.instance.redToolData[(int)redToolName].displayName,
                        GameMaster.instance.redToolData[(int)redToolName].description,
                        GameMaster.instance.redToolData[(int)redToolName].sprite
                    );
                    break;

                case ToolType.blue:
                    ToolInfoBox.instance.SetInfo(
                        GameMaster.instance.blueToolData[(int)blueToolName].displayName,
                        GameMaster.instance.blueToolData[(int)blueToolName].description,
                        GameMaster.instance.blueToolData[(int)blueToolName].sprite
                    );
                    break;

                case ToolType.yellow:
                    ToolInfoBox.instance.SetInfo(
                        GameMaster.instance.yellowToolData[(int)yellowToolName].displayName,
                        GameMaster.instance.yellowToolData[(int)yellowToolName].description,
                        GameMaster.instance.yellowToolData[(int)yellowToolName].sprite
                    );
                    break;
            }
        }
        else
        {
            ToolInfoBox.instance.SetInfo("", "", emptyToolSprite, false);
        }
    }

    private void HideToolInfo()
    {
        if (selectFrame != null)
            selectFrame.SetActive(false);
        
        ClearInfoBox();
    }

    private void ClearInfoBox()
    {
        if (ToolInfoBox.instance != null)
        {
            ToolInfoBox.instance.ClearInfo();
        }
    }

    private void EquipTool()
    {
        Player player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        if (player == null || !player.resting)
        {
            Debug.Log("ToolButton: No se puede equipar - jugador no está descansando");
            return;
        }

        if (InventoryMenu.instance != null && InventoryMenu.instance.pressedButtonSound != null)
        {
            InventoryMenu.instance.pressedButtonSound.Play();
        }

        switch (toolType)
        {
            case ToolType.red:
                GameMaster.instance.EquipUnequipRedTool(redToolName);
                break;

            case ToolType.blue:
                GameMaster.instance.EquipUnequipBlueTool(blueToolName);
                break;

            case ToolType.yellow:
                GameMaster.instance.EquipUnequipYellowTool(yellowToolName);
                break;
        }
    }

    private void ScrollPanel()
    {
        Canvas.ForceUpdateCanvases();

        RectTransform rect = GetComponent<RectTransform>();
        RectTransform groupRect = transform.parent.GetComponent<RectTransform>();
        RectTransform panelRect = groupRect.parent.GetComponent<RectTransform>();
        RectTransform scrollRect = panelRect.parent.GetComponent<RectTransform>();

        float itemPosY = rect.anchoredPosition.y + groupRect.anchoredPosition.y;
        float endPosY = 0 - (scrollRect.sizeDelta.y / 2) - itemPosY;
        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, endPosY);
    }

    private void CheckIfToolFound()
    {
        PlayerData playerData = GameMaster.instance.playerData;

        switch (toolType)
        {
            case ToolType.red:
                if (playerData.foundRedTools.Contains(redToolName))
                {
                    image.sprite = GameMaster.instance.redToolData[(int)redToolName].sprite;
                    foundItem = true;
                }
                else
                {
                    image.sprite = emptyToolSprite;
                    foundItem = false;
                }
                break;

            case ToolType.blue:
                if (playerData.foundBlueTools.Contains(blueToolName))
                {
                    image.sprite = GameMaster.instance.blueToolData[(int)blueToolName].sprite;
                    foundItem = true;
                }
                else
                {
                    image.sprite = emptyToolSprite;
                    foundItem = false;
                }
                break;

            case ToolType.yellow:
                if (playerData.foundYellowTools.Contains(yellowToolName))
                {
                    image.sprite = GameMaster.instance.yellowToolData[(int)yellowToolName].sprite;
                    foundItem = true;
                }
                else
                {
                    image.sprite = emptyToolSprite;
                    foundItem = false;
                }
                break;
        }
    }

    private void UpdateEquipState()
    {
        switch (toolType)
        {
            case ToolType.red:
                if (image.color.a != 0.3f && GameMaster.instance.playerData.equippedRedTools.Contains(redToolName))
                    image.color = new Color(1, 1, 1, 0.3f);
                else if (image.color.a != 1f && !GameMaster.instance.playerData.equippedRedTools.Contains(redToolName))
                    image.color = new Color(1, 1, 1, 1);
                break;

            case ToolType.blue:
                if (image.color.a != 0.3f && GameMaster.instance.playerData.equippedBlueTools.Contains(blueToolName))
                    image.color = new Color(1, 1, 1, 0.3f);
                else if (image.color.a != 1f && !GameMaster.instance.playerData.equippedBlueTools.Contains(blueToolName))
                    image.color = new Color(1, 1, 1, 1);
                break;

            case ToolType.yellow:
                if (image.color.a != 0.3f && GameMaster.instance.playerData.equippedYellowTools.Contains(yellowToolName))
                    image.color = new Color(1, 1, 1, 0.3f);
                else if (image.color.a != 1f && !GameMaster.instance.playerData.equippedYellowTools.Contains(yellowToolName))
                    image.color = new Color(1, 1, 1, 1);
                break;
        }
    }

    // Método público para seleccionar desde código
    public void SelectFromCode()
    {
        SelectTool(false);
    }
}