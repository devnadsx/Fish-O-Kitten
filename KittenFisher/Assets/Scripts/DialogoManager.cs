using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Adicionado para carregar a próxima cena
using TMPro;

public enum PosicaoPersonagem
{
    Esquerda,
    Direita,
    Centro
}

[System.Serializable]
public struct DadosPersonagem
{
    public string nomePersonagem;
    public Sprite spriteBocaFechada;
    public Sprite spriteBocaAberta;
    public Color corFoco;
}

[System.Serializable]
public struct LineDialogo
{
    public string nomeQuemFala;
    public string texto;
    public bool ehNarracao;
    public PosicaoPersonagem posicao;
    public AudioClip somFala;
}

public class DialogoManager : MonoBehaviour
{
    public static DialogoManager Instance;

    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoBalao;

    [Header("Slots de Imagem para Personagens na Tela")]
    public Image imagemEsquerda;
    public Image imagemCentro;
    public Image imagemDireita;

    [Header("Banco de Dados de Personagens")]
    public List<DadosPersonagem> listaPersonagens = new List<DadosPersonagem>();

    [Header("Configurações de Animação e Efeitos")]
    public float velocidadeEscrita = 0.03f;
    public float alturaPuloLetra = 6f;
    public Color corInativo = new Color(0.5f, 0.5f, 0.5f);
    public AudioSource audioSourceSFX;
    public AudioClip somFalaPadrao;

    [Header("Transição de Cena Final")]
    public string nomeProximaCena = "End"; // Digite aqui o nome da cena final no Inspector

    [Header("Falas Iniciais (Inspector)")]
    public List<LineDialogo> falasIniciais = new List<LineDialogo>();

    private Queue<LineDialogo> filaFalas = new Queue<LineDialogo>();
    private Coroutine coroutineDigitacao;
    private bool estaEscrevendo = false;
    private string textoCompletoAtual = "";

    private Image imagemAtivaAtual;
    private DadosPersonagem personagemAtivoAtual;
    private float tempoUltimoClique = 0f;
    private float intervaloMinimoClique = 0.15f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        EsconderTodasImagens();

