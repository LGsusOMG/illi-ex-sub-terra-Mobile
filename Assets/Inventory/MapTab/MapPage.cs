using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MapPage : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 500f;
    public float touchMoveSpeed = 1f;
    
    [Header("Zoom Settings")]
    public float zoomSpeed = 1f;
    public float touchZoomSpeed = 0.05f;
    public float minZoom = 0.2f;
    public float maxZoom = 3f;
    
    [Header("References")]
    public GameObject playerMarker;
    public GameObject mapContent;
    
    // Variables para control táctil
    private Vector2 lastTouchPosition;
    private bool isDragging = false;
    private bool isTouchingUI = false;
    
    // Variables para control con mouse (PC)
    private Vector2 lastMousePosition;
    private bool isMouseDragging = false;

    private void OnEnable()
    {
        UpdatePlayerPosOnMap();
    }

    private void Update()
    {
        // PC: Mouse controls
        if (Input.mousePresent && !Application.isMobilePlatform)
        {
            HandleMouseControls();
            HandleMouseWheelZoom();
        }
        
        // Móvil: Touch controls
        if (Input.touchSupported)
        {
            HandleTouchControls();
        }
    }

    #region PC Controls (Mouse)
    
    private void HandleMouseControls()
    {
        // Click izquierdo para arrastrar
        if (Input.GetMouseButtonDown(0))
        {
            // Verificar si estamos tocando UI
            if (!IsPointerOverUIElement())
            {
                lastMousePosition = Input.mousePosition;
                isMouseDragging = true;
            }
        }
        else if (Input.GetMouseButton(0) && isMouseDragging)
        {
            Vector2 mouseDelta = (Vector2)Input.mousePosition - lastMousePosition;
            Vector3 moveAmount = new Vector3(mouseDelta.x, mouseDelta.y, 0) * touchMoveSpeed;
            mapContent.transform.localPosition += moveAmount;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isMouseDragging = false;
        }
    }
    
    private void HandleMouseWheelZoom()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            float zoomAmount = scrollDelta * zoomSpeed;
            mapContent.transform.localScale += new Vector3(zoomAmount, zoomAmount, 0);
            ClampZoom();
        }
    }
    
    #endregion

    #region Mobile Controls (Touch)
    
    private void HandleTouchControls()
    {
        // Si hay un solo toque - arrastrar
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // Verificar si estamos tocando UI
                if (!IsPointerOverUIElement(touch.position))
                {
                    lastTouchPosition = touch.position;
                    isDragging = true;
                }
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 touchDelta = touch.position - lastTouchPosition;
                Vector3 moveAmount = new Vector3(touchDelta.x, touchDelta.y, 0) * touchMoveSpeed;
                mapContent.transform.localPosition += moveAmount;
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        // Si hay dos toques - hacer zoom (pinch)
        else if (Input.touchCount == 2)
        {
            isDragging = false;
            
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // Calcular la distancia anterior entre los toques
            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
            float touchDeltaMag = (touch0.position - touch1.position).magnitude;

            // Calcular la diferencia en la distancia
            float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

            // Aplicar zoom
            float zoomAmount = deltaMagnitudeDiff * touchZoomSpeed;
            mapContent.transform.localScale += new Vector3(zoomAmount, zoomAmount, 0);

            ClampZoom();
        }
        else
        {
            isDragging = false;
        }
    }
    
    #endregion

    #region Utility Methods
    
    private void ClampZoom()
    {
        float clampedZoom = Mathf.Clamp(mapContent.transform.localScale.x, minZoom, maxZoom);
        mapContent.transform.localScale = new Vector3(clampedZoom, clampedZoom, 1f);
    }

    private bool IsPointerOverUIElement()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    
    private bool IsPointerOverUIElement(Vector2 touchPosition)
    {
        if (EventSystem.current == null) return false;
        
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touchPosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        // Si hay resultados y alguno es un botón u otro elemento interactivo, no arrastramos
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
            {
                return true;
            }
        }
        
        return false;
    }

    public void UpdatePlayerPosOnMap()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        int areaCount = mapContent.transform.childCount;
        for (int i = 0; i < areaCount; i++)
        {
            Transform area = mapContent.transform.GetChild(i);
            int roomCount = area.childCount;
            for (int j = 0; j < roomCount; j++)
            {
                Transform room = area.GetChild(j);
                if (room.name == currentSceneName)
                {
                    // The marker will automatically destroyed on Disable
                    Instantiate(playerMarker, room, false);

                    // Auto centering toward that room
                    mapContent.transform.localScale = new Vector3(1f, 1f, 1f);
                    mapContent.transform.localPosition = -area.transform.localPosition;
                    mapContent.transform.localPosition -= room.transform.localPosition;
                    return;
                }
            }
        }
        
        Debug.LogWarning($"MapPage: No se encontró la habitación actual '{currentSceneName}' en el mapa");
    }
    
    /// <summary>
    /// Centrar el mapa en una posición específica
    /// </summary>
    public void CenterMapOnPosition(Vector3 position, float zoom = 1f)
    {
        mapContent.transform.localScale = new Vector3(zoom, zoom, 1f);
        mapContent.transform.localPosition = -position;
        ClampZoom();
    }
    
    /// <summary>
    /// Resetear el mapa a la posición del jugador
    /// </summary>
    public void ResetMapToPlayer()
    {
        UpdatePlayerPosOnMap();
    }
    
    #endregion
}