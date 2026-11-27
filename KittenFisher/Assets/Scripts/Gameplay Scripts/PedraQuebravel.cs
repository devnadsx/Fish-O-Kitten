
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedraQuebravel : MonoBehaviour
{
    [Header("Configurações do Impacto")]
    public int toquesNecessarios = 2;
    private int toquesAtuais = 0;

    [Header("Visual")]
    [Tooltip("Arraste aqui o Sprite da pedra rachada.")]
    public Sprite spriteRachado;

    [Header("Partículas")]
    public ParticleSystem particulasImpacto;
    public ParticleSystem particulasQuebra;

    [Header("Tremor")]
    public float duracaoTremor = 0.15f;
    public float intensidadeTremor = 0.08f;
    public float intensidadeTremorFinal = 0.15f;

    [Header("Sons")]
    public AudioSource audioSource;
    public AudioClip somPancada;
    public AudioClip somQuebrar;

    private SpriteRenderer spriteRenderer;
    private Collider2D colisor2D;
    private bool estaQuebrando = false;
    private Vector3 posicaoOriginal;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisor2D = GetComponent<Collider2D>();

        posicaoOriginal = transform.position;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnMouseDown()
    {
        if (GerenciadorInventario.Instance != null &&
            !GerenciadorInventario.Instance.temPicareta)
        {
            if (DialogoManager.Instance != null)
            {
                List<LineDialogo> falaSemPicareta = new List<LineDialogo>()
                {
                    new LineDialogo
                    {
                        nomeQuemFala = "Myke",
                        texto = "Ugh... Preciso de uma picareta para quebrar essa pedra.",
                        ehNarracao = false,
                        posicao = PosicaoPersonagem.Centro
                    }
                };

                DialogoManager.Instance.IniciarSequenciaDialogo(falaSemPicareta);
            }
            return;
        }

        if (EsconderijosManager.Instance != null &&
            EsconderijosManager.Instance.interacaoBloqueada) return;

        if (estaQuebrando) return;

        ProcessarClique();
    }

    void ProcessarClique()
    {
        toquesAtuais++;

        if (particulasImpacto != null)
            particulasImpacto.Play();

        if (toquesAtuais >= toquesNecessarios)
        {
            StartCoroutine(QuebrarPedra());
        }
        else
        {
            TocarSom(somPancada);

            if (spriteRachado != null && spriteRenderer != null)
                spriteRenderer.sprite = spriteRachado;

            StartCoroutine(EfeitoTremer(intensidadeTremor));
        }
    }

    IEnumerator EfeitoTremer(float intensidade)
    {
        float tempo = 0f;

        while (tempo < duracaoTremor)
        {
            tempo += Time.deltaTime;

            transform.position = posicaoOriginal +
                (Vector3)Random.insideUnitCircle * intensidade;

            yield return null;
        }

        transform.position = posicaoOriginal;
    }

    IEnumerator QuebrarPedra()
    {
        estaQuebrando = true;

        TocarSom(somQuebrar);

        StartCoroutine(EfeitoTremer(intensidadeTremorFinal));

        if (particulasQuebra != null)
            particulasQuebra.Play();

        if (colisor2D != null)
            colisor2D.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (audioSource != null && somQuebrar != null)
            yield return new WaitForSeconds(somQuebrar.length);
        else
            yield return new WaitForSeconds(duracaoTremor);

        Destroy(gameObject);
    }

    private void TocarSom(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
