using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryPage : MonoBehaviour
{
    public GameObject inventoryGrid;

    private void OnEnable()
    {
        AddSlotPrefabToGrid();
        StartCoroutine(SelectFirstItemAfterDelay());
    }

    private void OnDisable()
    {
        DeleteEverySlotInGrid();
        
        // Limpiar info box cuando se cierra la página
        if (InventoryInfoBox.instance != null)
        {
            InventoryInfoBox.instance.ClearInfo();
        }
    }

    private IEnumerator SelectFirstItemAfterDelay()
    {
        // Esperar a que se instancien los items
        yield return new WaitForEndOfFrame();
        
        // Buscar el primer item y seleccionarlo
        if (inventoryGrid.transform.childCount > 0)
        {
            Transform firstChild = inventoryGrid.transform.GetChild(0);
            InventoryItemButton firstButton = firstChild.GetComponent<InventoryItemButton>();
            
            if (firstButton != null)
            {
                firstButton.SelectFromCode();
                Debug.Log("InventoryPage: Primer item seleccionado automáticamente");
            }
        }
        else
        {
            Debug.Log("InventoryPage: No hay items en el inventario");
        }
    }

    private void AddSlotPrefabToGrid()
    {
        int[] inventoryItemAmount = GameMaster.instance.playerData.inventoryItemAmount;

        // Control del orden de aparición en el grid
        if (inventoryItemAmount[(int)InventoryItem.ItemName.verdantMantle] > 0)
            Instantiate(GameMaster.instance.inventorySlotPrefabs[(int)InventoryItem.ItemName.verdantMantle], inventoryGrid.transform, false);

        if (inventoryItemAmount[(int)InventoryItem.ItemName.notebookQuill] > 0)
            Instantiate(GameMaster.instance.inventorySlotPrefabs[(int)InventoryItem.ItemName.notebookQuill], inventoryGrid.transform, false);

        if (inventoryItemAmount[(int)InventoryItem.ItemName.carapaceBackpack] > 0)
            Instantiate(GameMaster.instance.inventorySlotPrefabs[(int)InventoryItem.ItemName.carapaceBackpack], inventoryGrid.transform, false);

        if (inventoryItemAmount[(int)InventoryItem.ItemName.silkbindSandal] > 0)
            Instantiate(GameMaster.instance.inventorySlotPrefabs[(int)InventoryItem.ItemName.silkbindSandal], inventoryGrid.transform, false);

        if (inventoryItemAmount[(int)InventoryItem.ItemName.coolKey] > 0)
            Instantiate(GameMaster.instance.inventorySlotPrefabs[(int)InventoryItem.ItemName.coolKey], inventoryGrid.transform, false);
    }

    private void DeleteEverySlotInGrid()
    {
        foreach (Transform child in inventoryGrid.transform)
        {
            Destroy(child.gameObject);
        }
    }
}