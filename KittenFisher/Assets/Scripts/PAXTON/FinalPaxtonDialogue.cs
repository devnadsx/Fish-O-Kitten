using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class PersonagemFinalPaxton
{
    public string nome;

    [Header("Sprites")]
    public Image imagem;
    public Sprite spriteBocaFechada;
    public Sprite spriteBocaAberta;

    [Header("Som de fala")]
    public AudioClip somFala;
}

[Serializable]
public class FalaFinalPaxton
{
    public string nomeQuemFala;

    [TextArea(2, 5)]
    public string texto;
}

public class FinalPaxtonDialogue : MonoBehaviour
{
    [Header("UI do diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("Personagens")]
    public List<PersonagemFinalPaxton> personagens =
        new List<PersonagemFinalPaxton>();

    [Header("Falas")]
    public List<FalaFinalPaxton> falas =
        new List<FalaFinalPaxton>();

    [Header("Efeitos de fala")]
    public float velocidadeEscrita = 0.03f;
    public float alturaPuloLetra = 6f;
    public AudioSource audioSourceSFX;

    [Header("Efeito de escurecimento")]
    public Image overlayEscuro;
    public float duracaoFadeFinal = 1.2f;

    [Header("Próxima cena")]
    public string nomeCenaEntrevista = "Fase2";
    public float esperaAntesDeTrocarCena = 1f;

    private int indiceFalaAtual;
    private bool estaEscrevendo;
    private bool finalizacaoIniciada;

    private string textoCompletoAtual = "";
    private Coroutine coroutineEscrita;

    private PersonagemFinalPaxton personagemAtual;
    private Vector2 posicaoOriginalPersonagem;

    void Start()
    {
        if (overlayEscuro != null)
            DefinirAlphaOverlay(0f);

        foreach (PersonagemFinalPaxton personagem in personagens)
        {
            if (personagem != null && personagem.imagem != null)
                personagem.imagem.gameObject.SetActive(false);
        }

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        if (botaoAvancarFala != null)
            botaoAvancarFala.gameObject.SetActive(true);

        indiceFalaAtual = 0;

        if (falas.Count > 0)
            MostrarFalaAtual();
        else
            StartCoroutine(FinalizarSequencia());
    }

    public void AvancarTexto()
    {
        if (finalizacaoIniciada)
            return;

        if (estaEscrevendo)
        {
            CompletarTexto();
            return;
        }

        indiceFalaAtual++;

        if (indiceFalaAtual < falas.Count)
            MostrarFalaAtual();
        else
            StartCoroutine(FinalizarSequencia());
    }

    void MostrarFalaAtual()
    {
        if (indiceFalaAtual < 0 || indiceFalaAtual >= falas.Count)
            return;

        FalaFinalPaxton fala = falas[indiceFalaAtual];

        personagemAtual = personagens.Find(
            p => p != null &&
            !string.IsNullOrWhiteSpace(p.nome) &&
            string.Equals(
                p.nome.Trim(),
                fala.nomeQuemFala.Trim(),
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (personagemAtual == null)
        {
            if (textoNomePersonagem != null)
                textoNomePersonagem.text = fala.nomeQuemFala;

            if (textoBalao != null)
                textoBalao.text = fala.texto;

            estaEscrevendo = false;
            return;
        }

        foreach (PersonagemFinalPaxton personagem in personajesSeguros())
        {
            if (personagem.imagem != null)
                personagem.imagem.gameObject.SetActive(false);
        }

        if (personagemAtual.imagem != null)
        {
            personagemAtual.imagem.gameObject.SetActive(true);

            posicaoOriginalPersonagem =
                personagemAtual.imagem.rectTransform.anchoredPosition;

            DefinirSpriteNormal();
        }

        if (textoNomePersonagem != null)
            textoNomePersonagem.text = personagemAtual.nome;

        IniciarDigitacao(fala.texto);
    }

    IEnumerable<PersonagemFinalPaxton> personajesSeguros()
    {
        foreach (PersonagemFinalPaxton personagem in personagens)
        {
            if (personagem != null)
                yield return personagem;
        }
    }

    void IniciarDigitacao(string texto)
    {
        textoCompletoAtual = texto ?? "";

        if (coroutineEscrita != null)
            StopCoroutine(coroutineEscrita);

        coroutineEscrita = StartCoroutine(DigitarTexto(textoCompletoAtual));
    }

    IEnumerator DigitarTexto(string texto)
    {
        estaEscrevendo = true;

        if (textoBalao != null)
            textoBalao.text = "";

        foreach (char letra in texto)
        {
            if (textoBalao != null)
                textoBalao.text += letra;

            if (char.IsLetterOrDigit(letra))
            {
                if (personagemAtual != null)
                {
                    if (personagemAtual.imagem != null &&
                        personagemAtual.spriteBocaAberta != null)
                    {
                        personagemAtual.imagem.sprite =
                            personagemAtual.spriteBocaAberta;

                        StartCoroutine(PularLetra());
                    }

                    if (audioSourceSFX != null &&
                        personagemAtual.somFala != null)
                    {
                        audioSourceSFX.PlayOneShot(personagemAtual.somFala);
                    }
                }
            }
            else
            {
                DefinirSpriteNormal();
            }

            yield return new WaitForSeconds(velocidadeEscrita);
        }

        DefinirSpriteNormal();
        RestaurarPosicaoPersonagem();

        estaEscrevendo = false;
        coroutineEscrita = null;
    }

    void CompletarTexto()
    {
        if (coroutineEscrita != null)
            StopCoroutine(coroutineEscrita);

        if (textoBalao != null)
            textoBalao.text = textoCompletoAtual;

        DefinirSpriteNormal();
        RestaurarPosicaoPersonagem();

        estaEscrevendo = false;
        coroutineEscrita = null;
    }

    IEnumerator PularLetra()
    {
        if (personagemAtual == null ||
            personagemAtual.imagem == null)
            yield break;

        RectTransform rect = personagemAtual.imagem.rectTransform;
        Vector2 posicaoOriginal = posicaoOriginalPersonagem;

        rect.anchoredPosition =
            posicaoOriginal + new Vector2(0f, alturaPuloLetra);

        yield return new WaitForSeconds(velocidadeEscrita * 0.5f);

        if (rect != null)
            rect.anchoredPosition = posicaoOriginal;
    }

    void RestaurarPosicaoPersonagem()
    {
        if (personagemAtual != null &&
            personagemAtual.imagem != null)
        {
            personagemAtual.imagem.rectTransform.anchoredPosition =
                posicaoOriginalPersonagem;
        }
    }

    void DefinirSpriteNormal()
    {
        if (personagemAtual != null &&
            personagemAtual.imagem != null &&
            personagemAtual.spriteBocaFechada != null)
        {
            personagemAtual.imagem.sprite =
                personagemAtual.spriteBocaFechada;
        }
    }

    IEnumerator FinalizarSequencia()
    {
        if (finalizacaoIniciada)
            yield break;

        finalizacaoIniciada = true;
        estaEscrevendo = false;

        if (coroutineEscrita != null)
        {
            StopCoroutine(coroutineEscrita);
            coroutineEscrita = null;
        }

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(false);

        foreach (PersonagemFinalPaxton personagem in personajesSeguros())
        {
            if (personagem.imagem != null)
                personagem.imagem.gameObject.SetActive(false);
        }

        if (botaoAvancarFala != null)
            botaoAvancarFala.gameObject.SetActive(false);

        float duracao = Mathf.Max(0.01f, duracaoFadeFinal);
        float tempo = 0f;

        float alphaInicial =
            overlayEscuro != null ? overlayEscuro.color.a : 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracao);

            if (overlayEscuro != null)
            {
                DefinirAlphaOverlay(
                    Mathf.Lerp(alphaInicial, 1f, progresso)
                );
            }

            yield return null;
        }

        if (overlayEscuro != null)
            DefinirAlphaOverlay(1f);

        yield return new WaitForSeconds(esperaAntesDeTrocarCena);

        if (!string.IsNullOrWhiteSpace(nomeCenaEntrevista))
            SceneManager.LoadScene(nomeCenaEntrevista);
    }

    void DefinirAlphaOverlay(float alpha)
    {
        if (overlayEscuro == null)
            return;

        Color cor = overlayEscuro.color;
        cor.a = alpha;
        overlayEscuro.color = cor;
    }
}
