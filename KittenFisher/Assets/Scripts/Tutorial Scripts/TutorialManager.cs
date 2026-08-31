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

    [Header("Gatinho Normal (UI)")]
    public GameObject objetoGatinhoNormal; // GameObject do Gatinho Padrão no Canvas
    public Image imagemGatinhoNormal;      // Componente Image do Gatinho Padrão
    public Sprite spriteNormalFechada;
    public Sprite spriteNormalAberta;

    [Header("Gatinho Final / Caixa (UI)")]
    public GameObject objetoGatinhoFinal;  // GameObject do Gatinho Especial no Canvas
    public Image imagemGatinhoFinal;       // Componente Image do Gatinho Especial
    public Sprite spriteFinalFechada;
    public Sprite spriteFinalAberta;

    [Header("Expressões Futuras (Expansível)")]
    public List<ExpressaoGatinho> expressoesExtras = new List<ExpressaoGatinho>();

    [Header("Estilo Visual Novel / RPG")]
    public float velocidadeEscrita = 0.04f;
    public float alturaPuloLetra = 8f;
    public AudioSource audioSourceSFX;
    public AudioClip somFalaGatinho;

    [Header("UI da Lista de Tarefas")]
    public GameObject painelListaItens;
    public TextMeshProUGUI textoLista;

    [Header("Desbloquear Gameplay Após a Lupa")]
    public MonoBehaviour[] scriptsParaAtivarAposLupa;

    [Header("Configuração de Cenas")]
    public string nomeCenaJogoPrincipal = "SampleScene";

    // Estados
    private int etapaFala = 0;
    private bool falaAtiva = true;

    // Itens
    private bool pegouLupa = false;
    private bool pegouAquario = false;
    private bool pegouTesoura = false;

    // Posições originais para animação de pulo
    private Vector3 posicaoOriginalNormal;
    private Vector3 posicaoOriginalFinal;
    private Coroutine coroutineEscrita;
    private bool estaEscrevendo = false;
    private string textoCompletoAtual = "";

    // Controle de qual gatinho está ativo no momento
    private bool usarGatinhoFinal = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (imagemGatinhoNormal != null)
        {
            posicaoOriginalNormal = imagemGatinhoNormal.rectTransform.anchoredPosition;
        }

        if (imagemGatinhoFinal != null)
        {
            posicaoOriginalFinal = imagemGatinhoFinal.rectTransform.anchoredPosition;
            if (objetoGatinhoFinal != null) objetoGatinhoFinal.SetActive(false); // Mantém o final oculto no início
        }

        if (painelListaItens != null) painelListaItens.SetActive(false);

        if (imagemAvisoControles != null)
        {
            imagemAvisoControles.SetActive(true);
        }

        DefinirSpriteNormal();
        DefinirEstadoScriptsPosLupa(false);
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
        ExibirGatinhoAtual(true);

        if (botaoAvancarFala != null) botaoAvancarFala.gameObject.SetActive(true);

        switch (etapaFala)
        {
            case 0:
                IniciarDigitacao("Oh, I need to do a new experiment!");
                break;

            case 1:
                IniciarDigitacao("I heard about a mysterious catnip in the ocean.");
                break;

            case 2:
                IniciarDigitacao("Okay. I need to collect some materials from the lab...");
                break;

            case 3:
                IniciarDigitacao("Uhh.. I need to find the magnifying glass, the fishbowl and the scissors. Where on earth did I leave each of them…?");
                break;

            case 4:
                EsconderDialogoEGato();
                if (painelListaItens != null) painelListaItens.SetActive(true);
                AtualizarTextoLista();
                break;

            case 5:
                IniciarDigitacao("I've found the magnifying glass! Now I can have a closer look to see if I can find the rest...");
                break;

            case 6:
                IniciarDigitacao("Tip: You can move the scene and zoom in on it for a better view!");
                break;

            case 7:
                EsconderDialogoEGato();
                break;

            case 8:
                // Alterna a flag para indicar que agora usaremos o objeto do Gatinho Final
                usarGatinhoFinal = true;
                ExibirGatinhoAtual(true);
                DefinirSpriteNormal();

                IniciarDigitacao("Great, I’ve got everything! Now I’m ready for the experiment! Yay!");

                if (botaoAvancarFala != null)
                {
                    botaoAvancarFala.onClick.RemoveAllListeners();
                    botaoAvancarFala.onClick.AddListener(IrParaOJogo);
                }
                break;
        }
    }

    // --- SISTEMA DE DIGITAÇÃO E ANIMAÇÃO ---

    void IniciarDigitacao(string texto)
    {
        textoCompletoAtual = texto;
        if (coroutineEscrita != null) StopCoroutine(coroutineEscrita);
        coroutineEscrita = StartCoroutine(EfeitoDigitarTexto(texto));
    }

    IEnumerator EfeitoDigitarTexto(string texto)
    {
        estaEscrevendo = true;
        if (textoBalao != null) textoBalao.text = "";

        foreach (char letra in texto.ToCharArray())
        {
            if (textoBalao != null) textoBalao.text += letra;

            if (char.IsLetterOrDigit(letra))
            {
                DefinirSpriteAberta();

                StartCoroutine(PulinhoRapidoGato());

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
        if (textoBalao != null) textoBalao.text = textoCompletoAtual;

        DefinirSpriteNormal();
        estaEscrevendo = false;
        ResetarPosicaoGato();
    }

    IEnumerator PulinhoRapidoGato()
    {
        Image imgAtual = usarGatinhoFinal ? imagemGatinhoFinal : imagemGatinhoNormal;
        Vector3 posOriginal = usarGatinhoFinal ? posicaoOriginalFinal : posicaoOriginalNormal;

        if (imgAtual == null || !imgAtual.gameObject.activeInHierarchy) yield break;

        RectTransform rect = imgAtual.rectTransform;
        rect.anchoredPosition = posOriginal + new Vector3(0, alturaPuloLetra, 0);
        yield return new WaitForSeconds(velocidadeEscrita * 0.5f);
        rect.anchoredPosition = posOriginal;
    }

    void ResetarPosicaoGato()
    {
        if (imagemGatinhoNormal != null)
            imagemGatinhoNormal.rectTransform.anchoredPosition = posicaoOriginalNormal;

        if (imagemGatinhoFinal != null)
            imagemGatinhoFinal.rectTransform.anchoredPosition = posicaoOriginalFinal;
    }

    // --- GERENCIAMENTO DE SPRITES E VISIBILIDADE ---

    void DefinirSpriteNormal()
    {
        if (!usarGatinhoFinal && imagemGatinhoNormal != null && spriteNormalFechada != null)
        {
            imagemGatinhoNormal.sprite = spriteNormalFechada;
        }
        else if (usarGatinhoFinal && imagemGatinhoFinal != null && spriteFinalFechada != null)
        {
            imagemGatinhoFinal.sprite = spriteFinalFechada;
        }
    }

    void DefinirSpriteAberta()
    {
        if (!usarGatinhoFinal && imagemGatinhoNormal != null && spriteNormalAberta != null)
        {
            imagemGatinhoNormal.sprite = spriteNormalAberta;
        }
        else if (usarGatinhoFinal && imagemGatinhoFinal != null && spriteFinalAberta != null)
        {
            imagemGatinhoFinal.sprite = spriteFinalAberta;
        }
    }

    void ExibirGatinhoAtual(bool visivel)
    {
        if (!visivel)
        {
            if (objetoGatinhoNormal != null) objetoGatinhoNormal.SetActive(false);
            if (objetoGatinhoFinal != null) objetoGatinhoFinal.SetActive(false);
            return;
        }

        if (usarGatinhoFinal)
        {
            if (objetoGatinhoNormal != null) objetoGatinhoNormal.SetActive(false);
            if (objetoGatinhoFinal != null) objetoGatinhoFinal.SetActive(true);
        }
        else
        {
            if (objetoGatinhoNormal != null) objetoGatinhoNormal.SetActive(true);
            if (objetoGatinhoFinal != null) objetoGatinhoFinal.SetActive(false);
        }
    }

    void EsconderDialogoEGato()
    {
        falaAtiva = false;
        DefinirSpriteNormal();
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
        ExibirGatinhoAtual(false);
    }

    // --- GAMEPLAY E COLETA DE ITENS ---

    public bool PodeColetarItem(string nomeDoItem)
    {
        return !falaAtiva;
    }

    public void ColetarItem(string nome)
    {
        if (nome == "Lupa")
        {
            pegouLupa = true;
            AtualizarTextoLista();
            DefinirEstadoScriptsPosLupa(true);

            etapaFala = 5;
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
            etapaFala = 8;
            MostrarFalaAtual();
        }
    }

    void DefinirEstadoScriptsPosLupa(bool estado)
    {
        if (scriptsParaAtivarAposLupa != null)
        {
            foreach (MonoBehaviour script in scriptsParaAtivarAposLupa)
            {
                if (script != null) script.enabled = estado;
            }
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