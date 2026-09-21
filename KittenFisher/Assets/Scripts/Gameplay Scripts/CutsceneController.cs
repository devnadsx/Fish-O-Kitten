using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public struct Personagem
{
    public string nome;
    public Sprite bocaFechada;
    public Sprite bocaAberta;
}

[System.Serializable]
public struct FalaLore
{
    public string nomeQuemFala;
    [TextArea(2, 5)]
    public string texto;
    public Sprite imagemFundo;
    public AudioClip somFala;
}

public class CutsceneController : MonoBehaviour
{
    [Header("Componentes de UI")]
    public Image imagemFundo;
    public Image imagemPersonagemUI;            // Slot da UI onde o personagem aparece
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoBalao;
    public CanvasGroup canvasGroupFade;

    [Header("Áudio")]
    public AudioSource audioSourceSFX;

    [Header("Configurações")]
    public float duracaoFade = 0.5f;
    public float velocidadeEscrita = 0.05f;
    public string nomeProximaCena;

    [Header("Banco de Dados de Personagens")]
    public List<Personagem> listaPersonagens = new List<Personagem>();

    [Header("Lista de Falas da Lore")]
    public List<FalaLore> falas = new List<FalaLore>();

    private int indiceAtual = 0;
    private bool estaEmTransicao = false;
    private Coroutine coroutineEscrita;

    void Start()
    {
        if (canvasGroupFade != null)
        {
            canvasGroupFade.alpha = 0f;
            canvasGroupFade.blocksRaycasts = false;
        }

        if (falas != null && falas.Count > 0)
        {
            if (falas[0].imagemFundo != null && imagemFundo != null)
            {
                imagemFundo.sprite = falas[0].imagemFundo;
            }
            ExibirFalaAtual();
        }
    }

    public void Avançar()
    {
        if (estaEmTransicao) return;

        // Se o texto ainda estiver digitando, completa instantaneamente e fecha a boca
        if (coroutineEscrita != null)
        {
            StopCoroutine(coroutineEscrita);
            coroutineEscrita = null;
            if (textoBalao != null) textoBalao.text = falas[indiceAtual].texto;

            // Garante que a boca fecha ao interromper
            Personagem p = BuscarPersonagem(falas[indiceAtual].nomeQuemFala);
            if (imagemPersonagemUI != null && p.bocaFechada != null)
            {
                imagemPersonagemUI.sprite = p.bocaFechada;
            }
            return;
        }

        indiceAtual++;

        if (indiceAtual < falas.Count)
        {
            Sprite fundoAnterior = falas[indiceAtual - 1].imagemFundo;
            Sprite fundoNovo = falas[indiceAtual].imagemFundo;

            if (fundoNovo != null && fundoNovo != fundoAnterior)
            {
                StartCoroutine(TrocarFundoEFalaComFade(fundoNovo));
            }
            else
            {
                ExibirFalaAtual();
            }
        }
        else
        {
            StartCoroutine(FinalizarEIrParaCena());
        }
    }

    private void ExibirFalaAtual()
    {
        FalaLore fala = falas[indiceAtual];

        if (textoNomePersonagem != null)
        {
            textoNomePersonagem.text = fala.nomeQuemFala;
        }

        Personagem personagemAtual = BuscarPersonagem(fala.nomeQuemFala);

        // Se encontrou o personagem, exibe a imagem dele
        if (imagemPersonagemUI != null)
        {
            if (personagemAtual.bocaFechada != null)
            {
                imagemPersonagemUI.enabled = true;
                imagemPersonagemUI.sprite = personagemAtual.bocaFechada;
            }
            else
            {
                // Esconde a imagem se for o Narrador ou não tiver sprite
                imagemPersonagemUI.enabled = false;
            }
        }

        if (coroutineEscrita != null) StopCoroutine(coroutineEscrita);
        coroutineEscrita = StartCoroutine(EfeitoDigitaTexto(fala.texto, fala.somFala, personagemAtual));
    }

    private IEnumerator EfeitoDigitaTexto(string textoCompleto, AudioClip somDaFala, Personagem personagem)
    {
        if (textoBalao != null)
        {
            textoBalao.text = "";
            bool bocaEstaAberta = false;

            foreach (char letra in textoCompleto.ToCharArray())
            {
                textoBalao.text += letra;

                // Toca o som de digitação
                if (letra != ' ' && somDaFala != null && audioSourceSFX != null)
                {
                    audioSourceSFX.PlayOneShot(somDaFala);
                }

                // Alterna a boca a cada espaço (por palavra) ou por caractere
                if (imagemPersonagemUI != null && imagemPersonagemUI.enabled)
                {
                    if (letra == ' ')
                    {
                        // Reseta para boca fechada nos espaços entre palavras
                        imagemPersonagemUI.sprite = personagem.bocaFechada;
                        bocaEstaAberta = false;
                    }
                    else if (personagem.bocaAberta != null && personagem.bocaFechada != null)
                    {
                        // Alterna a boca conforme digita
                        bocaEstaAberta = !bocaEstaAberta;
                        imagemPersonagemUI.sprite = bocaEstaAberta ? personagem.bocaAberta : personagem.bocaFechada;
                    }
                }

                yield return new WaitForSeconds(velocidadeEscrita);
            }

            // Garante que a boca fecha quando o texto termina de ser digitado
            if (imagemPersonagemUI != null && personagem.bocaFechada != null)
            {
                imagemPersonagemUI.sprite = personagem.bocaFechada;
            }
        }
        coroutineEscrita = null;
    }

    private Personagem BuscarPersonagem(string nome)
    {
        foreach (Personagem p in listaPersonagens)
        {
            if (p.nome.ToLower() == nome.ToLower())
            {
                return p;
            }
        }
        return default(Personagem);
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

        ExibirFalaAtual();

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