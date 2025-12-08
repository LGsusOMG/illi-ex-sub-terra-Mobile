using UnityEngine;

public class MobileControls : MonoBehaviour
{
    [Header("Joystick")]
    public Joystick movementJoystick;

    [Header("Mobile Control Flags")]
    public bool jumpPressed = false;
    public bool jumpHeld = false;
    public bool attackPressed = false;
    public bool dashPressed = false;
    public bool healPressed = false;
    public bool pausePressed = false;
    public bool tabLeftPressed = false;
    public bool tabRightPressed = false;

    // Referencias para almacenar el estado de los botones
    private bool previousJumpState = false;

    public static MobileControls instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persistir entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Buscar el joystick en la escena actual si no está asignado
        FindAndConnectJoystick();
    }

    // Método público para forzar la búsqueda del joystick
    public void FindAndConnectJoystick()
    {
        if (movementJoystick == null)
        {
            Debug.Log("Buscando joystick...");

            // Primero intentar buscar por tag (más específico)
            GameObject joystickObj = GameObject.FindWithTag("MobileJoystick");
            if (joystickObj != null)
            {
                movementJoystick = joystickObj.GetComponent<Joystick>();
                if (movementJoystick != null)
                {
                    Debug.Log("MobileControls: Conectado al joystick por etiqueta: " + movementJoystick.gameObject.name);
                    return;
                }
                else
                {
                    Debug.LogWarning("¡GameObject con etiqueta MobileJoystick encontrado pero sin componente Joystick!");
                }
            }
            else
            {
                Debug.Log("Ningún GameObject con etiqueta 'MobileJoystick' encontrado, buscando por tipo...");
            }

            // Si no funciona por tag, usar el método anterior
            movementJoystick = FindFirstObjectByType<Joystick>();
            if (movementJoystick != null)
            {
                Debug.Log("MobileControls: Conectado al joystick por tipo: " + movementJoystick.gameObject.name);
                Debug.LogWarning("Considera agregar la etiqueta 'MobileJoystick' a " + movementJoystick.gameObject.name + " para mejor rendimiento");
            }
            else
            {
                Debug.LogError("MobileControls: ¡NO SE ENCONTRÓ JOYSTICK! Asegúrate de que hay un GameObject con componente Joystick en la escena");

                // Listar todos los GameObjects para debug
                Joystick[] allJoysticks = FindObjectsByType<Joystick>(FindObjectsSortMode.None);
                Debug.Log("Total de componentes Joystick en la escena: " + allJoysticks.Length);
                for (int i = 0; i < allJoysticks.Length; i++)
                {
                    Debug.Log("Joystick " + i + ": " + allJoysticks[i].gameObject.name + " (Activo: " + allJoysticks[i].gameObject.activeInHierarchy + ")");
                }
            }
        }
        else
        {
            Debug.Log("MobileControls: Joystick ya conectado: " + movementJoystick.gameObject.name + " (Activo: " + movementJoystick.gameObject.activeInHierarchy + ")");
        }
    }

    private void Update()
    {
        // Reset pressed flags at the end of frame using LateUpdate timing
        // This ensures the flags persist through the frame for proper detection
    }

    private void LateUpdate()
    {
        // Verificar si el joystick sigue existiendo cada frame
        if (movementJoystick != null && movementJoystick.gameObject == null)
        {
            Debug.LogWarning("¡GameObject del Joystick fue destruido! Buscando uno nuevo...");
            movementJoystick = null;
            FindAndConnectJoystick();
        }

        // Si no hay joystick, intentar encontrar uno
        if (movementJoystick == null)
        {
            FindAndConnectJoystick();
        }

        // Reset flags after all Update calls are complete
        if (jumpPressed && !jumpHeld)
        {
            jumpPressed = false;
        }

        if (attackPressed)
        {
            attackPressed = false;
        }

        if (dashPressed)
        {
            dashPressed = false;
        }

        if (healPressed)
        {
            healPressed = false;
        }

        if (pausePressed)
        {
            pausePressed = false;
        }

        if (tabLeftPressed)
        {
            tabLeftPressed = false;
        }

        if (tabRightPressed)
        {
            tabRightPressed = false;
        }
    }

    // Métodos llamados por los botones de la UI
    public void OnJumpButtonDown()
    {
        jumpPressed = true;
        jumpHeld = true;
    }

    public void OnJumpButtonUp()
    {
        jumpHeld = false;
    }

    public void OnAttackButtonPressed()
    {
        attackPressed = true;
    }

    public void OnDashButtonPressed()
    {
        dashPressed = true;
        Debug.Log("¡Botón de desplazamiento presionado en MobileControls!");
    }

    public void OnHealButtonPressed()
    {
        healPressed = true;
        Debug.Log("¡Botón de curación presionado en MobileControls!");
    }

    public void OnPauseButtonPressed()
    {
        pausePressed = true;
        Debug.Log("¡Botón de pausa presionado en MobileControls!");

        // Intentar usar LevelUIManager si está disponible (para niveles)
        if (LevelUIManager.instance != null)
        {
            LevelUIManager.instance.TogglePause();
            Debug.Log("Usando LevelUIManager para pausa");
        }
    }

    public void OnTabLeftPressed()
    {
        tabLeftPressed = true;
        Debug.Log("¡Tab izquierdo presionado en MobileControls!");
    }

    public void OnTabRightPressed()
    {
        tabRightPressed = true;
        Debug.Log("¡Tab derecho presionado en MobileControls!");
    }

    // Métodos para obtener input del joystick
    public Vector2 GetMovementInput()
    {
        if (movementJoystick != null)
        {
            Vector2 direction = movementJoystick.Direction;
            // Debug solo cuando hay movimiento para no spam la consola
            if (direction.magnitude > 0.1f)
            {
                Debug.Log("Entrada del joystick: " + direction);
            }
            return direction;
        }
        else
        {
            Debug.LogWarning("MobileControls: movementJoystick is null!");
            return Vector2.zero;
        }
    }

    public float GetHorizontalInput()
    {
        if (movementJoystick != null)
        {
            return movementJoystick.Horizontal;
        }
        else
        {
            Debug.LogWarning("MobileControls: movementJoystick is null!");
            return 0f;
        }
    }

    public float GetVerticalInput()
    {
        if (movementJoystick != null)
        {
            return movementJoystick.Vertical;
        }
        else
        {
            Debug.LogWarning("MobileControls: movementJoystick is null!");
            return 0f;
        }
    }

    // Métodos para verificar estados de botones (similares a InputAction)
    public bool WasJumpPressedThisFrame()
    {
        return jumpPressed;
    }

    public bool IsJumpPressed()
    {
        return jumpHeld;
    }

    public bool WasJumpReleasedThisFrame()
    {
        bool released = previousJumpState && !jumpHeld;
        previousJumpState = jumpHeld;
        return released;
    }

    public bool WasAttackPressedThisFrame()
    {
        return attackPressed;
    }

    public bool WasDashPressedThisFrame()
    {
        return dashPressed;
    }

    public bool WasHealPressedThisFrame()
    {
        return healPressed;
    }

    public bool WasPausePressedThisFrame()
    {
        return pausePressed;
    }

    public bool WasTabLeftPressedThisFrame()
    {
        return tabLeftPressed;
    }

    public bool WasTabRightPressedThisFrame()
    {
        return tabRightPressed;
    }
}