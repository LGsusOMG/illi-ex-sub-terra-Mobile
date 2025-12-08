using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TalismanPage : MonoBehaviour
{
    public GameObject redGroup;
    public GameObject blueGroup;
    public GameObject yellowGroup;
    public GameObject reminder;

    private void OnEnable()
    {
        Player player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        if (player != null && !player.resting)
        {
            if (reminder != null)
                reminder.SetActive(true);
        }
        else
        {
            if (reminder != null)
                reminder.SetActive(false);
        }
        
        StartCoroutine(SelectFirstToolAfterDelay());
    }

    private void OnDisable()
    {
        // Limpiar info box cuando se cierra la página
        if (ToolInfoBox.instance != null)
        {
            ToolInfoBox.instance.ClearInfo();
        }
    }

    private IEnumerator SelectFirstToolAfterDelay()
    {
        // Esperar a que el layout se actualice
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Buscar el primer tool disponible en orden: red -> blue -> yellow
        ToolButton firstTool = null;
        
        if (redGroup != null && redGroup.transform.childCount > 0)
        {
            firstTool = redGroup.transform.GetChild(0).GetComponent<ToolButton>();
        }
        else if (blueGroup != null && blueGroup.transform.childCount > 0)
        {
            firstTool = blueGroup.transform.GetChild(0).GetComponent<ToolButton>();
        }
        else if (yellowGroup != null && yellowGroup.transform.childCount > 0)
        {
            firstTool = yellowGroup.transform.GetChild(0).GetComponent<ToolButton>();
        }

        if (firstTool != null)
        {
            firstTool.SelectFromCode();
            Debug.Log("TalismanPage: Primer tool seleccionado automáticamente");
        }
        else
        {
            Debug.Log("TalismanPage: No hay tools disponibles");
        }
    }
}