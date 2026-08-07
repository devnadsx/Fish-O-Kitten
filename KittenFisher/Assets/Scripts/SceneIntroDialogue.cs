using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneIntroDialogue : MonoBehaviour
{
    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("Estilo Visual Novel / RPG")]
    public Image imagemGatinho;
    public float velocidadeEscrita = 0.03f;
    public float alturaPuloLetra = 6f;
    public AudioSource audioSourceSFX;
    public AudioClip somFalaGatinho;

    [Header("✍️ DIGITE AS FALAS AQUI NO INSPECTOR!")]
    [TextArea(2, 5)]
    public List<string> falasDoGato = new List<string>(); // Adicione quantas falas quiser pelo Inspector!

    [Header("Desbloquear Gameplay")]
    public MonoBehaviour[] scriptsParaAtivarAposDialogo; // Scripts de gameplay para ativar só depois que o diálogo terminar

    private Vector3 posicaoOriginalGato;
    private Coroutine coroutineEscrita;
    private bool estaEscrevendo = false;
    private string textoCompletoAtual = "";
    private int indiceFalaAtual = 0;

    void Start()
    {
        if (imagemGatinho != null)
        {
            posicaoOriginalGato = imagemGatinho.rectTransform.anchoredPosition;
        }

        // Se tiver falas configuradas, inicia o diálogo
        if (falasDoGato.Count > 0)
        {
            // Opcional: Desativa scripts de controle durante o diálogo
            DefinirEstadoGameplay(false);

            indiceFalaAtual = 0;
            MostrarFalaAtual();
        }
        else
        {
            // Se não tiver nenhuma fala, fecha tudo e libera o jogo direto
            FinalizarDialogo();
        }
    }

    public void AvancarTexto()
    {
        // Se ainda está digitando a frase, o clique completa a palavra na hora!
        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        indiceFalaAtual++;

        if (indiceFalaAtual < falasDoGato.Count)
        {
            MostrarFalaAtual();
        }
        else
        {
            FinalizarDialogo();
        }
    }

    void MostrarFalaAtual()
    {
        if (painelBalaoFala != null) painelBalaoFala.SetActive(true);
        if (imagemGatinho != null) imagemGatinho.gameObject.SetActive(true);
        if (botaoAvancarFala != null) botaoAvancarFala.gameObject.SetActive(true);

        IniciarDigitacao(falasDoGato[indiceFalaAtual]);
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
                if (imagemGatinho != null && imagemGatinho.gameObject.activeSelf)
                {
                    StartCoroutine(PulinhoRapidoGato());
                }

                if (audioSourceSFX != null && somFalaGatinho != null)
                {
                    audioSourceSFX.PlayOneShot(somFalaGatinho);
                }
            }

            yield return new WaitForSeconds(velocidadeEscrita);
        }

        estaEscrevendo = false;
    }

    void CompletarTextoImediatamente()
    {
        if (coroutineEscrita != null) StopCoroutine(coroutineEscrita);
        textoBalao.text = textoCompletoAtual;
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

    void FinalizarDialogo()
    {
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
        if (imagemGatinho != null) imagemGatinho.gameObject.SetActive(false);

        // Libera os scripts do jogo para o jogador começar a pescar!
        DefinirEstadoGameplay(true);
    }

    void DefinirEstadoGameplay(bool estado)
    {
        if (scriptsParaAtivarAposDialogo != null)
        {
            foreach (MonoBehaviour script in scriptsParaAtivarAposDialogo)
            {
                if (script != null) script.enabled = estado;
            }
        }
    }
}