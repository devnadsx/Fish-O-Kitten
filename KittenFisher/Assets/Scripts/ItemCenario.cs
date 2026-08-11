using System.Collections;
using UnityEngine;

public class ItemCenario : MonoBehaviour
{
    [Header("Configurações do Item")]
    [Tooltip("Se marcado, o item é removido do jogo permanentemente após ser clicado.")]
    public bool destruirAposRevelar = false;

    [Header("Sons de Interação")]
    public AudioSource audioSource;
    public AudioClip somAoClicar;
    public AudioClip somAoRespawnar;

    [Header("Proteção de Clique")]
    [Tooltip("Tempo em segundos que o item ignora cliques logo após reaparecer (evita o clique fantasma do armário).")]
    public float tempoImunidadeClique = 0.2f;

    private SpriteRenderer spriteRenderer;
    private Collider2D colisor2D;
    private bool vasculhando = false;
    private bool podeClicar = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisor2D = GetComponent<Collider2D>();

        // Tenta pegar o AudioSource se não tiver sido arrastado no Inspector
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void OnMouseDown()
    {
        // Trava 1: Se a interação global estiver bloqueada pelo Manager
        if (EsconderijosManager.Instance != null && EsconderijosManager.Instance.interacaoBloqueada) return;

        // Trava 2: Se o item estiver em tempo de recarga de imunidade a cliques ou já estiver vasculhando
        if (!podeClicar || vasculhando) return;

        StartCoroutine(RevelarEsconderijo());
    }

    IEnumerator RevelarEsconderijo()
    {
        vasculhando = true;

        // 🔊 Toca o som ao clicar/revelar
        TocarSom(somAoClicar);

        // Esconde o item do cenário imediatamente
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (colisor2D != null) colisor2D.enabled = false;

        // 🗑️ Se a opção de tirar definitivamente estiver ligada:
        if (destruirAposRevelar)
        {
            // Se houver som ao clicar, espera o som tocar antes de destruir o objeto
            if (audioSource != null && somAoClicar != null)
            {
                yield return new WaitForSeconds(somAoClicar.length);
            }

            Destroy(gameObject); // Remove o objeto da cena permanentemente
            yield break;
        }

        // Pega o tempo configurado no EsconderijosManager para reaparecer
        float tempo = EsconderijosManager.Instance != null ? EsconderijosManager.Instance.tempoReveladoPadrao : 3f;

        yield return new WaitForSeconds(tempo);

        // Volta o item para cobrir o esconderijo
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (colisor2D != null) colisor2D.enabled = true;

        // 🔊 Toca o som de respawn
        TocarSom(somAoRespawnar);

        // 🛑 Ativa a proteção para não pegar o clique "sem querer" que abriu o esconderijo
        podeClicar = false;
        yield return new WaitForSeconds(tempoImunidadeClique);
        podeClicar = true;

        vasculhando = false;
    }

    private void TocarSom(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}