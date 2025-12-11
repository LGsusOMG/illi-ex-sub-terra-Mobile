using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class LevelLoader : MonoBehaviour
{
    [Header("LevelTransition")]
    public Animator anim;
    public float transitionTime = 1f;
    public static LevelLoader instance;
    public string spawnPosName = "";

    [Header("Spawn Position")]
    public Vector3 customSpawnPosition = new Vector3(0.22f, 21.93f, 0f);
    public bool useCustomSpawnPosition = true;

    [Header("Respawn")]
    private bool doRespawn;

    [Header("Other")]
    private Player player;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            player = GameObject.Find("Tenroh").GetComponent<Player>();
            SpawnPointInit();
        }
        else
            Destroy(this.gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneChange;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneChange;
    }

    public void SceneChange(Scene scene, LoadSceneMode mode)
    {
        // Si volvemos al MainMenu, desactivar spawn personalizado y limpiar referencias
        if (scene.name == "MainMenu")
        {
            useCustomSpawnPosition = false;
            player = null;
            spawnPosName = "";
            doRespawn = false;
            Debug.Log("LevelLoader: Estado reseteado al volver al MainMenu");
            return; // No hacer nada más en el MainMenu
        }
        
        GameMaster.instance.PatchInventoryReference();
        GameMaster.instance.AddVisitedRoom(scene.name);
        
        // IMPORTANTE: Actualizar referencia del jugador después de cambiar escena
        RefreshPlayerReference();
        
        UpdatePlayerPosition();
    }
    
    // Actualiza la referencia del jugador después de cambios de escena
    private void RefreshPlayerReference()
    {
        // Buscar el jugador en la escena actual
        GameObject playerGO = GameObject.Find("Tenroh");
        if (playerGO != null)
        {
            player = playerGO.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log($"LevelLoader: Referencia del jugador actualizada en escena {SceneManager.GetActiveScene().name}");
            }
            else
            {
                Debug.LogError("LevelLoader: GameObject 'Tenroh' encontrado pero no tiene componente Player");
            }
        }
        else
        {
            Debug.LogWarning($"LevelLoader: No se encontró GameObject 'Tenroh' en escena {SceneManager.GetActiveScene().name}");
            // Intentar buscar por tag o componente
            Player foundPlayer = FindFirstObjectByType<Player>();
            if (foundPlayer != null)
            {
                player = foundPlayer;
                Debug.Log($"LevelLoader: Jugador encontrado por componente: {foundPlayer.gameObject.name}");
            }
        }
    }

    private void SpawnPointInit()
    {
        if (GameMaster.instance.playerData.respawnScene == "")
        {
            Debug.Log("Should never reach here");
            //GameMaster.instance.playerData.respawnScene = SceneManager.GetActiveScene().name;
        }
        else
        {
            doRespawn = true;

            // For easier unit testing
            if (Application.isEditor)
            {
                Debug.Log("Reset player respawnPos to (0 0 0) and delete respawnChairName");
                GameMaster.instance.playerData.respawnPos = Vector3.zero;
                GameMaster.instance.playerData.respawnChairName = "";
            }

            // Edge case: When player New game and die before reach the first chair
            if (GameMaster.instance.playerData.respawnChairName == "" && GameMaster.instance.playerData.respawnPos == Vector3.zero)
            {
                if (player != null)
                {
                    GameMaster.instance.playerData.respawnPos = player.transform.position;
                }
                else
                {
                    Debug.LogWarning("LevelLoader: No se puede establecer respawnPos, jugador no encontrado");
                }
            }
            UpdatePlayerPosition();
        }
    }

    private void UpdatePlayerPosition()
    {
        // Verificar que tenemos referencia del jugador
        if (player == null)
        {
            Debug.LogWarning("LevelLoader: No hay referencia del jugador, intentando encontrarlo...");
            RefreshPlayerReference();
            
            if (player == null)
            {
                Debug.LogError("LevelLoader: No se puede actualizar posición, jugador no encontrado");
                return;
            }
        }
        
        // Update position because of death
        if (doRespawn)
        {
            // Si está habilitado el spawn personalizado, usar esa coordenada
            if (useCustomSpawnPosition)
            {
                player.transform.position = customSpawnPosition;
                player.rb.bodyType = RigidbodyType2D.Dynamic;
                Debug.Log($"LevelLoader: Jugador spawneado en posición personalizada: {customSpawnPosition}");
            }
            // Not sit on any chair yet
            else if (GameMaster.instance.playerData.respawnChairName == "")
            {
                player.transform.position = GameMaster.instance.playerData.respawnPos;
                player.rb.bodyType = RigidbodyType2D.Dynamic;
            }
            else
            {
                RestChair chair = GameObject.Find(GameMaster.instance.playerData.respawnChairName).GetComponent<RestChair>();
                if (chair != null)
                {
                    player.transform.position = chair.transform.position;
                    chair.RespawnAssignToChair(player.GetComponent<Player>());
                    chair.GetOnChair();
                }
                else
                {
                    Debug.LogError($"LevelLoader: No se encontró RestChair '{GameMaster.instance.playerData.respawnChairName}'");
                }
            }
            doRespawn = false;
        }
        // Update position because of change scene
        else if (spawnPosName != "")
        {
            Transform target = GameObject.Find(spawnPosName).transform;
            if (target != null)
            {
                player.transform.position = target.position;
            }
            else
            {
                Debug.LogError($"LevelLoader: No se encontró spawn point '{spawnPosName}'");
            }
            
            spawnPosName = "";
            player.rb.gravityScale = player.originalGravityScale;
        }
        
        // Restaurar contadores de control
        if (player.disableControlCounter > 0)
        {
            player.disableControlCounter -= 1;
            if (player.disableControlCounter < 0)
                player.disableControlCounter = 0;
        }
    }

    public void UpdateSpawnPoint(string restChairName)
    {
        GameMaster.instance.playerData.respawnChairName = restChairName;
        GameMaster.instance.playerData.respawnScene = SceneManager.GetActiveScene().name;
    }

    public void Respawn()
    {
        doRespawn = true;
        LoadLevel(GameMaster.instance.playerData.respawnScene, "");
    }

    public void LoadLevel(string sceneName, string spawnPosName)
    {
        this.spawnPosName = spawnPosName;
        player.disableControlCounter += 1;
        player.rb.gravityScale = 0f;
        StartCoroutine(LoadingScreen(sceneName));
    }

    private IEnumerator LoadingScreen(string sceneName)
    {
        anim.SetBool("changeScene", true);
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(sceneName);
        anim.SetBool("changeScene", false);
    }
}
