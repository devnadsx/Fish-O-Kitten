using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverDialogue : MonoBehaviour
{
    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("Sprites do Gatinho (Boca Aberta / Fechada)")]
    public Image imagemGatinho;
    public Sprite spriteBocaFechada;
    public Sprite spriteBocaAberta;

    [Header("Estilo Visual Novel / RPG")]
    public float velocidadeEscrita = 0.03f;
    public float alturaPuloLetra = 6f;
    public AudioSource audioSourceSFX;
    public AudioClip somFalaGatinho;

    [Header("✍️ FALAS DO GATINHO NO GAME OVER")]
    [TextArea(2, 5)]
    public List<string> falasGameOver = new List<string>();

    [Header("Efeitos de Perda de Consciência (Piscadas)")]
    public Image overlayEscuro;
    public int quantidadeFalasComPiscada = 2;
    public float duracaoPiscada = 0.4f;

    [Header("Texto Final de Game Over (Com Fade)")]
    [Tooltip("Arraste aqui o CanvasGroup do texto 'Game Over'")]
    public CanvasGroup textoGameOverCanvasGroup;

    [Header("Painel de Opções (Botões)")]
    public RectTransform painelBotoesGameOver;
    public float duracaoFadeFinal = 1.2f;
    public float duracaoAnimacaoBotoes = 0.6f;
    public float deslocamentoYSubida = 300f;

    [Header("Navegação de Cenas")]
    public string nomeCenaMenuPrincipal = "MenuPrincipal";

    private Vector3 posicaoOriginalGato;
    private Vector2 posicaoFinalPainelBotoes;
    private Coroutine coroutineEscrita;
    private Coroutine coroutinePiscada;
    private bool estaEscrevendo = false;
    private string textoCompletoAtual = "";
    private int indiceFalaAtual = 0;

    void Start()
    {
        if (imagemGatinho != null)
        {
            posicaoOriginalGato = imagemGatinho.rectTransform.anchoredPosition;
            DefinirSpriteNormal();
        }

        if (overlayEscuro != null)
        {
            SetOverlayAlpha(0f);
        }

        // Garante que o texto de Game Over comece completamente invisível
        if (textoGameOverCanvasGroup != null)
        {
            textoGameOverCanvasGroup.alpha = 0f;
        }

        // Prepara o painel de botões escondido lá embaixo
        if (painelBotoesGameOver != null)
        {
            posicaoFinalPainelBotoes = painelBotoesGameOver.anchoredPosition;
            painelBotoesGameOver.anchoredPosition = posicaoFinalPainelBotoes - new Vector2(0, deslocamentoYSubida);
            painelBotoesGameOver.gameObject.SetActive(false);
        }

        if (falasGameOver.Count > 0)
        {
            indiceFalaAtual = 0;
            MostrarFalaAtual();
        }
        else
        {
            StartCoroutine(SequenciaFadeEFim());
        }
    }

    public void AvancarTexto()
    {
        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        indiceFalaAtual++;

        if (indiceFalaAtual < falasGameOver.Count)
        {
            MostrarFalaAtual();
        }
        else
        {
            StartCoroutine(SequenciaFadeEFim());
        }
    }

    void MostrarFalaAtual()
    {
        if (painelBalaoFala != null) painelBalaoFala.SetActive(true);
        if (imagemGatinho != null) imagemGatinho.gameObject.SetActive(true);
        if (botaoAvancarFala != null) botaoAvancarFala.gameObject.SetActive(true);

        int falasRestantes = falasGameOver.Count - 1 - indiceFalaAtual;
        if (falasRestantes > 0 && falasRestantes <= quantidadeFalasComPiscada)
        {
            DispararPiscadaPreta();
        }

        IniciarDigitacao(falasGameOver[indiceFalaAtual]);
    }

    void DispararPiscadaPreta()
    {
        if (coroutinePiscada != null) StopCoroutine(coroutinePiscada);
        coroutinePiscada = StartCoroutine(EfeitoPiscadaPreta());
    }

    IEnumerator EfeitoPiscadaPreta()
    {
        if (overlayEscuro == null) yield break;

        float t = 0;
        while (t < duracaoPiscada * 0.5f)
        {
            t += Time.deltaTime;
            SetOverlayAlpha(Mathf.Lerp(0f, 0.85f, t / (duracaoPiscada * 0.5f)));
            yield return null;
        }

        t = 0;
        while (t < duracaoPiscada * 0.5f)
        {
            t += Time.deltaTime;
            SetOverlayAlpha(Mathf.Lerp(0.85f, 0.2f, t / (duracaoPiscada * 0.5f)));
            yield return null;
        }
    }

    void IniciarDigitacao(string texto)
    {
        textoCompletoAtual = texto;
        if (coroutineEscrita != null) StopCoroutine(coroutineEscrita);
        coroutineEscrita = StartCoroutine(EfeitoDigitarTexto(texto));
    }

    IEnumerator EfeitoDigitarTexto(string texto)
    {
        estaEscrevendo = true;
        textoBalao.text = "";

        foreach (char letra in texto.ToCharArray())
        {
            textoBalao.text += letra;

            if (char.IsLetterOrDigit(letra))
            {
                if (imagemGatinho != null && spriteBocaAberta != null)
                {
                    imagemGatinho.sprite = spriteBocaAberta;
                }

                if (imagemGatinho != null && imagemGatinho.gameObject.activeSelf)
                {
                    StartCoroutine(PulinhoRapidoGato());
                }

                if (audioSourceSFX != null && somFalaGatinho != null)
                {
                    audioSourceSFX.PlayOneShot(somFalaGatinho);
                }
            }
            else
            {
                DefinirSpriteNormal();
            }

            yield return new WaitForSeconds(velocidadeEscrita);
        }

        DefinirSpriteNormal();
        estaEscrevendo = false;
    }

    void CompletarTextoImediatamente()
    {
        if (coroutineEscrita != null) StopCoroutine(coroutineEscrita);
        textoBalao.text = textoCompletoAtual;

        DefinirSpriteNormal();
        estaEscrevendo = false;

        if (imagemGatinho != null)
        {
            imagemGatinho.rectTransform.anchoredPosition = posicaoOriginalGato;
        }
    }

    IEnumerator PulinhoRapidoGato()
    {
        RectTransform rect = imagemGatinho.rectTransform;
        rect.anchoredPosition = posicaoOriginalGato + new Vector3(0, alturaPuloLetra, 0);
        yield return new WaitForSeconds(velocidadeEscrita * 0.5f);
        rect.anchoredPosition = posicaoOriginalGato;
    }

    void DefinirSpriteNormal()
    {
        if (imagemGatinho != null && spriteBocaFechada != null)
        {
            imagemGatinho.sprite = spriteBocaFechada;
        }
    }

    // -----------------------------------------------------------
    // 🌑 FADE OUT COMPLETO, TEXTO E SURGIMENTO DOS BOTÕES
    // -----------------------------------------------------------

    IEnumerator SequenciaFadeEFim()
    {
        DefinirSpriteNormal();

        // 1. Esconde a UI do diálogo imediatamente
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
        if (imagemGatinho != null) imagemGatinho.gameObject.SetActive(false);
        if (botaoAvancarFala != null) botaoAvancarFala.gameObject.SetActive(false);

        // 2. Fade Out do fundo escuro E Fade In do Texto de Game Over ao mesmo tempo
        float tempo = 0f;
        float alphaInicialOverlay = (overlayEscuro != null) ? overlayEscuro.color.a : 0f;

        while (tempo < duracaoFadeFinal)
        {
            tempo += Time.deltaTime;
            float percentual = tempo / duracaoFadeFinal;

            // Escurece o fundo
            if (overlayEscuro != null)
            {
                SetOverlayAlpha(Mathf.Lerp(alphaInicialOverlay, 1f, percentual));
            }

            // Revela o texto suavemente
            if (textoGameOverCanvasGroup != null)
            {
                textoGameOverCanvasGroup.alpha = Mathf.Lerp(0f, 1f, percentual);
            }

            yield return null;
        }

        if (overlayEscuro != null) SetOverlayAlpha(1f);
        if (textoGameOverCanvasGroup != null) textoGameOverCanvasGroup.alpha = 1f;

        // 3. Exibe e sobe o painel com os botões
        if (painelBotoesGameOver != null)
        {
            painelBotoesGameOver.gameObject.SetActive(true);

            float tempoSubida = 0f;
            Vector2 posInicial = painelBotoesGameOver.anchoredPosition;

            while (tempoSubida < duracaoAnimacaoBotoes)
            {
                tempoSubida += Time.deltaTime;
                float pct = Mathf.Clamp01(tempoSubida / duracaoAnimacaoBotoes);
                float curva = Mathf.SmoothStep(0f, 1f, pct);

                painelBotoesGameOver.anchoredPosition = Vector2.Lerp(posInicial, posicaoFinalPainelBotoes, curva);
                yield return null;
            }

            painelBotoesGameOver.anchoredPosition = posicaoFinalPainelBotoes;
        }
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (overlayEscuro != null)
        {
            Color c = overlayEscuro.color;
            c.a = alpha;
            overlayEscuro.color = c;
        }
    }

    // -----------------------------------------------------------
    // 🕹️ MÉTODOS DOS BOTÕES
    // -----------------------------------------------------------

    public void TentarNovamente()
    {
        string ultimaFase = PlayerPrefs.GetString("UltimaFaseJogada", "Game3");
        SceneManager.LoadScene(ultimaFase);
    }

    public void IrParaMenuPrincipal()
    {
        SceneManager.LoadScene(nomeCenaMenuPrincipal);
    }
}