        if (falasIniciais != null && falasIniciais.Count > 0)
        {
            IniciarSequenciaDialogo(falasIniciais);
        }
    }

    void Update()
    {
        // Aceita o clique com Espaço ou Enter além do botão na UI
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            AvancarTexto();
        }
    }

    public void IniciarSequenciaDialogo(List<LineDialogo> listaFalas)
    {
        LimparDialogo();

        foreach (var fala in listaFalas)
        {
            filaFalas.Enqueue(fala);
        }

        ExibirProximaFrase();
    }

    // Função que deve ser associada ao evento OnClick() do seu Botão
    public void AvancarTexto()
    {
        if (Time.time - tempoUltimoClique < intervaloMinimoClique) return;
        tempoUltimoClique = Time.time;

        // 1. Se o texto ainda está sendo digitado na tela, completa a frase na hora
        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        // 2. Se ainda existem falas na fila, avança para a próxima
        if (filaFalas.Count > 0)
        {
            ExibirProximaFrase();
        }
        // 3. Se NÃO tem mais falas disponíveis, fecha o diálogo e troca para a próxima cena
        else
        {
            FinalizarEDevolverCena();
        }
    }

    public void ExibirProximaFrase()
    {
        if (filaFalas.Count == 0) return;

        LineDialogo falaAtual = filaFalas.Dequeue();

        if (painelBalaoFala != null) painelBalaoFala.SetActive(true);

        textoCompletoAtual = falaAtual.texto;

        if (textoNomePersonagem != null)
        {
            textoNomePersonagem.text = falaAtual.ehNarracao ? "" : falaAtual.nomeQuemFala;
        }

        personagemAtivoAtual = ObterDadosPersonagem(falaAtual.nomeQuemFala);
        imagemAtivaAtual = ObterSlotImagem(falaAtual.posicao);

        AtualizarDestaquePersonagens(imagemAtivaAtual, falaAtual.ehNarracao);

        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        coroutineDigitacao = StartCoroutine(EfeitoDigitarTexto(falaAtual));
    }

    IEnumerator EfeitoDigitarTexto(LineDialogo linhaAtual)
    {
        estaEscrevendo = true;
        if (textoBalao != null) textoBalao.text = "";

        Vector3 posOriginalPersonagem = Vector3.zero;
        RectTransform rectPersonagem = null;

        if (imagemAtivaAtual != null)
        {
            rectPersonagem = imagemAtivaAtual.rectTransform;
            posOriginalPersonagem = rectPersonagem.anchoredPosition;
        }

        foreach (char letra in linhaAtual.texto.ToCharArray())
        {
            if (textoBalao != null) textoBalao.text += letra;

            if (char.IsLetterOrDigit(letra))
            {
                if (rectPersonagem != null)
                {
                    rectPersonagem.anchoredPosition = posOriginalPersonagem + new Vector3(0, alturaPuloLetra, 0);
                }

                if (imagemAtivaAtual != null && personagemAtivoAtual.spriteBocaAberta != null)
                {
                    imagemAtivaAtual.sprite = personagemAtivoAtual.spriteBocaAberta;
                }

                AudioClip clipParaTocar = linhaAtual.somFala != null ? linhaAtual.somFala : somFalaPadrao;
                if (audioSourceSFX != null && clipParaTocar != null)
                {
                    audioSourceSFX.PlayOneShot(clipParaTocar);
                }
            }

            yield return new WaitForSeconds(velocidadeEscrita);

            if (rectPersonagem != null)
            {
                rectPersonagem.anchoredPosition = posOriginalPersonagem;
            }
            RestaurarSpriteBocaFechada();
        }

        if (rectPersonagem != null)
        {
            rectPersonagem.anchoredPosition = posOriginalPersonagem;
        }
        RestaurarSpriteBocaFechada();
        estaEscrevendo = false;
    }

    void CompletarTextoImediatamente()
    {
        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        if (textoBalao != null) textoBalao.text = textoCompletoAtual;

        RestaurarSpriteBocaFechada();
        estaEscrevendo = false;
    }

    private void FinalizarEDevolverCena()
    {
        FecharDialogo();

        // Verifica se o nome da cena foi digitado no Inspector e carrega a nova cena
        if (!string.IsNullOrEmpty(nomeProximaCena))
        {
            SceneManager.LoadScene(nomeProximaCena);
        }
    }

    private DadosPersonagem ObterDadosPersonagem(string nome)
    {
        foreach (var p in listaPersonagens)
        {
            if (p.nomePersonagem.ToLower() == nome.ToLower()) return p;
        }
        return new DadosPersonagem { nomePersonagem = nome, corFoco = Color.white };
    }

    private Image ObterSlotImagem(PosicaoPersonagem posicao)
    {
        switch (posicao)
        {
            case PosicaoPersonagem.Esquerda: return imagemEsquerda;
            case PosicaoPersonagem.Direita: return imagemDireita;
            case PosicaoPersonagem.Centro: return imagemCentro;
            default: return imagemCentro;
        }
    }

    private void AtualizarDestaquePersonagens(Image slotAtivo, bool ehNarracao)
    {
        Image[] slots = { imagemEsquerda, imagemCentro, imagemDireita };

        foreach (var slot in slots)
        {
            if (slot == null) continue;

            if (slot == slotAtivo && !ehNarracao)
            {
                slot.gameObject.SetActive(true);
                slot.color = personagemAtivoAtual.corFoco != Color.clear ? personagemAtivoAtual.corFoco : Color.white;
                if (personagemAtivoAtual.spriteBocaFechada != null) slot.sprite = personagemAtivoAtual.spriteBocaFechada;
            }
            else if (slot.gameObject.activeSelf)
            {
                slot.color = corInativo;
            }
        }
    }

    private void RestaurarSpriteBocaFechada()
    {
        if (imagemAtivaAtual != null && personagemAtivoAtual.spriteBocaFechada != null)
        {
            imagemAtivaAtual.sprite = personagemAtivoAtual.spriteBocaFechada;
        }
    }

    private void EsconderTodasImagens()
    {
        if (imagemEsquerda != null) imagemEsquerda.gameObject.SetActive(false);
        if (imagemCentro != null) imagemCentro.gameObject.SetActive(false);
        if (imagemDireita != null) imagemDireita.gameObject.SetActive(false);
    }

    public bool TemFalasPendentes()
    {
        return filaFalas.Count > 0 || estaEscrevendo || (painelBalaoFala != null && painelBalaoFala.activeSelf);
    }

    public void LimparDialogo()
    {
        filaFalas.Clear();
    }

    public void FecharDialogo()
    {
        LimparDialogo();
        EsconderTodasImagens();
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
    }
}