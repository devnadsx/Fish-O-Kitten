using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use TextMeshProUGUI se estiver usando TMP, ou 'using UnityEngine.UI;' se for Text padrão

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Contador de Peixes")]
    public int peixesColetados = 0;
    public TextMeshProUGUI textoContadorPeixes; // O texto da UI do contador!

    [Header("Animação do Ícone (Juice)")]
    public RectTransform iconeInventario;     // Arraste o RectTransform do Ícone do Inventário
    public float intensidadePulo = 1.25f;      // O quanto ele aumenta de tamanho rapidinho
    public float duracaoPulo = 0.15f;         // Duração do efeito (segundos)

    [Header("Efeitos Sonoros")]
    public AudioSource audioSource;
    public AudioClip somColetaPeixe;

    private Vector3 escalaOriginal;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (iconeInventario != null)
        {
            escalaOriginal = iconeInventario.localScale;
        }

        AtualizarTextoContador();
    }

    // Função chamada toda vez que o jogador pesca/coleta um peixe!
    public void AdicionarPeixe()
    {
        peixesColetados++;
        AtualizarTextoContador();

        // 1. Toca o Som
        if (audioSource != null && somColetaPeixe != null)
        {
            audioSource.PlayOneShot(somColetaPeixe);
        }

        // 2. Faz o ícone balançar/pular
        if (iconeInventario != null)
        {
            StopAllCoroutines();
            StartCoroutine(EfeitoPuloEscale());
        }
    }

    void AtualizarTextoContador()
    {
        if (textoContadorPeixes != null)
        {
            textoContadorPeixes.text = peixesColetados.ToString();
            // Se preferir mostrar tipo "x3", basta mudar para: $"x{peixesColetados}"
        }
    }

    // --- COROUTINE DO EFEITO VISUAL (Pulinho no Ícone) ---
    IEnumerator EfeitoPuloEscale()
    {
        Vector3 escalaPulo = escalaOriginal * intensidadePulo;
        float tempo = 0f;

        // Aumenta o tamanho do ícone
        while (tempo < duracaoPulo / 2f)
        {
            tempo += Time.deltaTime;
            iconeInventario.localScale = Vector3.Lerp(escalaOriginal, escalaPulo, tempo / (duracaoPulo / 2f));
            yield return null;
        }

        tempo = 0f;

        // Volta ao tamanho original
        while (tempo < duracaoPulo / 2f)
        {
            tempo += Time.deltaTime;
            iconeInventario.localScale = Vector3.Lerp(escalaPulo, escalaOriginal, tempo / (duracaoPulo / 2f));
            yield return null;
        }

        iconeInventario.localScale = escalaOriginal;
    }
}