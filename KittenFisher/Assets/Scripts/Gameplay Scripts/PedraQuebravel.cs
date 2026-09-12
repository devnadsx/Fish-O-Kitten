using System.Collections;
using System.Collections.Generic; // Necessário para usar List
using UnityEngine;

public class PedraQuebravel : MonoBehaviour
{
    [Header("Configurações do Impacto")]
    public int toquesNecessarios = 2;
    private int toquesAtuais = 0;

    [Header("Visual")]
    [Tooltip("Arraste aqui o Sprite da pedra rachada.")]
    public Sprite spriteRachado;

    [Header("Sons")]
    public AudioSource audioSource;
    public AudioClip somPancada;
    public AudioClip somQuebrar;

    private SpriteRenderer spriteRenderer;
    private Collider2D colisor2D;
    private bool estaQuebrando = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisor2D = GetComponent<Collider2D>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnMouseDown()
    {
        // 🔒 BLOQUEIO: Só funciona se o jogador tiver a picareta
        if (GerenciadorInventario.Instance != null && !GerenciadorInventario.Instance.temPicareta)
        {
            if (DialogoManager.Instance != null)
            {
                // Cria a fala usando a nova estrutura do DialogoManager
                List<LineDialogo> falaSemPicareta = new List<LineDialogo>()
                {
                    new LineDialogo
                    {
                        nomeQuemFala = "Gatinho",
                        texto = "This rock is too hard! I need a pickaxe to break it.",
                        ehNarracao = false,
                        posicao = PosicaoPersonagem.Centro
                    }
                };

                DialogoManager.Instance.IniciarSequenciaDialogo(falaSemPicareta);
            }
            return;
        }

        if (EsconderijosManager.Instance != null && EsconderijosManager.Instance.interacaoBloqueada) return;
        if (estaQuebrando) return;

        ProcessarClique();
    }

    void ProcessarClique()
    {
        toquesAtuais++;

        if (toquesAtuais == 1)
        {
            TocarSom(somPancada);

            if (spriteRachado != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = spriteRachado;
            }

            StartCoroutine(EfeitoTremer());
        }
        else if (toquesAtuais >= toquesNecessarios)
        {
            StartCoroutine(QuebrarPedra());
        }
    }

    IEnumerator EfeitoTremer()
    {
        Vector3 posOriginal = transform.position;
        float duracao = 0.15f;
        float tempo = 0;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            transform.position = posOriginal + (Vector3)Random.insideUnitCircle * 0.08f;
            yield return null;
        }

        transform.position = posOriginal;
    }

    IEnumerator QuebrarPedra()
    {
        estaQuebrando = true;
        TocarSom(somQuebrar);

        if (colisor2D != null) colisor2D.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        if (audioSource != null && somQuebrar != null)
        {
            yield return new WaitForSeconds(somQuebrar.length);
        }

        Destroy(gameObject);
    }

    private void TocarSom(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}