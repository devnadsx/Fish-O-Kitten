using System.Collections;
using UnityEngine;

public class ItemCenario : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D colisor2D;
    private bool vasculhando = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisor2D = GetComponent<Collider2D>();
    }

    void OnMouseDown()
    {
        // Se a interação estiver bloqueada pelo Manager (ex: durante o diálogo) ou já estiver sumido, não faz nada!
        if (EsconderijosManager.Instance != null && EsconderijosManager.Instance.interacaoBloqueada) return;

        if (!vasculhando)
        {
            StartCoroutine(RevelarEsconderijo());
        }
    }

    IEnumerator RevelarEsconderijo()
    {
        vasculhando = true;

        // Esconde o item do cenário
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (colisor2D != null) colisor2D.enabled = false;

        // Pega o tempo configurado no EsconderijosManager
        float tempo = EsconderijosManager.Instance != null ? EsconderijosManager.Instance.tempoReveladoPadrao : 3f;

        yield return new WaitForSeconds(tempo);

        // Volta o item para cobrir o peixe
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (colisor2D != null) colisor2D.enabled = true;

        vasculhando = false;
    }
}