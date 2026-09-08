using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class AnalyseUIManager : MonoBehaviour
{
    [Header("UI Elementos")]
    public GameObject painelBalaoFala;  // Objeto 'fala'
    public TextMeshProUGUI textoBalao; // Texto do balão
    public Image imagemGatoUI;         // GatinhoVN
    public RectTransform rectGatoUI;   // GatinhoVN

    [Header("Expressões do Gato")]
    public Sprite bocaFechada;
    public Sprite bocaAberta;

    [Header("Configurações do Pulo e Fala")]
    public float forcaPuloUI = 10f;
    public float velocidadeEscrita = 0.03f;
    public AudioSource audioSource;
    public AudioClip somFala;

    [Header("Próxima Cena")]
    public string nomeCenaEntrevista = "End";

    private Vector2 posOriginalGato;
    private Coroutine coroutineFala;
    private bool estaEscrevendo = false;
    private string textoAtual = "";

    void Start()
    {
        if (rectGatoUI != null)
            posOriginalGato = rectGatoUI.anchoredPosition;

        // FORÇA O BALÃO A APARECER COM A SUA FRASE NA HORA QUE DÁ PLAY
        MostrarFala("Time to test and write about them. Which one should i start with?.");
    }

    void Update()
    {
        // Clique ou Espaço avança / completa o texto
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (painelBalaoFala != null && painelBalaoFala.activeSelf)
            {
                AvancarOuPularTexto();
            }
        }
    }

    public void AvancarOuPularTexto()
    {
        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        if (painelBalaoFala != null && painelBalaoFala.activeSelf)
        {
            painelBalaoFala.SetActive(false);
        }
    }

    public void MostrarFala(string mensagem)
    {
        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        if (coroutineFala != null)
            StopCoroutine(coroutineFala);

        coroutineFala = StartCoroutine(EfeitoDigitar(mensagem));
    }

    IEnumerator EfeitoDigitar(string mensagem)
    {
        estaEscrevendo = true;
        textoAtual = mensagem;

        if (textoBalao != null)
            textoBalao.text = "";

        foreach (char letra in mensagem.ToCharArray())
        {
            if (textoBalao != null)
                textoBalao.text += letra;

            if (char.IsLetterOrDigit(letra))
            {
                if (rectGatoUI != null)
                    rectGatoUI.anchoredPosition = posOriginalGato + new Vector2(0, forcaPuloUI);

                if (imagemGatoUI != null && bocaAberta != null)
                    imagemGatoUI.sprite = bocaAberta;

                if (audioSource != null && somFala != null)
                    audioSource.PlayOneShot(somFala);
            }

            yield return new WaitForSeconds(velocidadeEscrita);
            ResetaExpressao();
        }

        ResetaExpressao();
        estaEscrevendo = false;
    }

    void CompletarTextoImediatamente()
    {
        if (coroutineFala != null)
            StopCoroutine(coroutineFala);

        if (textoBalao != null)
            textoBalao.text = textoAtual;

        ResetaExpressao();
        estaEscrevendo = false;
    }

    void ResetaExpressao()
    {
        if (rectGatoUI != null)
            rectGatoUI.anchoredPosition = posOriginalGato;

        if (imagemGatoUI != null && bocaFechada != null)
            imagemGatoUI.sprite = bocaFechada;
    }

    public void FinalizarAnalyseETrocarCena()
    {
        if (!string.IsNullOrEmpty(nomeCenaEntrevista))
        {
            SceneManager.LoadScene(nomeCenaEntrevista);
        }
    }
}