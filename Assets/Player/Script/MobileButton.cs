using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Configuration")]
    public ButtonType buttonType = ButtonType.Jump;

    [Header("Visual Feedback")]
    public Image buttonImage;
    public Color normalColor = Color.white;
    public Color pressedColor = Color.gray;

    private MobileControls mobileControls;

    public enum ButtonType
    {
        Jump,
        Attack,
        Dash,
        Heal,
        Pause,
        TabLeft,
        TabRight,
    }

    private void Start()
    {
        mobileControls = FindFirstObjectByType<MobileControls>();

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        if (buttonImage != null)
            buttonImage.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // DEBUG: Log para ver si el botón detecta el toque
        Debug.Log("MobileButton.OnPointerDown llamado en: " + gameObject.name + " tipo: " + buttonType);

        // Visual feedback
        if (buttonImage != null)
            buttonImage.color = pressedColor;

        // Send input to mobile controls
        if (mobileControls != null)
        {
            Debug.Log("mobileControls encontrado, ejecutando switch para: " + buttonType);

            switch (buttonType)
            {
                case ButtonType.Jump:
                    mobileControls.OnJumpButtonDown();
                    break;
                case ButtonType.Attack:
                    mobileControls.OnAttackButtonPressed();
                    break;
                case ButtonType.Dash:
                    mobileControls.OnDashButtonPressed();
                    break;
                case ButtonType.Heal:
                    mobileControls.OnHealButtonPressed();
                    break;
                case ButtonType.TabLeft:
                    mobileControls.OnTabLeftPressed();
                    break;
                case ButtonType.TabRight:
                    mobileControls.OnTabRightPressed();
                    break;
            }
        }
        else
        {
            Debug.LogError("¡mobileControls es NULL en MobileButton!");
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Visual feedback
        if (buttonImage != null)
            buttonImage.color = normalColor;

        // Send input to mobile controls (only needed for jump)
        if (mobileControls != null && buttonType == ButtonType.Jump)
        {
            mobileControls.OnJumpButtonUp();
        }
    }
}