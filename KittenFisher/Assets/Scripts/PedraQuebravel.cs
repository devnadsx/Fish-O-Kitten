using System.Collections;
using UnityEngine;

public class PedraQuebravel : MonoBehaviour
{
    [Header("Configurações do Impacto")]
    public int toquesNecessarios = 2;
    private int toquesAtuais = 0;

    [Header("Efeitos")]
    public AudioSource audioSource;
    public AudioClip somPancada;     // Som do 1º clique (impacto)
    public AudioClip somQuebrar;     // Som do 2º clique (destruição)
    public GameObject efeitoParticulas; // (Opcional) Prefab de poeira/pedrinhas caindo

    [Header("Mudança Visual (Opcional)")]
    public Sprite spritePedraTrincada; // Sprite da pedra rachiada após o 1º impacto

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
        // Se o jogo estiver travado por diálogo ou se a pedra estiver sendo destruída, ignora
        if (EsconderijosManager.Instance != null && EsconderijosManager.Instance.interacaoBloqueada) return;
        if (estaQuebrando) return;

        // 🛑 VERIFICAÇÃO: O jogador precisa ter a picareta!
        if (GerenciadorInventario.Instance != null && !GerenciadorInventario.Instance.temPicareta)
        {
            // Fala rápida avisando que precisa de uma ferramenta
            if (DialogoManager.Instance != null)
            {
                DialogoManager.Instance.LimparDialogo();
                DialogoManager.Instance.AdicionarFala("This rock is too hard! I need a pickaxe to break it.", false);
            }
            return;
        }

        // Se tem a picareta, processa o clique:
        RegistrarClique();
    }

    void RegistrarClique()
    {
        toquesAtuais++;

        if (toquesAtuais < toquesNecessarios)
        {
            // 🔨 Primeiro clique: Trinca a pedra
            TocarSom(somPancada);

            if (spritePedraTrincada != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = spritePedraTrincada;
            }

            // Animação simples de tremer a pedra no 1º impacto
            StartCoroutine(EfeitoTremer());
        }
        else
        {
            // 💥 Segundo clique: Destrói a pedra
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

        // Desativa o colisor e o sprite
        if (colisor2D != null) colisor2D.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        // Spawna partículas de quebra se configurado
        if (efeitoParticulas != null)
        {
            Instantiate(efeitoParticulas, transform.position, Quaternion.identity);
        }

        // Aguarda o som de destruição terminar antes de remover o objeto da hierarquia
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