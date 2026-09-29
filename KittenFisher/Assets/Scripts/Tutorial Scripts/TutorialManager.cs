using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Aviso de Controles")]
    public GameObject imagemAvisoControles;

    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("UI dos Botões de Escolha do Paxton")]
    public GameObject painelBotoesEscolha;
    public Button botaoAceitarPaxton;
    public Button botaoRecusarPaxton;

    [Header("Gatinho UI")]
    public GameObject objetoGatinhoUI;
    public Image imagemGatinhoUI;
    public Sprite spriteNormalFechada;
    public Sprite spriteNormalAberta;

    [Header("Efeitos e Som")]
    public float velocidadeEscrita = 0.04f;
    public float alturaPuloLetra = 8f;
    public AudioSource audioSourceSFX;
    public AudioClip somFalaGatinho;

    [Header("UI da Lista de Tarefas")]
    public GameObject painelListaItens;
    public TextMeshProUGUI textoLista;

    [Header("Desbloquear Gameplay Após Pegar o Capacete")]
    public MonoBehaviour[] scriptsParaAtivarAposCapacete;

    [Header("Configuração de Cenas")]
    public string nomeCenaJogoPrincipal = "SampleScene";

    // Estados
    private int etapaFala = 0;
    private bool falaAtiva = true;

    private bool pegouCapacete = false;
    private bool pegouTanque = false;
    private bool pegouFerramenta = false;

    private Vector3 posicaoOriginalGato;
    private Coroutine coroutineEscrita;
    private bool estaEscrevendo = false;
    private string textoCompletoAtual = "";

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (imagemGatinhoUI != null)
        {
            posicaoOriginalGato = imagemGatinhoUI.rectTransform.anchoredPosition;
        }

        if (painelListaItens != null) painelListaItens.SetActive(false);
        if (painelBotoesEscolha != null) painelBotoesEscolha.SetActive(false);

        if (imagemAvisoControles != null)
        {
            imagemAvisoControles.SetActive(true);
        }

        if (botaoAceitarPaxton != null)
            botaoAceitarPaxton.onClick.AddListener(() => EscolherPaxton(true));

        if (botaoRecusarPaxton != null)
            botaoRecusarPaxton.onClick.AddListener(() => EscolherPaxton(false));

        DefinirSpriteFechada();
        DefinirEstadoScripts(false);
        MostrarFalaAtual();
    }

    void Update()
    {
        // Se o Skill Check estiver aberto, não permite avançar falas
        if (SkillCheckManager.Instance != null && SkillCheckManager.Instance.estaAtivo) return;

        if (falaAtiva && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)))
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

        if (painelBotoesEscolha != null && painelBotoesEscolha.activeSelf) return;

        etapaFala++;
        MostrarFalaAtual();
    }

    void MostrarFalaAtual()
    {
        falaAtiva = true;

        if (painelBalaoFala != null) painelBalaoFala.SetActive(true);
        if (objetoGatinhoUI != null) objetoGatinhoUI.SetActive(true);
        if (botaoAvancarFala != null) botaoAvancarFala.gameObject.SetActive(true);

        switch (etapaFala)
        {
            case 0:
                DefinirNomePersonagem("Myke");
                IniciarDigitacao("Right... The abyss pressure is extreme, but my dive suit is almost ready.");
                break;

            case 1:
                DefinirNomePersonagem("Myke");
                IniciarDigitacao("I need to find 3 essential gear pieces left in the lab before I head to the boat.");
                break;

            case 2:
                DefinirNomePersonagem("Myke");
                IniciarDigitacao("Let's see: I need the Helmet, the Oxygen Tank, and the Calibrator Tool.");
                break;

            case 3:
                EsconderDialogoEGato();
                if (painelListaItens != null) painelListaItens.SetActive(true);
                AtualizarTextoLista();
                break;

            case 4:
                DefinirNomePersonagem("Myke");
                IniciarDigitacao("Got the Helmet! The HUD scope is active now.");
                break;

            case 5:
                DefinirNomePersonagem("Narrator");
                IniciarDigitacao("Tip: You can move the camera and zoom in on objects in the lab to inspect details!");
                break;

            case 6:
                EsconderDialogoEGato();
                break;

            // --- MONTAGEM DO TRAJE (INÍCIO DO SKILL CHECK) ---
            case 7:
                DefinirNomePersonagem("Myke");
                IniciarDigitacao("Alright, I have all the pieces! Now I need to assemble and calibrate the suit's pressure valves.");
                break;

            case 8:
                EsconderDialogoEGato();
                IniciarDesafioSkillCheck();
                break;

            case 9:
                DefinirNomePersonagem("Myke");
                IniciarDigitacao("Perfect! Calibration complete. The suit is fully operational.");
                break;

            // --- ENTRADA DO PAXTON ---
            case 10:
                DefinirNomePersonagem("Paxton");
                IniciarDigitacao("Wait! Myke, don't leave yet!");
                break;

            case 11:
                DefinirNomePersonagem("Myke");
                IniciarDigitacao("Paxton?! What are you doing here in my workshop?");
                break;

            case 12:
                DefinirNomePersonagem("Paxton");
                IniciarDigitacao("I looked over your depth schematics. You're going down to the trench alone? That's insane!");
                break;

            case 13:
                DefinirNomePersonagem("Myke");
                IniciarDigitacao("I don't need your help, Paxton. Not after what happened at the institute.");
                break;

            case 14:
                DefinirNomePersonagem("Paxton");
                IniciarDigitacao("I was wrong about the credit back then... I'm sorry. Just let me handle telemetry from the surface boat. Please.");
                break;

            case 15:
                if (botaoAvancarFala != null) botaoAvancarFala.gameObject.SetActive(false);
                if (painelBotoesEscolha != null) painelBotoesEscolha.SetActive(true);
                break;
        }
    }

    void IniciarDesafioSkillCheck()
    {
        if (SkillCheckManager.Instance != null)
        {
            // Parâmetros: 3 acertos necessários, velocidade base x1.0, e aumento de 35% a cada acerto
            SkillCheckManager.Instance.IniciarSequenciaMultipla(3, 1.0f, 1.35f, OnSkillCheckResultado);
        }
    }

    void OnSkillCheckResultado(bool sucesso)
    {
        if (sucesso)
        {
            etapaFala = 9;
            MostrarFalaAtual();
        }
        else
        {
            // Em caso de erro, dá uma mensagem e tenta de novo
            DefinirNomePersonagem("Myke");
            IniciarDigitacao("Oops, a valve slipped! Let me retry calibrating...");
            etapaFala = 7; // Volta para tentar novamente na próxima barra
        }
    }

    public void ExibirLoreObjeto(string nomeAutor, string textoLore)
    {
        if (falaAtiva) return;

        falaAtiva = true;
        if (painelBalaoFala != null) painelBalaoFala.SetActive(true);
        if (objetoGatinhoUI != null) objetoGatinhoUI.SetActive(true);

        DefinirNomePersonagem(nomeAutor);
        IniciarDigitacao(textoLore);
    }

    private void DefinirNomePersonagem(string nome)
    {
        if (textoNomePersonagem != null)
        {
            textoNomePersonagem.text = nome == "Narrator" ? "" : nome;
        }
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
                DefinirSpriteFechada();
            }

            yield return new WaitForSeconds(velocidadeEscrita);
        }

        DefinirSpriteFechada();
        estaEscrevendo = false;
    }

    void CompletarTextoImediatamente()
    {
        if (coroutineEscrita != null) StopCoroutine(coroutineEscrita);
        if (textoBalao != null) textoBalao.text = textoCompletoAtual;

        DefinirSpriteFechada();
        estaEscrevendo = false;
        ResetarPosicaoGato();
    }

    IEnumerator PulinhoRapidoGato()
    {
        if (imagemGatinhoUI == null || !imagemGatinhoUI.gameObject.activeInHierarchy) yield break;

        RectTransform rect = imagemGatinhoUI.rectTransform;
        rect.anchoredPosition = posicaoOriginalGato + new Vector3(0, alturaPuloLetra, 0);
        yield return new WaitForSeconds(velocidadeEscrita * 0.5f);
        rect.anchoredPosition = posicaoOriginalGato;
    }

    void ResetarPosicaoGato()
    {
        if (imagemGatinhoUI != null)
            imagemGatinhoUI.rectTransform.anchoredPosition = posicaoOriginalGato;
    }

    void DefinirSpriteFechada()
    {
        if (imagemGatinhoUI != null && spriteNormalFechada != null)
            imagemGatinhoUI.sprite = spriteNormalFechada;
    }

    void DefinirSpriteAberta()
    {
        if (imagemGatinhoUI != null && spriteNormalAberta != null)
            imagemGatinhoUI.sprite = spriteNormalAberta;
    }

    void EsconderDialogoEGato()
    {
        falaAtiva = false;
        DefinirSpriteFechada();
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
        if (objetoGatinhoUI != null) objetoGatinhoUI.SetActive(false);
    }

    public bool PodeColetarItem()
    {
        return !falaAtiva;
    }

    public void ColetarItem(string nome)
    {
        if (nome == "Capacete")
        {
            pegouCapacete = true;
            AtualizarTextoLista();
            DefinirEstadoScripts(true);

            etapaFala = 4;
            MostrarFalaAtual();
        }
        else if (nome == "Tanque")
        {
            pegouTanque = true;
            AtualizarTextoLista();
        }
        else if (nome == "Ferramenta")
        {
            pegouFerramenta = true;
            AtualizarTextoLista();
        }

        if (pegouCapacete && pegouTanque && pegouFerramenta)
        {
            etapaFala = 7;
            MostrarFalaAtual();
        }
    }

    void DefinirEstadoScripts(bool estado)
    {
        if (scriptsParaAtivarAposCapacete != null)
        {
            foreach (MonoBehaviour script in scriptsParaAtivarAposCapacete)
            {
                if (script != null) script.enabled = estado;
            }
        }
    }

    void AtualizarTextoLista()
    {
        if (textoLista == null) return;

        textoLista.text = $"<b>Gear Checklist:</b>\n" +
            $"{(pegouCapacete ? "<s>• Dive Helmet</s>" : "• Dive Helmet")}\n" +
            $"{(pegouTanque ? "<s>• Oxygen Tank</s>" : "• Oxygen Tank")}\n" +
            $"{(pegouFerramenta ? "<s>• Calibrator Tool</s>" : "• Calibrator Tool")}";
    }

    public void EscolherPaxton(bool aceitou)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.paxtonAcompanha = aceitou;
        }

        IrParaOJogo();
    }

    public void IrParaOJogo()
    {
        SceneManager.LoadScene(nomeCenaJogoPrincipal);
    }
}