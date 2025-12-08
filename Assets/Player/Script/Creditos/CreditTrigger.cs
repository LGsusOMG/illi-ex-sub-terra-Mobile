using UnityEngine;

public class CreditTrigger : MonoBehaviour
{
    [Tooltip("Arrastra aquí el GameObject Panel_Creditos.")]
    public GameObject creditsPanel;

    [Tooltip("Script ScrollingCredits del Text_Container.")]
    public ScrollingCredits scrollingCredits;

    [Tooltip("El tag del jugador (ej. 'Player')")]
    public string playerTag = "Player";

    [Tooltip("¿Pausar el juego durante los créditos?")]
    public bool pauseGame = false;

    [Tooltip("¿Permitir ver los créditos múltiples veces?")]
    public bool allowMultipleViews = false;

    private bool creditsTriggered = false;

    // Para juegos 2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !creditsTriggered)
        {
            ActivateCredits();
        }
    }

    // Para juegos 3D
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !creditsTriggered)
        {
            ActivateCredits();
        }
    }

    private void ActivateCredits()
    {
        creditsTriggered = true;

        // Activar el panel
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }

        // Iniciar la animación de scroll
        if (scrollingCredits != null)
        {
            scrollingCredits.StartCredits();
        }
        else
        {
            Debug.LogError("ScrollingCredits no asignado!");
        }

        // Pausar el juego si se desea
        if (pauseGame)
        {
            Time.timeScale = 0f;
        }

        // Desactivar el trigger solo si NO se permiten múltiples vistas
        if (!allowMultipleViews)
        {
            var col2D = GetComponent<Collider2D>();
            if (col2D != null) col2D.enabled = false;

            var col3D = GetComponent<Collider>();
            if (col3D != null) col3D.enabled = false;

            Debug.Log("Créditos activados. Trigger desactivado (vista única).");
        }
        else
        {
            Debug.Log("Créditos activados. Se pueden ver múltiples veces.");
        }
    }

    // Método público para activar desde otro script (botón, etc.)
    public void TriggerCreditsManually()
    {
        ActivateCredits();
    }
}