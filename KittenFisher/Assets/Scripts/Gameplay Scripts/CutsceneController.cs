using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Estrutura do Personagem (Nome + Sprite)
[System.Serializable]
public struct Personagem
{
    public string nome;
    public Sprite spritePersonagem; // Imagem/Retrato do personagem se quiser usar
}

// Estrutura de cada Fala da Lore
[System.Serializable]
public struct FalaLore
{
    public string nomeQuemFala;     // Nome do personagem que está falando
    [TextArea(2, 5)]
    public string texto;            // A fala
    public Sprite imagemFundo;       // Fundo associado
    public AudioClip somFala;        // Som de digitação/efeito
}

public class CutsceneController : MonoBehaviour
{
    [Header("Componentes de UI")]
    public Image imagemFundo;
    public TextMeshProUGUI textoNomePersonagem; // Campo UI para o nome
    public TextMeshProUGUI textoBalao;          // Campo UI para o texto
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

        // Se o texto ainda estiver digitando, completa instantaneamente
        if (coroutineEscrita != null)
        {
            StopCoroutine(coroutineEscrita);
            coroutineEscrita = null;
            if (textoBalao != null) textoBalao.text = falas[indiceAtual].texto;
            return;
        }

        indiceAtual++;

        if (indiceAtual < falas.Count)
        {
            Sprite fundoAnterior = falas[indiceAtual - 1].imagemFundo;
            Sprite fundoNovo = falas[indiceAtual].imagemFundo;

            // Se o fundo mudou, faz a transição com Fade
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

        // Atualiza o nome do personagem na UI
        if (textoNomePersonagem != null)
        {
            textoNomePersonagem.text = fala.nomeQuemFala;
        }

        // Inicia a digitação e passa o som correspondente para a corrotina
        if (coroutineEscrita != null) StopCoroutine(coroutineEscrita);
        coroutineEscrita = StartCoroutine(EfeitoDigitaTexto(fala.texto, fala.somFala));
    }

    private IEnumerator EfeitoDigitaTexto(string textoCompleto, AudioClip somDaFala)
    {
        if (textoBalao != null)
        {
            textoBalao.text = "";
            foreach (char letra in textoCompleto.ToCharArray())
            {
                textoBalao.text += letra;

                // Toca o som a cada letra (ignorando espaços para não ficar estranho)
                if (letra != ' ' && somDaFala != null && audioSourceSFX != null)
                {
                    audioSourceSFX.PlayOneShot(somDaFala);
                }

                yield return new WaitForSeconds(velocidadeEscrita);
            }
        }
        coroutineEscrita = null;
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