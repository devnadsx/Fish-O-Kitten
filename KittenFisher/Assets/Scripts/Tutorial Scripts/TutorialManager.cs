using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    
    [Header("Aviso de Controles (Canva)")]
    public GameObject imagemAvisoControles; // Arraste a imagem do aviso aqui no Inspector!
    private bool avisoJaFoiExibido = false;

    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("Estilo Visual Novel / RPG")]
    public Image imagemGatinho;          // Sprite do gatinho do lado do balão
    public float velocidadeEscrita = 0.04f; // Tempo entre cada letra (menor = mais rápido)
    public float alturaPuloLetra = 8f;   // Pulinho curto a cada letra
    public AudioSource audioSourceSFX;   // AudioSource para o som de fala
    public AudioClip somFalaGatinho;     // Som curto (bipe/miau bem curto) para cada letra

    private Vector3 posicaoOriginalGato;
    private Coroutine coroutineEscrita;
    private bool estaEscrevendo = false;
    private string textoCompletoAtual = "";

    [Header("UI da Lista de Tarefas")]
    public GameObject painelListaItens;
    public TextMeshProUGUI textoLista;

    [Header("Configuração de Cenas")]
    public string nomeCenaJogoPrincipal = "SampleScene";

    // Estados
    private int etapaFala = 0;
    private bool falaAtiva = true;

    // Itens
    private bool pegouLupa = false;
    private bool pegouAquario = false;
    private bool pegouTesoura = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (imagemGatinho != null)
        {
            posicaoOriginalGato = imagemGatinho.rectTransform.anchoredPosition;
        }

        if (painelListaItens != null) painelListaItens.SetActive(false);

        // Garante que o aviso esteja visível na primeira fala
        if (imagemAvisoControles != null)
        {
            imagemAvisoControles.SetActive(true);
        }

        MostrarFalaAtual();
    }

    void Update()
    {
        // Permite avançar a fala/diálogo pressionando ENTER no teclado ou Keypad
        if (falaAtiva && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            AvancarTexto();
        }
    }

    // Chamado pelo Botão / Tecla ENTER / Clique para avançar
    public void AvancarTexto()
    {
        // Esconde o aviso do Canva assim que o jogador avança a primeira vez!
        if (imagemAvisoControles != null && imagemAvisoControles.activeSelf)
        {
            imagemAvisoControles.SetActive(false);
        }

        // Se ainda está digitando a frase, o clique/ENTER completa a frase na hora
        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        etapaFala++;
        MostrarFalaAtual();
    }
    void MostrarFalaAtual()
    {
        falaAtiva = true;

        if (painelBalaoFala != null) painelBalaoFala.SetActive(true);
        if (imagemGatinho != null) imagemGatinho.gameObject.SetActive(true);
        if (botaoAvancarFala != null) botaoAvancarFala.gameObject.SetActive(true);

        switch (etapaFala)
        {
            case 0:
                IniciarDigitacao("Oh, I need to do a new experiment!");
                break;

            case 1:
                IniciarDigitacao("But.., to do that, I need to collect some materials from the lab...");
                break;

            case 2:
                IniciarDigitacao("Tsk.. I need to find the magnifying glass, the fishbowl and the scissors. Where on earth did I leave each of them…?");
                break;

            case 3:
                EsconderDialogoEGato();
                if (painelListaItens != null) painelListaItens.SetActive(true);
                AtualizarTextoLista();
                break;

            case 4:
                IniciarDigitacao("I've found the magnifying glass! Now I can have a closer look to see if I can find the rest...");
                break;

            case 5:
                IniciarDigitacao("💡 Tip: You can move the scene and zoom in on it for a better view!");
                break;

            case 6:
                EsconderDialogoEGato();
                break;

            case 7:
                IniciarDigitacao("Great, I’ve got everything! Now I’m ready for the experiment! Yay!");
                if (botaoAvancarFala != null)
                {
                    botaoAvancarFala.onClick.RemoveAllListeners();
                    botaoAvancarFala.onClick.AddListener(IrParaOJogo);
                }
                break;
        }
    }

    // --- LÓGICA DE DIGITAÇÃO E PULINHO POR LETRA ---

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

            // Se for uma letra (não espaço/pontuação), faz o gatinho pular e toca som
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

        // Sobe um pouquinho
        rect.anchoredPosition = posicaoOriginalGato + new Vector3(0, alturaPuloLetra, 0);
        yield return new WaitForSeconds(velocidadeEscrita * 0.5f);

        // Volta ao normal
        rect.anchoredPosition = posicaoOriginalGato;
    }

    void EsconderDialogoEGato()
    {
        falaAtiva = false;
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
        if (imagemGatinho != null) imagemGatinho.gameObject.SetActive(false);
    }

    public bool PodeColetarItem(string nomeDoItem)
    {
        if (falaAtiva) return false;
        if (!pegouLupa) return nomeDoItem == "Lupa";
        return true;
    }

    public void ColetarItem(string nome)
    {
        if (nome == "Lupa")
        {
            pegouLupa = true;
            AtualizarTextoLista();
            etapaFala = 4;
            MostrarFalaAtual();
        }
        else if (nome == "Aquario")
        {
            pegouAquario = true;
            AtualizarTextoLista();
        }
        else if (nome == "Tesoura")
        {
            pegouTesoura = true;
            AtualizarTextoLista();
        }

        if (pegouLupa && pegouAquario && pegouTesoura)
        {
            etapaFala = 7;
            MostrarFalaAtual();
        }
    }

    void AtualizarTextoLista()
    {
        if (textoLista == null) return;

        // Texto da checklist adaptado para Inglês no jogo
        textoLista.text = $"<b>Materials:</b>\n" +
            $"{(pegouLupa ? "<s>• Magnifying Glass</s>" : "• Magnifying Glass")}\n" +
            $"{(pegouAquario ? "<s>• Fishbowl</s>" : "• Fishbowl")}\n" +
            $"{(pegouTesoura ? "<s>• Scissors</s>" : "• Scissors")}";
    }

    public void PularTutorial()
    {
        IrParaOJogo();
    }

    public void IrParaOJogo()
    {
        SceneManager.LoadScene(nomeCenaJogoPrincipal);
    }
}