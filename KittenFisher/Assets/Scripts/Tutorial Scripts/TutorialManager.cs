using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;



public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Aviso de Controles (Canva)")]
    public GameObject imagemAvisoControles;

    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("Sprites do Gatinho (Visual Novel)")]
    public Image imagemGatinho;           // Componente Image na tela
    public Sprite spriteBocaFechada;      // Sprite padrão (Rosto normal)
    public Sprite spriteBocaAberta;       // Sprite falando (Boca aberta)

    [Header("Expressões Futuras (Expansível)")]
    public List<ExpressaoGatinho> expressoesExtras = new List<ExpressaoGatinho>();

    [Header("Estilo Visual Novel / RPG")]
    public float velocidadeEscrita = 0.04f;
    public float alturaPuloLetra = 8f;
    public AudioSource audioSourceSFX;
    public AudioClip somFalaGatinho;

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
            DefinirSpriteNormal();
        }

        if (painelListaItens != null) painelListaItens.SetActive(false);

        if (imagemAvisoControles != null)
        {
            imagemAvisoControles.SetActive(true);
        }

        MostrarFalaAtual();
    }

    void Update()
    {
        if (falaAtiva && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            AvancarTexto();
        }
    }

    public void AvancarTexto()
    {
        if (imagemAvisoControles != null && imagemAvisoControles.activeSelf)
        {
            imagemAvisoControles.SetActive(false);
        }

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

    // --- DIGITAÇÃO COM ANIMAÇÃO DE BOCA ABERTA/FECHADA ---

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
                // 😮 Troca o sprite para a boca aberta enquanto digita a letra!
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
                // 😐 Em pontuações e espaços, ele fecha a boca um segundo
                DefinirSpriteNormal();
            }

            yield return new WaitForSeconds(velocidadeEscrita);
        }

        // 😐 A fala acabou, volta para o sprite de boca fechada normal
        DefinirSpriteNormal();
        estaEscrevendo = false;
    }

    void CompletarTextoImediatamente()
    {
        if (coroutineEscrita != null) StopCoroutine(coroutineEscrita);
        textoBalao.text = textoCompletoAtual;

        // Garante que ao terminar a boca fecha e volta a posição
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

    // --- SISTEMA EXPANSÍVEL DE EXPRESSÕES FUTURAS ---
    // Você pode chamar isso no futuro assim: MudarExpressao("Triste");
    public void MudarExpressao(string nomeExpressao)
    {
        foreach (var item in expressoesExtras)
        {
            if (item.nomeExpressao.ToLower() == nomeExpressao.ToLower())
            {
                if (imagemGatinho != null) imagemGatinho.sprite = item.sprite;
                return;
            }
        }
    }

    void EsconderDialogoEGato()
    {
        falaAtiva = false;
        DefinirSpriteNormal();
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