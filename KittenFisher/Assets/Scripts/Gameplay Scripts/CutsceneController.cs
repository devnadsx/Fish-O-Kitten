using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum PosicaoCutscene
{
    Esquerda,
    Centro,
    Direita
}

[System.Serializable]

public struct PersonagemCutscene
{
    public string nome;
    public Sprite spriteBocaFechada;
    public Sprite spriteBocaAberta;
    public Color corFoco;
}

[System.Serializable]
public struct FalaCutscene
{
    public string nomeQuemFala;
    public bool ehNarracao; // Se true, escurece os personagens e não mexe a boca
    public PosicaoCutscene posicao;
    [TextArea(2, 5)]
    public string texto;
    public Sprite imagemFundo;
    public AudioClip somFala;
}

public class CutsceneController : MonoBehaviour
{
    [Header("Componentes de UI - Geral")]
    public Image imagemFundo;
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoBalao;
    public CanvasGroup canvasGroupFade;

    [Header("Slots de Imagem para Personagens")]
    public Image imagemEsquerda;
    public Image imagemCentro;
    public Image imagemDireita;

    [Header("Áudio")]
    public AudioSource audioSourceSFX;
    public AudioClip somFalaPadrao;

    [Header("Configurações de Animação e Cores")]
    public float duracaoFade = 0.5f;
    public float velocidadeEscrita = 0.03f;
    public float alturaPuloLetra = 6f;
    public Color corInativo = new Color(0.5f, 0.5f, 0.5f, 1f);
    public string nomeProximaCena = "End";

    [Header("Banco de Dados de Personagens")]
    public List<PersonagemCutscene> listaPersonagens = new List<PersonagemCutscene>();

    [Header("Lista de Falas da Lore (Inspector)")]
    public List<FalaCutscene> falas = new List<FalaCutscene>();

    private Queue<FalaCutscene> filaFalas = new Queue<FalaCutscene>();
    private FalaCutscene falaAtual;
    private int indiceAtual = 0;

    private bool estaEmTransicao = false;
    private bool estaEscrevendo = false;
    private Coroutine coroutineEscrita;

    private Image imagemAtivaAtual;
    private PersonagemCutscene personagemAtivoAtual;

    private Vector2 posFixaEsquerda;
    private Vector2 posFixaCentro;
    private Vector2 posFixaDireita;

    private float tempoUltimoClique = 0f;
    private float intervaloMinimoClique = 0.15f;

    void Awake()
    {
        // Salva as posições fixas originais para o pulo não acumular erro
        if (imagemEsquerda != null) posFixaEsquerda = imagemEsquerda.rectTransform.anchoredPosition;
        if (imagemCentro != null) posFixaCentro = imagemCentro.rectTransform.anchoredPosition;
        if (imagemDireita != null) posFixaDireita = imagemDireita.rectTransform.anchoredPosition;
    }

    void Start()
    {
        if (canvasGroupFade != null)
        {
            canvasGroupFade.alpha = 0f;
            canvasGroupFade.blocksRaycasts = false;
        }

        if (falas != null && falas.Count > 0)
        {
            // Aplica o primeiro fundo da história
            if (falas[0].imagemFundo != null && imagemFundo != null)
            {
                imagemFundo.sprite = falas[0].imagemFundo;
            }

            // Enfileira as falas
            foreach (var fala in falas)
            {
                filaFalas.Enqueue(fala);
            }

            ExibirProximaFala();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            Avançar();
        }
    }

    public void Avançar()
    {
        if (estaEmTransicao) return;

        // Debounce de clique
        if (Time.time - tempoUltimoClique < intervaloMinimoClique) return;
        tempoUltimoClique = Time.time;

        // Se estiver digitando, completa o texto instantaneamente
        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        if (filaFalas.Count > 0)
        {
            FalaCutscene proximaFala = filaFalas.Peek();

            // Verifica se o fundo mudou para fazer a transição por Fade
            if (proximaFala.imagemFundo != null && proximaFala.imagemFundo != imagemFundo.sprite)
            {
                StartCoroutine(TrocarFundoEFalaComFade(proximaFala.imagemFundo));
            }
            else
            {
                ExibirProximaFala();
            }
        }
        else
        {
            StartCoroutine(FinalizarEIrParaCena());
        }
    }

    private void ExibirProximaFala()
    {
        if (filaFalas.Count == 0) return;

        falaAtual = filaFalas.Dequeue();
        indiceAtual++;

        if (textoNomePersonagem != null)
        {
            textoNomePersonagem.text = falaAtual.ehNarracao ? "" : falaAtual.nomeQuemFala;
        }

        personagemAtivoAtual = BuscarPersonagem(falaAtual.nomeQuemFala);
        imagemAtivaAtual = ObterSlotImagem(falaAtual.posicao);

        AtualizarDestaquePersonagens(imagemAtivaAtual, falaAtual.ehNarracao);

        if (coroutineEscrita != null) StopCoroutine(coroutineEscrita);
        coroutineEscrita = StartCoroutine(EfeitoDigitaTexto(falaAtual));
    }

