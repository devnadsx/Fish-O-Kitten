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

    [Header("Sprites do Gatinho (Boca Aberta / Fechada)")]
    public Image imagemGatinho;
    public Sprite spriteBocaFechada; // Sprite do gatinho quieto
    public Sprite spriteBocaAberta;  // Sprite do gatinho falando

    [Header("Estilo Visual Novel / RPG")]
    public float velocidadeEscrita = 0.03f;
    public float alturaPuloLetra = 6f;
    public AudioSource audioSourceSFX;
    public AudioClip somFalaGatinho;

    [Header("✍️ DIGITE AS FALAS AQUI NO INSPECTOR!")]
    [TextArea(2, 5)]
    public List<string> falasDoGato = new List<string>();

    [Header("Desbloquear Gameplay")]
    public MonoBehaviour[] scriptsParaAtivarAposDialogo;

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
            DefinirSpriteNormal();
        }

        if (falasDoGato.Count > 0)
        {
            DefinirEstadoGameplay(false);
            indiceFalaAtual = 0;
            MostrarFalaAtual();
        }
        else
        {
            FinalizarDialogo();
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
                // 😮 Troca para boca aberta quando for uma letra
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
                // 😐 Fecha a boca em espaços e pontuações
                DefinirSpriteNormal();
            }

            yield return new WaitForSeconds(velocidadeEscrita);
        }

        // 😐 Fecha a boca quando a frase terminar
        DefinirSpriteNormal();
        estaEscrevendo = false;
    }

    void CompletarTextoImediatamente()
    {
        if (coroutineEscrita != null) StopCoroutine(coroutineEscrita);
        textoBalao.text = textoCompletoAtual;

        DefinirSpriteNormal(); // Garante que fecha a boca
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

    void FinalizarDialogo()
    {
        DefinirSpriteNormal();
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
        if (imagemGatinho != null) imagemGatinho.gameObject.SetActive(false);

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
    // Adicione este método em qualquer lugar dentro da classe SceneIntroDialogue
    public void IniciarNovoDialogoExterno()
    {
        if (falasDoGato.Count > 0)
        {
            indiceFalaAtual = 0;
            DefinirEstadoGameplay(false); // Pausa ações enquanto fala
            MostrarFalaAtual();
        }
    }
}