using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public GameObject pickupText;
    private bool activated;
    private InputMaster inputMaster;
    private bool inRange;
    public AudioClip pickupSound;

    public enum ItemType
    {
        redTool, blueTool, yellowTool
    }
    public ItemType itemType;

    [DrawIf("itemType", ItemType.redTool)]
    public RedTool.ToolName redTool;
    [DrawIf("itemType", ItemType.blueTool)]
    public BlueTool.ToolName blueTool;
    [DrawIf("itemType", ItemType.yellowTool)]
    public YellowTool.ToolName yellowTool;

    private void Start()
    {
        pickupText.SetActive(false);

        // Si el jugador ya tiene este item, destruirlo
        switch (itemType)
        {
            case ItemType.redTool:
                if (GameMaster.instance.playerData.foundRedTools.Contains(redTool))
                    Destroy(gameObject);
                break;

            case ItemType.blueTool:
                if (GameMaster.instance.playerData.foundBlueTools.Contains(blueTool))
                    Destroy(gameObject);
                break;

            case ItemType.yellowTool:
                if (GameMaster.instance.playerData.foundYellowTools.Contains(yellowTool))
                    Destroy(gameObject);
                break;
        }
    }

    private void OnEnable()
    {
        inputMaster = new InputMaster();
        inputMaster.Enable();
    }

    private void OnDisable()
    {
        if (inputMaster != null)
            inputMaster.Disable();
    }

    private void Update()
    {
        bool pressUp = inputMaster.Gameplay.Movement.ReadValue<Vector2>().y == 1;

        if (pressUp && inRange && !activated)
        {
            PickupItem();
        }
    }

    // --------------------------------------
    // NUEVO → RECOGER AL TOCAR EL ÍTEM
    // --------------------------------------
    private void OnMouseDown()
    {
        if (!activated)  
        {
            PickupItem();
        }
    }
    // --------------------------------------

    private void PickupItem()
    {
        activated = true;
        AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position);

        switch (itemType)
        {
            case ItemType.redTool:
                if (!GameMaster.instance.playerData.foundRedTools.Contains(redTool))
                {
                    GameMaster.instance.playerData.foundRedTools.Add(redTool);
                    RedTool toolData = GameMaster.instance.redToolData[(int)redTool];
                    NotifyCanvas.instance.AddItemNotifyBox(toolData.sprite, toolData.displayName);
                }
                break;

            case ItemType.blueTool:
                if (!GameMaster.instance.playerData.foundBlueTools.Contains(blueTool))
                {
                    GameMaster.instance.playerData.foundBlueTools.Add(blueTool);
                    BlueTool toolData = GameMaster.instance.blueToolData[(int)blueTool];
                    NotifyCanvas.instance.AddItemNotifyBox(toolData.sprite, toolData.displayName);
                }
                break;

            case ItemType.yellowTool:
                if (!GameMaster.instance.playerData.foundYellowTools.Contains(yellowTool))
                {
                    GameMaster.instance.playerData.foundYellowTools.Add(yellowTool);
                    YellowTool toolData = GameMaster.instance.yellowToolData[(int)yellowTool];
                    NotifyCanvas.instance.AddItemNotifyBox(toolData.sprite, toolData.displayName);
                }
                break;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player)
        {
            pickupText.SetActive(true);
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player)
        {
            pickupText.SetActive(false);
            inRange = false;
        }
    }
}
