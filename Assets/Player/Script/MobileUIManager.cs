using UnityEngine;
using UnityEngine.SceneManagement;

public class MobileUIManager : MonoBehaviour
{
    [Header("Mobile UI Elements")]
    public GameObject mobileUICanvas;
    public MobileControls mobileControls;
    
    [Header("Auto Detection")]
    public bool autoDetectPlatform = true;
    
    public static MobileUIManager instance;
    
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
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void Start()
    {
        SetupMobileUI();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name + ", starting reconnection...");
        
        // Usar Invoke para dar tiempo a que todos los objetos se inicialicen
        Invoke("DelayedReconnection", 0.1f);
    }
    
    private void DelayedReconnection()
    {
        // Reconectar referencias cuando se carga una nueva escena
        ReconnectReferences();
        SetupMobileUI();
        
        // Verificación adicional después de un pequeño delay
        Invoke("VerifyConnections", 0.2f);
    }
    
    private void VerifyConnections()
    {
        if (mobileControls != null && mobileControls.movementJoystick == null)
        {
            Debug.LogWarning("Joystick still not connected after scene load, retrying...");
            mobileControls.FindAndConnectJoystick();
        }
        
        // Verificar que el Player tenga la referencia correcta
        Player player = FindFirstObjectByType<Player>();
        if (player != null && player.useMobileControls && mobileControls != null)
        {
            Debug.Log("Mobile controls verification complete - Player and joystick connected");
        }
    }
    
    private void ReconnectReferences()
    {
        Debug.Log("MobileUIManager: Reconnecting references...");
        
        // Buscar el canvas móvil en la nueva escena si no está asignado
        if (mobileUICanvas == null)
        {
            // Buscar canvas con nombre específico o por tag
            GameObject[] canvases = GameObject.FindGameObjectsWithTag("MobileUI");
            if (canvases.Length > 0)
            {
                mobileUICanvas = canvases[0];
                Debug.Log("Found mobile canvas by tag: " + mobileUICanvas.name);
            }
            else
            {
                // Buscar canvas por nombre
                mobileUICanvas = GameObject.Find("Canvas - Mobile UI");
                if (mobileUICanvas != null)
                {
                    Debug.Log("Found mobile canvas by name: " + mobileUICanvas.name);
                }
            }
        }
        
        // Buscar MobileControls en la escena
        if (mobileControls == null)
        {
            mobileControls = FindFirstObjectByType<MobileControls>();
            if (mobileControls != null)
            {
                Debug.Log("Found MobileControls: " + mobileControls.gameObject.name);
            }
        }
        
        // IMPORTANTE: Reconectar joystick SIEMPRE, incluso si ya hay referencia
        if (mobileControls != null)
        {
            // Usar el nuevo método para buscar y conectar el joystick (incluye búsqueda por tag)
            mobileControls.FindAndConnectJoystick();
        }
        else
        {
            Debug.LogWarning("MobileUIManager: MobileControls not found!");
        }
        
        // Reconectar Player con MobileControls
        Player player = FindFirstObjectByType<Player>();
        if (player != null && mobileControls != null)
        {
            // Usar reflexión para actualizar la referencia privada
            var field = typeof(Player).GetField("mobileControls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(player, mobileControls);
                Debug.Log("Reconnected Player with MobileControls");
            }
        }
        
        Debug.Log("MobileUIManager: Reconnection complete");
    }
    
    private void SetupMobileUI()
    {
        bool isMobile = false;
        
        if (autoDetectPlatform)
        {
            // Auto detect mobile platform
            #if UNITY_ANDROID || UNITY_IOS
                isMobile = true;
            #elif UNITY_EDITOR
                // In editor, you can force mobile mode for testing
                isMobile = Application.isMobilePlatform || true; // Forzar para testing en editor
            #endif
        }
        
        // Enable/disable mobile UI
        if (mobileUICanvas != null)
        {
            mobileUICanvas.SetActive(isMobile);
            
            // IMPORTANTE: Hacer que el canvas persista entre escenas
            if (isMobile && mobileUICanvas.transform.parent == null) // Solo si no es hijo de otro objeto
            {
                DontDestroyOnLoad(mobileUICanvas);
                Debug.Log("Mobile Canvas set to persist across scenes");
            }
        }
        else
        {
            Debug.LogWarning("MobileUICanvas is null! Cannot setup mobile UI");
        }
        
        // Configure player to use mobile controls
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.useMobileControls = isMobile;
            Debug.Log("Player configured for mobile controls: " + isMobile);
        }
        else
        {
            Debug.LogWarning("No Player found to configure mobile controls");
        }
    }
    
    // Method to manually toggle mobile controls (useful for testing)
    public void ToggleMobileControls(bool enable)
    {
        if (mobileUICanvas != null)
        {
            mobileUICanvas.SetActive(enable);
        }
        
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.useMobileControls = enable;
        }
    }
    
    // Método público para reconectar manualmente si es necesario
    public void ForceReconnect()
    {
        ReconnectReferences();
        SetupMobileUI();
    }
}