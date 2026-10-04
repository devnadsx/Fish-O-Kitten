using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("Escolha do Paxton")]
    public GameObject painelBotoesEscolha;
    public Button botaoAceitarPaxton;
    public Button botaoRecusarPaxton;

    [Header("Gatinho")]
    public GameObject objetoGatinhoUI;
    public Image imagemGatinhoUI;
    public Sprite spriteNormalFechada;
    public Sprite spriteNormalAberta;

    [Header("Texto")]
    public float velocidadeEscrita = 0.04f;

    [Header("Cena")]
    public string nomeCenaJogoPrincipal = "SampleScene";


    // Itens encontrados
    private bool capacete;
    private bool tanque;
    private bool ferramenta;

    // Controle do diálogo
    private int etapa = 0;
    private bool dialogoAtivo;
    private bool escrevendo;

    private string textoAtual;
    private Coroutine digitacao;

    private Vector3 posicaoGato;


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


    void Start()
    {
        if (imagemGatinhoUI != null)
            posicaoGato = imagemGatinhoUI.rectTransform.anchoredPosition;

        if (painelBotoesEscolha != null)
            painelBotoesEscolha.SetActive(false);

        if (botaoAceitarPaxton != null)
            botaoAceitarPaxton.onClick.AddListener(
                () => EscolherPaxton(true));

        if (botaoRecusarPaxton != null)
            botaoRecusarPaxton.onClick.AddListener(
                () => EscolherPaxton(false));

        MostrarFala();
    }


    void Update()
    {
        // Não permite avançar o diálogo durante o Skill Check.
        if (SkillCheckManager.Instance != null &&
            SkillCheckManager.Instance.estaAtivo)
            return;

        if (dialogoAtivo &&
            (Input.GetKeyDown(KeyCode.Return) ||
             Input.GetKeyDown(KeyCode.KeypadEnter) ||
             Input.GetKeyDown(KeyCode.Space)))
        {
            AvancarTexto();
        }
    }


    // =========================================================
    // DIÁLOGO
    // =========================================================

    public void AvancarTexto()
    {
        // Primeiro aperto termina a animação da fala.
        if (escrevendo)
        {
            CompletarTexto();
            return;
        }

        // Não avança enquanto os botões de escolha estão ativos.
        if (painelBotoesEscolha != null &&
            painelBotoesEscolha.activeSelf)
            return;

        etapa++;
        MostrarFala();
    }


    void MostrarFala()
    {
        dialogoAtivo = true;

        painelBalaoFala?.SetActive(true);
        objetoGatinhoUI?.SetActive(true);
        botaoAvancarFala?.gameObject.SetActive(true);

        switch (etapa)
        {
            // =========================
            // INTRO
            // =========================

            case 0:
                Fala("Myke",
                    "Right... The abyss pressure is extreme, but my dive suit is almost ready.");
                break;

            case 1:
                Fala("Myke",
                    "I need to find 3 essential gear pieces left in the lab before I head to the boat.");
                break;

            case 2:
                Fala("Myke",
                    "Let's see: I need the Helmet, the Oxygen Tank, and the Calibrator Tool.");
                break;


            // =========================
            // EXPLORAÇÃO
            // =========================

            case 3:
                FecharDialogo();
                break;


            // =========================
            // ENCONTROU OS 3 ITENS
            // =========================

            case 4:
                Fala("Myke",
                    "Great! I found everything I need.");
                break;

            case 5:
                Fala("Narrator",
                    "Now it's time to put everything together.");
                break;

            case 6:
                Fala("Myke",
                    "Alright, let's assemble and calibrate the suit.");
                break;

            case 7:
                FecharDialogo();
                IniciarSkillCheck();
                break;


            // =========================
            // SKILL CHECK CONCLUÍDO
            // =========================

            case 8:
                Fala("Myke",
                    "Perfect! Calibration complete. The suit is fully operational.");
                break;


            // =========================
            // PAXTON
            // =========================

            case 9:
                Fala("Paxton",
                    "Wait! Myke, don't leave yet!");
                break;

            case 10:
                Fala("Myke",
                    "Paxton?! What are you doing here in my workshop?");
                break;

            case 11:
                Fala("Paxton",
                    "I looked over your depth schematics. You're going down to the trench alone? That's insane!");
                break;

            case 12:
                Fala("Myke",
                    "I don't need your help, Paxton. Not after what happened at the institute.");
                break;

            case 13:
                Fala("Paxton",
                    "I was wrong about the credit back then... I'm sorry. Just let me handle telemetry from the surface boat. Please.");
                break;


            // =========================
            // ESCOLHA
            // =========================

            case 14:
                botaoAvancarFala?.gameObject.SetActive(false);
                painelBotoesEscolha?.SetActive(true);
                break;


            // =========================
            // ERRO NO SKILL CHECK
            // =========================

            case 15:
                Fala("Myke",
                    "Oops, a valve slipped! I need to try the calibration again.");
                break;

            case 16:
                FecharDialogo();
                IniciarSkillCheck();
                break;
        }
    }


    void Fala(string personagem, string texto)
    {
        if (textoNomePersonagem != null)
            textoNomePersonagem.text =
                personagem == "Narrator" ? "" : personagem;

        IniciarDigitacao(texto);
    }


    void FecharDialogo()
    {
        dialogoAtivo = false;

        painelBalaoFala?.SetActive(false);
        objetoGatinhoUI?.SetActive(false);
        botaoAvancarFala?.gameObject.SetActive(false);
    }


    // =========================================================
    // BLOQUEIO DE INTERAÇÃO
    // =========================================================

    public bool PodeInteragir()
    {
        if (dialogoAtivo)
            return false;

        if (SkillCheckManager.Instance != null &&
            SkillCheckManager.Instance.estaAtivo)
            return false;

        return true;
    }


    public bool PodeColetarItem()
    {
        return PodeInteragir();
    }


    // =========================================================
    // ITENS
    // =========================================================

    public void ColetarItem(string nome)
    {
        switch (nome)
        {
            case "Capacete":
                capacete = true;
                break;

            case "Tanque":
                tanque = true;
                break;

            case "Ferramenta":
                ferramenta = true;
                break;
        }

        // Só continua quando os 3 foram encontrados.
        if (capacete && tanque && ferramenta)
        {
            etapa = 4;
            MostrarFala();
        }
    }


    // =========================================================
    // SKILL CHECK
    // =========================================================

    void IniciarSkillCheck()
    {
        if (SkillCheckManager.Instance == null)
        {
            Debug.LogError(
                "SkillCheckManager não encontrado!");
            return;
        }

        // 3 acertos
        // velocidade inicial = 1x
        // aumento = 35%
        SkillCheckManager.Instance.IniciarSequenciaMultipla(
            3,
            1f,
            1.35f,
            ResultadoSkillCheck
        );
    }


    void ResultadoSkillCheck(bool sucesso)
    {
        if (sucesso)
        {
            etapa = 8;
            MostrarFala();
        }
        else
        {
            etapa = 15;
            MostrarFala();
        }
    }


    // =========================================================
    // ESCOLHA DO PAXTON
    // =========================================================

    public void EscolherPaxton(bool aceitou)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.paxtonAcompanha = aceitou;

        SceneManager.LoadScene(nomeCenaJogoPrincipal);
    }


    // =========================================================
    // DIGITAÇÃO
    // =========================================================

    void IniciarDigitacao(string texto)
    {
        textoAtual = texto;

        if (digitacao != null)
            StopCoroutine(digitacao);

        digitacao = StartCoroutine(Digitar());
    }


    IEnumerator Digitar()
    {
        escrevendo = true;

        if (textoBalao != null)
            textoBalao.text = "";

        foreach (char letra in textoAtual)
        {
            if (textoBalao != null)
                textoBalao.text += letra;

            if (imagemGatinhoUI != null)
            {
                imagemGatinhoUI.sprite =
                    spriteNormalAberta;

                imagemGatinhoUI.rectTransform
                    .anchoredPosition =
                    posicaoGato + Vector3.up * 8f;
            }

            yield return new WaitForSeconds(
                velocidadeEscrita);

            if (imagemGatinhoUI != null)
            {
                imagemGatinhoUI.sprite =
                    spriteNormalFechada;

                imagemGatinhoUI.rectTransform
                    .anchoredPosition =
                    posicaoGato;
            }
        }

        escrevendo = false;
    }


    void CompletarTexto()
    {
        if (digitacao != null)
            StopCoroutine(digitacao);

        if (textoBalao != null)
            textoBalao.text = textoAtual;

        escrevendo = false;

        if (imagemGatinhoUI != null)
        {
            imagemGatinhoUI.sprite =
                spriteNormalFechada;

            imagemGatinhoUI.rectTransform
                .anchoredPosition =
                posicaoGato;
        }
    }


    // =========================================================
    // LORE
    // =========================================================

    public void ExibirLoreObjeto(
        string autor,
        string texto)
    {
        if (dialogoAtivo)
            return;

        dialogoAtivo = true;

        painelBalaoFala?.SetActive(true);
        objetoGatinhoUI?.SetActive(true);

        if (textoNomePersonagem != null)
            textoNomePersonagem.text =
                autor == "Narrator" ? "" : autor;

        IniciarDigitacao(texto);
    }
}