    private IEnumerator EfeitoDigitaTexto(FalaCutscene linha)
    {
        estaEscrevendo = true;
        if (textoBalao != null) textoBalao.text = "";

        Vector2 posOrigemSlot = ObterPosicaoFixaSlot(linha.posicao);
        RectTransform rectPersonagem = imagemAtivaAtual != null ? imagemAtivaAtual.rectTransform : null;

        if (rectPersonagem != null) rectPersonagem.anchoredPosition = posOrigemSlot;

        foreach (char letra in linha.texto.ToCharArray())
        {
            if (textoBalao != null) textoBalao.text += letra;

            // Animação de fala, pulo e som ocorrem apenas se NÃO for narração
            if (!linha.ehNarracao && char.IsLetterOrDigit(letra))
            {
                if (rectPersonagem != null)
                {
                    rectPersonagem.anchoredPosition = posOrigemSlot + new Vector2(0, alturaPuloLetra);
                }

                if (imagemAtivaAtual != null && personagemAtivoAtual.spriteBocaAberta != null)
                {
                    imagemAtivaAtual.sprite = personagemAtivoAtual.spriteBocaAberta;
                }

                AudioClip clipParaTocar = linha.somFala != null ? linha.somFala : somFalaPadrao;
                if (audioSourceSFX != null && clipParaTocar != null)
                {
                    audioSourceSFX.PlayOneShot(clipParaTocar);
                }
            }

            yield return new WaitForSeconds(velocidadeEscrita);

            // Retorna à posição original e fecha a boca
            if (rectPersonagem != null) rectPersonagem.anchoredPosition = posOrigemSlot;
            RestaurarBocaFechada();
        }

        if (rectPersonagem != null) rectPersonagem.anchoredPosition = posOrigemSlot;
        RestaurarBocaFechada();
        estaEscrevendo = false;
    }

    private void CompletarTextoImediatamente()
    {
        if (coroutineEscrita != null) StopCoroutine(coroutineEscrita);
        if (textoBalao != null) textoBalao.text = falaAtual.texto;

        ResetarPosicoesDeTodosSlots();
        RestaurarBocaFechada();
        estaEscrevendo = false;
    }

    private PersonagemCutscene BuscarPersonagem(string nome)
    {
        foreach (var p in listaPersonagens)
        {
            if (p.nome.ToLower() == nome.ToLower()) return p;
        }
        return new PersonagemCutscene { nome = nome, corFoco = Color.white };
    }

    private Image ObterSlotImagem(PosicaoCutscene posicao)
    {
        switch (posicao)
        {
            case PosicaoCutscene.Esquerda: return imagemEsquerda;
            case PosicaoCutscene.Direita: return imagemDireita;
            case PosicaoCutscene.Centro: return imagemCentro;
            default: return imagemCentro;
        }
    }

    private Vector2 ObterPosicaoFixaSlot(PosicaoCutscene posicao)
    {
        switch (posicao)
        {
            case PosicaoCutscene.Esquerda: return posFixaEsquerda;
            case PosicaoCutscene.Direita: return posFixaDireita;
            case PosicaoCutscene.Centro: return posFixaCentro;
            default: return posFixaCentro;
        }
    }

    private void AtualizarDestaquePersonagens(Image slotAtivo, bool ehNarracao)
    {
        Image[] slots = { imagemEsquerda, imagemCentro, imagemDireita };

        foreach (var slot in slots)
        {
            if (slot == null) continue;

            if (slot.sprite != null) slot.gameObject.SetActive(true);

            if (slot == slotAtivo && !ehNarracao)
            {
                slot.color = personagemAtivoAtual.corFoco != Color.clear ? personagemAtivoAtual.corFoco : Color.white;
                if (personagemAtivoAtual.spriteBocaFechada != null)
                {
                    slot.sprite = personagemAtivoAtual.spriteBocaFechada;
                }
            }
            else
            {
                slot.color = corInativo;
            }
        }
    }

    private void RestaurarBocaFechada()
    {
        if (imagemAtivaAtual != null && personagemAtivoAtual.spriteBocaFechada != null)
        {
            imagemAtivaAtual.sprite = personagemAtivoAtual.spriteBocaFechada;
        }
    }

    private void ResetarPosicoesDeTodosSlots()
    {
        if (imagemEsquerda != null) imagemEsquerda.rectTransform.anchoredPosition = posFixaEsquerda;
        if (imagemCentro != null) imagemCentro.rectTransform.anchoredPosition = posFixaCentro;
        if (imagemDireita != null) imagemDireita.rectTransform.anchoredPosition = posFixaDireita;
    }

    private IEnumerator TrocarFundoEFalaComFade(Sprite proximaImagem)
    {
        estaEmTransicao = true;
        if (canvasGroupFade != null) canvasGroupFade.blocksRaycasts = true;

        float tempo = 0f;
        while (tempo < duracaoFade)
        {
            tempo += Time.deltaTime;
            if (canvasGroupFade != null) canvasGroupFade.alpha = Mathf.Clamp01(tempo / duracaoFade);
            yield return null;
        }

        if (canvasGroupFade != null) canvasGroupFade.alpha = 1f;
        if (imagemFundo != null) imagemFundo.sprite = proximaImagem;

        ExibirProximaFala();

        tempo = duracaoFade;
        while (tempo > 0f)
        {
            tempo -= Time.deltaTime;
            if (canvasGroupFade != null) canvasGroupFade.alpha = Mathf.Clamp01(tempo / duracaoFade);
            yield return null;
        }

        if (canvasGroupFade != null)
        {
            canvasGroupFade.alpha = 0f;
            canvasGroupFade.blocksRaycasts = false;
        }
        estaEmTransicao = false;
    }

    private IEnumerator FinalizarEIrParaCena()
    {
        estaEmTransicao = true;
        if (canvasGroupFade != null) canvasGroupFade.blocksRaycasts = true;

        float tempo = 0f;
        while (tempo < duracaoFade)
        {
            tempo += Time.deltaTime;
            if (canvasGroupFade != null) canvasGroupFade.alpha = Mathf.Clamp01(tempo / duracaoFade);
            yield return null;
        }

        SceneManager.LoadScene(nomeProximaCena);
    }
}