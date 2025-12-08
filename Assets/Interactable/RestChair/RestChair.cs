using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestChair : InRangeInteractable
{
    public Transform sittingPos;
    [SerializeField] private bool playerSitting;
    public Image softFlash;
    public GameObject savingText;
    public AudioSource saveSfx;

    protected override void Start()
    {
        base.Start();
        savingText.SetActive(false);
    }

    private void Update()
    {
        HandleGetOnChair();
        HandleGetOffChair();
    }

    // -----------------------------
    //  MÉTODO NUEVO: CLICK / TOUCH
    // -----------------------------
    private void OnMouseDown()
    {
        if (player == null) return; // Seguridad

        // Tocar para subirse
        if (playerInRange && !playerSitting && !player.inMenu)
        {
            GetOnChair();
            StopAllCoroutines();
            StartCoroutine(SoftFlash());
            StartCoroutine(SavingText());
        }
        // Tocar para bajarse
        else if (playerSitting && !player.inMenu)
        {
            GetOffChair();
        }
    }
    // -----------------------------


    private void HandleGetOnChair()
    {
        bool pressUp = false;
        
        // Verificar input de teclado/gamepad
        Vector2 keyboardInput = inputMaster.Gameplay.Movement.ReadValue<Vector2>();
        if (keyboardInput.y == 1)
        {
            pressUp = true;
        }
        
        // Verificar input móvil si está disponible
        if (player != null && player.useMobileControls)
        {
            MobileControls mobileControls = MobileControls.instance;
            if (mobileControls != null)
            {
                Vector2 mobileInput = mobileControls.GetMovementInput();
                if (mobileInput.y >= 0.8f) // Un poco más tolerante para controles táctiles
                {
                    pressUp = true;
                }
            }
        }

        if (playerInRange && pressUp && !playerSitting && !player.inMenu)
        {
            GetOnChair();
            StopAllCoroutines();
            StartCoroutine(SoftFlash());
            StartCoroutine(SavingText());
        }
    }

    private void HandleGetOffChair()
    {
        bool pressDown = false;
        
        // Verificar input de teclado/gamepad
        Vector2 keyboardInput = inputMaster.Gameplay.Movement.ReadValue<Vector2>();
        if (keyboardInput.y == -1)
        {
            pressDown = true;
        }
        
        // Verificar input móvil si está disponible
        if (player != null && player.useMobileControls)
        {
            MobileControls mobileControls = MobileControls.instance;
            if (mobileControls != null)
            {
                Vector2 mobileInput = mobileControls.GetMovementInput();
                if (mobileInput.y <= -0.8f) // Un poco más tolerante para controles táctiles
                {
                    pressDown = true;
                }
            }
        }

        if (playerSitting && pressDown && !player.inMenu)
        {
            GetOffChair();
        }
    }

    private void GetOffChair()
    {
        playerSitting = false;
        player.resting = false;
        player.rb.bodyType = RigidbodyType2D.Dynamic;

        // Turn off invulnerable
        int playerLayerId = LayerMask.NameToLayer("Player");
        int EnemyLayerId = LayerMask.NameToLayer("Enemy");
        int EnemyAttackLayerId = LayerMask.NameToLayer("EnemyAttack");
        Physics2D.IgnoreLayerCollision(playerLayerId, EnemyLayerId, false);
        Physics2D.IgnoreLayerCollision(playerLayerId, EnemyAttackLayerId, false);

        interactText.SetActive(true);
    }

    public void GetOnChair()
    {
        playerSitting = true;
        player.resting = true;
        player.RestChairRecovery();
        player.rb.linearVelocity = Vector2.zero;
        player.rb.bodyType = RigidbodyType2D.Kinematic;
        player.transform.position = sittingPos.position;
        LevelLoader.instance.UpdateSpawnPoint(this.gameObject.name);

        // Invulnerable to enemy
        int playerLayerId = LayerMask.NameToLayer("Player");
        int EnemyLayerId = LayerMask.NameToLayer("Enemy");
        int EnemyAttackLayerId = LayerMask.NameToLayer("EnemyAttack");
        Physics2D.IgnoreLayerCollision(playerLayerId, EnemyLayerId, true);
        Physics2D.IgnoreLayerCollision(playerLayerId, EnemyAttackLayerId, true);

        GameMaster.instance.RefillRedTool();

        saveSfx.Play();
        SaveSystem.SavePlayerData();

        interactText.SetActive(false);
    }

    public void RespawnAssignToChair(Player player)
    {
        this.player = player;
    }

    private IEnumerator SoftFlash()
    {
        Color fadeColor = softFlash.color;
        fadeColor.a = (float)(100.0 / 255.0);
        softFlash.color = fadeColor;

        while (fadeColor.a > 0)
        {
            fadeColor.a -= 0.3f * Time.deltaTime;
            softFlash.color = fadeColor;
            yield return null;
        }
    }

    private IEnumerator SavingText()
    {
        savingText.SetActive(true);
        yield return new WaitForSeconds(1f);
        savingText.SetActive(false);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player>())
        {
            playerInRange = true;
            if (!playerSitting)
                interactText.SetActive(true);
        }
    }
}
