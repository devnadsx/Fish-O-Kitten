using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    public Color corInativo = new Color(0.5f, 0.5f, 0.5f, 1f);
    public AudioSource audioSourceSFX;
    public AudioClip somFalaPadrao;

    [Header("Transição de Cena Final")]
    public string nomeProximaCena = "End";

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

    // Posições originais fixas travadas no início do jogo para evitar acúmulo de pulos
    private Vector2 posFixaEsquerda;
    private Vector2 posFixaCentro;
    private Vector2 posFixaDireita;

    void Awake()
    {
        if (Instance == null) Instance = this;

        // Salva as posições originais dos RectTransforms para os pulos serem absolutos
        if (imagemEsquerda != null) posFixaEsquerda = imagemEsquerda.rectTransform.anchoredPosition;
        if (imagemCentro != null) posFixaCentro = imagemCentro.rectTransform.anchoredPosition;
        if (imagemDireita != null) posFixaDireita = imagemDireita.rectTransform.anchoredPosition;
    }

    void Start()
    {
        // Garante que os personagens na tela estejam visíveis (sem ocultar)
        GarantirPersonagensVisiveis();

        if (falasIniciais != null && falasIniciais.Count > 0)
        {
            IniciarSequenciaDialogo(falasIniciais);
        }
    }

    void Update()
    {
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

    public void AvancarTexto()
    {
        if (Time.time - tempoUltimoClique < intervaloMinimoClique) return;
        tempoUltimoClique = Time.time;

        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        if (filaFalas.Count > 0)
        {
            ExibirProximaFrase();
        }
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

        // Posição de origem limpa e imutável do slot ativo
        Vector2 posOrigemSlot = ObterPosicaoFixaSlot(linhaAtual.posicao);
        RectTransform rectPersonagem = imagemAtivaAtual != null ? imagemAtivaAtual.rectTransform : null;

        // Garante reset de posição no início da frase
        if (rectPersonagem != null)
        {
            rectPersonagem.anchoredPosition = posOrigemSlot;
        }

        foreach (char letra in linhaAtual.texto.ToCharArray())
        {
            if (textoBalao != null) textoBalao.text += letra;

            if (!linhaAtual.ehNarracao && char.IsLetterOrDigit(letra))
            {
                // Aplica pulo relativo à posição original fixa
                if (rectPersonagem != null)
                {
                    rectPersonagem.anchoredPosition = posOrigemSlot + new Vector2(0, alturaPuloLetra);
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

            // Retorna estritamente à posição base travada
            if (rectPersonagem != null)
            {
                rectPersonagem.anchoredPosition = posOrigemSlot;
            }
            RestaurarSpriteBocaFechada();
        }

        // Dupla garantia ao término da frase
        if (rectPersonagem != null)
        {
            rectPersonagem.anchoredPosition = posOrigemSlot;
        }
        RestaurarSpriteBocaFechada();
        estaEscrevendo = false;
    }

    void CompletarTextoImediatamente()
    {
        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        if (textoBalao != null) textoBalao.text = textoCompletoAtual;

        // Força a restauração exata de posições e boca fechada
        ResetarPosicoesDeTodosSlots();
        RestaurarSpriteBocaFechada();
        estaEscrevendo = false;
    }

    private void FinalizarEDevolverCena()
    {
        FecharDialogo();

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

    private Vector2 ObterPosicaoFixaSlot(PosicaoPersonagem posicao)
    {
        switch (posicao)
        {
            case PosicaoPersonagem.Esquerda: return posFixaEsquerda;
            case PosicaoPersonagem.Direita: return posFixaDireita;
            case PosicaoPersonagem.Centro: return posFixaCentro;
            default: return posFixaCentro;
        }
    }

    private void AtualizarDestaquePersonagens(Image slotAtivo, bool ehNarracao)
    {
        Image[] slots = { imagemEsquerda, imagemCentro, imagemDireita };

        foreach (var slot in slots)
        {
            if (slot == null) continue;

            // Mantém os GameObjects visíveis se tiverem Sprite atrelado
            if (slot.sprite != null)
            {
                slot.gameObject.SetActive(true);
            }

            if (slot == slotAtivo && !ehNarracao)
            {
                // Destaque para quem está falando
                slot.color = personagemAtivoAtual.corFoco != Color.clear ? personagemAtivoAtual.corFoco : Color.white;
                if (personagemAtivoAtual.spriteBocaFechada != null)
                {
                    slot.sprite = personagemAtivoAtual.spriteBocaFechada;
                }
            }
            else
            {
                // Fica levemente escurecido enquanto ouve ou narra
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

    private void GarantirPersonagensVisiveis()
    {
        Image[] slots = { imagemEsquerda, imagemCentro, imagemDireita };
        foreach (var slot in slots)
        {
            if (slot != null && slot.sprite != null)
            {
                slot.gameObject.SetActive(true);
                slot.color = corInativo;
            }
        }
    }

    private void ResetarPosicoesDeTodosSlots()
    {
        if (imagemEsquerda != null) imagemEsquerda.rectTransform.anchoredPosition = posFixaEsquerda;
        if (imagemCentro != null) imagemCentro.rectTransform.anchoredPosition = posFixaCentro;
        if (imagemDireita != null) imagemDireita.rectTransform.anchoredPosition = posFixaDireita;
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
        ResetarPosicoesDeTodosSlots();
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
    }
}