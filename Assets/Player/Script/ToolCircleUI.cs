using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToolCircleUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public Image circleImage;
    public Image toolImage;
    private Button button;
    
    // Para feedback visual al presionar
    private Vector3 originalScale;
    private bool isPressed = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        
        originalScale = transform.localScale;
        
        // Activar Raycast Target para detectar clics
        Image img = GetComponent<Image>();
        if (img != null && !img.raycastTarget)
        {
            img.raycastTarget = true;
        }
        
        // Las imágenes hijas no deben bloquear el raycast
        if (circleImage != null)
            circleImage.raycastTarget = false;
        if (toolImage != null)
            toolImage.raycastTarget = false;
    }

    private void Update()
    {
        if (GameMaster.instance.playerData.equippedRedTools.Count == 0)
        {
            if (circleImage.enabled)
                circleImage.enabled = false;
            if (toolImage.enabled)
                toolImage.enabled = false;
            return;
        }
        else
        {
            if (!circleImage.enabled)
                circleImage.enabled = true;
            if (!toolImage.enabled)
                toolImage.enabled = true;

            int currentToolId = (int)GameMaster.instance.playerData.equippedRedTools[GameMaster.instance.playerData.selectedRedToolId];
            circleImage.fillAmount = GameMaster.instance.playerData.redToolsCurrentCharge[currentToolId] / GameMaster.instance.redToolData[currentToolId].maxCharge;

            // Update tool image
            if (toolImage.sprite != GameMaster.instance.redToolData[currentToolId].sprite)
                toolImage.sprite = GameMaster.instance.redToolData[currentToolId].sprite;
        }
    }

    // MÓVIL & PC: Click/toque para usar la herramienta
    public void OnPointerClick(PointerEventData eventData)
    {
        UseTool();
    }

    // Feedback visual al presionar
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        transform.localScale = originalScale * 0.9f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        transform.localScale = originalScale;
    }

    private void UseTool()
    {
        // Verificar que hay herramientas equipadas
        if (GameMaster.instance.playerData.equippedRedTools.Count == 0)
        {
            Debug.Log("ToolCircleUI: No hay herramientas rojas equipadas");
            return;
        }

        // Obtener el Player
        Player player = Player.instance;
        if (player == null)
        {
            Debug.LogError("ToolCircleUI: No se encontró Player.instance");
            return;
        }

        // Verificar que el jugador puede usar herramientas (mismas condiciones que en HandleToolUsage)
        if (player.resting || player.disableControlCounter > 0 || player.inMenu)
        {
            Debug.Log("ToolCircleUI: El jugador no puede usar herramientas ahora");
            return;
        }

        // Verificar cooldown de la herramienta
        // Nota: El toolTimer es privado en Player, así que confiamos en que el cooldown se maneje internamente
        // O podrías hacer toolTimer público en Player si quieres verificarlo aquí

        // Obtener la herramienta actual equipada
        RedTool.ToolName usedTool = GameMaster.instance.playerData.equippedRedTools[GameMaster.instance.playerData.selectedRedToolId];

        // Obtener el RedToolController y usar la herramienta
        RedToolController toolController = player.GetComponent<RedToolController>();
        if (toolController == null)
        {
            Debug.LogError("ToolCircleUI: No se encontró RedToolController en el jugador");
            return;
        }

        Debug.Log($"ToolCircleUI: Usando herramienta {usedTool}");
        toolController.UseRedTool(usedTool);
    }

    // Método público para usar herramienta desde código
    public void UseToolFromCode()
    {
        UseTool();
    }

    private void OnDisable()
    {
        // Restaurar escala si se desactiva mientras está presionado
        if (isPressed)
        {
            transform.localScale = originalScale;
            isPressed = false;
        }
    }
}