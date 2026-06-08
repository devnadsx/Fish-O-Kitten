using System.Collections; // IMPORTANTE: Necessário para usar Coroutines
using UnityEngine;

public class ItemCenario : MonoBehaviour
{
    [Header("Configurações de Tempo")]
    public float tempoRevelado = 3f; // Quantos segundos o item some antes de voltar

    private SpriteRenderer spriteRenderer;
    private Collider2D colisor2D; // Mude para 'Collider' se seu jogo for 3D
    private bool vasculhando = false;

    void Start()
    {
        // Pega os componentes do próprio objeto do cenário automaticamente
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisor2D = GetComponent<Collider2D>();
    }

    void OnMouseDown()
    {
        // Só ativa se o objeto já não estiver sumido
        if (!vasculhando)
        {
            StartCoroutine(RevelarEsconderijo());
        }
    }

    IEnumerator RevelarEsconderijo()
    {
        vasculhando = true;

        // 1. Esconde o objeto do cenário e desliga o clique dele
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (colisor2D != null) colisor2D.enabled = false;

        Debug.Log(gameObject.name + " sumiu! Pegue o peixe rápido!");

        // 2. Espera o tempo que você configurou no Inspector
        yield return new WaitForSeconds(tempoRevelado);

        // 3. Traz o objeto do cenário de volta (cobrindo o peixe de novo se ele não foi pego)
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (colisor2D != null) colisor2D.enabled = true;

        vasculhando = false;
        Debug.Log(gameObject.name + " voltou a cobrir o esconderijo.");
    }
}