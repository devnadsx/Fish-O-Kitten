
using System.Collections;
using UnityEngine;

public class ItemCenario : MonoBehaviour
{
    [Header("Configurações do Item")]
    [Tooltip("Se marcado, o item é removido do jogo permanentemente após ser clicado.")]
    public bool destruirAposRevelar = false;

    [Header("Animação de Respawn")]
    public bool ehAlga = false;
    public float duracaoAnimacao = 0.3f;

    [Header("Sons de Interação")]
    public AudioSource audioSource;
    public AudioClip somAoClicar;
    public AudioClip somAoRespawnar;

    [Header("Proteção de Clique")]
    [Tooltip("Tempo em segundos que o item ignora cliques logo após reaparecer.")]
    public float tempoImunidadeClique = 0.2f;

    private SpriteRenderer spriteRenderer;
    private Collider2D colisor2D;
    private bool vasculhando = false;
    private bool podeClicar = true;

    private Vector3 escalaOriginal;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisor2D = GetComponent<Collider2D>();

        escalaOriginal = transform.localScale;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void OnMouseDown()
    {
        if (EsconderijosManager.Instance != null &&
            EsconderijosManager.Instance.interacaoBloqueada) return;

        if (!podeClicar || vasculhando) return;

        StartCoroutine(RevelarEsconderijo());
    }

    IEnumerator RevelarEsconderijo()
    {
        vasculhando = true;

        TocarSom(somAoClicar);

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (colisor2D != null) colisor2D.enabled = false;

        if (destruirAposRevelar)
        {
            if (audioSource != null && somAoClicar != null)
            {
                yield return new WaitForSeconds(somAoClicar.length);
            }

            Destroy(gameObject);
            yield break;
        }

        float tempo = EsconderijosManager.Instance != null
            ? EsconderijosManager.Instance.tempoReveladoPadrao
            : 3f;

        yield return new WaitForSeconds(tempo);

        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (colisor2D != null) colisor2D.enabled = true;

        if (ehAlga)
        {
            yield return StartCoroutine(AnimarRespawn());
        }

        TocarSom(somAoRespawnar);

        podeClicar = false;
        yield return new WaitForSeconds(tempoImunidadeClique);
        podeClicar = true;

        vasculhando = false;
    }

    IEnumerator AnimarRespawn()
    {
        float tempo = 0f;

        transform.localScale = Vector3.zero;

        while (tempo < duracaoAnimacao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracaoAnimacao);

            transform.localScale = Vector3.Lerp(
                Vector3.zero,
                escalaOriginal,
                progresso
            );

            yield return null;
        }

        transform.localScale = escalaOriginal;
    }

    private void TocarSom(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
