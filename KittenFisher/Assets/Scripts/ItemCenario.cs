using System.Collections;
using UnityEngine;

public class ItemCenario : MonoBehaviour
{
    [Header("Configurações do Item")]
    [Tooltip("Se marcado, o item é removido do jogo permanentemente após ser clicado.")]
    public bool destruirAposRevelar = false;

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
        // Se a interação estiver bloqueada pelo Manager ou já estiver vasculhando, ignora o clique
        if (EsconderijosManager.Instance != null && EsconderijosManager.Instance.interacaoBloqueada) return;

        if (!vasculhando)
        {
            StartCoroutine(RevelarEsconderijo());
        }
    }

    IEnumerator RevelarEsconderijo()
    {
        vasculhando = true;

        // Esconde o item do cenário imediatamente
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (colisor2D != null) colisor2D.enabled = false;

        // 🗑️ Se a opção de tirar definitivamente estiver ligada:
        if (destruirAposRevelar)
        {
            Destroy(gameObject); // Remove o objeto da cena permanentemente
            yield break; // Interrompe a Coroutine aqui para não tentar restaurar o item
        }

        // Pega o tempo configurado no EsconderijosManager para reaparecer
        float tempo = EsconderijosManager.Instance != null ? EsconderijosManager.Instance.tempoReveladoPadrao : 3f;

        yield return new WaitForSeconds(tempo);

        // Volta o item para cobrir o peixe/esconderijo
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (colisor2D != null) colisor2D.enabled = true;

        vasculhando = false;
    }
}