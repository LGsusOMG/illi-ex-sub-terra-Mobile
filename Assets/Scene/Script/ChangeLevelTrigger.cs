using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLevelTrigger : MonoBehaviour
{
    public string levelName;
    public string spawnPosName;
    private bool activated;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!activated)
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player)
            {
                // Verificar que InventoryMenu.instance existe antes de usarlo
                if (InventoryMenu.instance != null)
                {
                    InventoryMenu.instance.CloseMenu();
                }
                else
                {
                    Debug.LogWarning("InventoryMenu.instance is null in ChangeLevelTrigger");
                }
                
                // Verificar que LevelLoader.instance existe antes de usarlo
                if (LevelLoader.instance != null)
                {
                    LevelLoader.instance.LoadLevel(levelName, spawnPosName);
                    activated = true;
                }
                else
                {
                    Debug.LogError("LevelLoader.instance is null in ChangeLevelTrigger. Cannot change level!");
                }
            }
        }
    }
}
