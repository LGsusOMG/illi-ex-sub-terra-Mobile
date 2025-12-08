using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeTalismanButton : MonoBehaviour, IPointerClickHandler
{
    public GameObject selectFrame;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        
        // Activar Raycast Target
        Image image = GetComponent<Image>();
        if (image != null && !image.raycastTarget)
        {
            image.raycastTarget = true;
        }
    }

    public void OnEnable()
    {
        selectFrame = transform.GetChild(0)?.gameObject;
        if (selectFrame != null)
        {
            selectFrame.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (selectFrame != null)
            selectFrame.SetActive(false);
    }

    // MÓVIL & PC: Click/toque
    public void OnPointerClick(PointerEventData eventData)
    {
        PressedButton();
    }

    public void PressedButton()
    {
        Player player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        if (player == null || !player.resting)
        {
            Debug.Log("ChangeTalismanButton: No se puede cambiar talismán - jugador no está descansando");
            return;
        }

        if (InventoryMenu.instance != null && InventoryMenu.instance.pressedButtonSound != null)
        {
            InventoryMenu.instance.pressedButtonSound.Play();
        }
        
        GameMaster.instance.ChangeToNextTalisman();
        Debug.Log("ChangeTalismanButton: Cambiando al siguiente talismán");
    }
}