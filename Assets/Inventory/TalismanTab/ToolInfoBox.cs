using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ToolInfoBox : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Image image;
    public TextMeshProUGUI descText;

    public static ToolInfoBox instance;

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

    public void SetInfo(string toolName, string toolDescription, Sprite toolSprite, bool showImage = true)
    {
        if (nameText != null)
            nameText.text = toolName;
        
        if (descText != null)
            descText.text = toolDescription;
        
        if (image != null)
        {
            image.sprite = toolSprite;
            image.enabled = showImage;
        }
    }

    public void ClearInfo()
    {
        if (nameText != null)
            nameText.text = "";
        
        if (descText != null)
            descText.text = "";
        
        if (image != null)
            image.enabled = false;
    }
}