using System.Collections;
using UnityEngine;

public class PedraQuebravel : MonoBehaviour
{
    [Header("Configurações do Impacto")]
    public int toquesNecessarios = 2;
    private int toquesAtuais = 0;

    [Header("Visual")]
    [Tooltip("Arraste aqui a versão rachada dessa mesma pedra (ou o overlay de rachadura).")]
    public GameObject objetoRachadura; // Pode ser um objeto filho com o Sprite rachado ou a variação do sprite

    [Header("Sons")]
    public AudioSource audioSource;
    public AudioClip somPancada;     // Som do 1º impacto (quando racha)
    public AudioClip somQuebrar;     // Som do 2º impacto (quando destrói)

    private SpriteRenderer spriteRenderer;
    private Collider2D colisor2D;
    private bool estaQuebrando = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisor2D = GetComponent<Collider2D>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Garante que o efeito/sprite de rachadura comece escondido
        if (objetoRachadura != null)
        {
            objetoRachadura.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        // 🔒 BLOQUEIO 1: Só funciona se o jogador JÁ tiver a picareta no inventário
        if (GerenciadorInventario.Instance != null && !GerenciadorInventario.Instance.temPicareta)
        {
            // Opcional: Se quiser que o gato dê uma dica quando clicar na pedra sem a picareta
            if (DialogoManager.Instance != null)
            {
                DialogoManager.Instance.LimparDialogo();
                DialogoManager.Instance.AdicionarFala("This rock is too hard! I need a pickaxe to break it.", false);
            }
            return; // Interrompe o código aqui, impedindo qualquer clique ou quebra!
        }

        // Trava de segurança para diálogos ativos ou animação em andamento
        if (EsconderijosManager.Instance != null && EsconderijosManager.Instance.interacaoBloqueada) return;
        if (estaQuebrando) return;

        // Se chegou até aqui, o player possui a picareta!
        ProcessarClique();
    }

    void ProcessarClique()
    {
        toquesAtuais++;

        if (toquesAtuais == 1)
        {
            // 🔨 1º Clique: Ativa o aspecto rachado e toca som
            TocarSom(somPancada);

            if (objetoRachadura != null)
            {
                objetoRachadura.SetActive(true);
            }

            StartCoroutine(EfeitoTremer());
        }
        else if (toquesAtuais >= toquesNecessarios)
        {
            // 💥 2º Clique: Quebra e destrói a pedra
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

        // Esconde a pedra da tela
        if (colisor2D != null) colisor2D.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (objetoRachadura != null) objetoRachadura.SetActive(false);

        // Aguarda o som de quebrar tocar antes de destruir o objeto
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