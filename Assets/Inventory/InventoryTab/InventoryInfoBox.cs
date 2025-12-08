using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryInfoBox : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    public static InventoryInfoBox instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ClearInfo();
    }

    public void ClearInfo()
    {
        if (nameText != null)
            nameText.text = "";
        
        if (descText != null)
            descText.text = "";
    }

    public void SetInfo(string itemName, string itemDescription)
    {
        if (nameText != null)
            nameText.text = itemName;
        
        if (descText != null)
            descText.text = itemDescription;
    }
}