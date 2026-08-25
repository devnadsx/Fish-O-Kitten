using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct ExpressaoGatinho
{
    public string nomeExpressao;
    public Sprite sprite;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Aviso de Controles (Canva)")]
    public GameObject imagemAvisoControles;

    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("Sprites do Gatinho (Padrão)")]
    public Image imagemGatinho;           // Componente Image na tela
    public Sprite spriteBocaFechada;      // Sprite padrão (Rosto normal)
    public Sprite spriteBocaAberta;       // Sprite falando (Boca aberta)

    [Header("Sprites do Gatinho (Final - Todos os Itens)")]
    public Sprite spriteBocaFechadaFinal; // Rosto especial com olhos/expressão diferente (fechado)
    public Sprite spriteBocaAbertaFinal;  // Rosto especial falando (aberto)

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
    public MonoBehaviour[] scriptsParaAtivarAposLupa; // Scripts que serão liberados só após achar a lupa!

    [Header("Configuração de Cenas")]
    public string nomeCenaJogoPrincipal = "SampleScene";

    // Estados
    private int etapaFala = 0;
    private bool falaAtiva = true;

    // Itens
    private bool pegouLupa = false;
    private bool pegouAquario = false;
    private bool pegouTesoura = false;

    private Vector3 posicaoOriginalGato;
    private Coroutine coroutineEscrita;
    private bool estaEscrevendo = false;
    private string textoCompletoAtual = "";

    // Controle dinâmico da expressão atual
    private Sprite spriteFechadaAtual;
    private Sprite spriteAbertaAtual;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Define as expressões iniciais como o padrão
        spriteFechadaAtual = spriteBocaFechada;
        spriteAbertaAtual = spriteBocaAberta;

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

        // Garante que os scripts pós-lupa comecem desativados
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
        if (imagemGatinho != null) imagemGatinho.gameObject.SetActive(true);
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
                // Oculta o diálogo para o jogador procurar os itens na tela
                EsconderDialogoEGato();
                if (painelListaItens != null) painelListaItens.SetActive(true);
                AtualizarTextoLista();
                break;

            case 5:
                // Fala acionada imediatamente após encontrar a LUPA!
                IniciarDigitacao("I've found the magnifying glass! Now I can have a closer look to see if I can find the rest...");
                break;

            case 6:
                IniciarDigitacao("Tip: You can move the scene and zoom in on it for a better view!");
                break;

            case 7:
                // Oculta o diálogo para o jogador continuar buscando os itens restantes
                EsconderDialogoEGato();
                break;

            case 8:
                // 🎭 Troca para os Sprites Especiais de comemoração final!
                if (spriteBocaFechadaFinal != null) spriteFechadaAtual = spriteBocaFechadaFinal;
                if (spriteBocaAbertaFinal != null) spriteAbertaAtual = spriteBocaAbertaFinal;

                DefinirSpriteNormal();

                // Fala acionada após pegar TODOS os 3 itens!
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
        if (textoBalao != null) textoBalao.text = "";

        foreach (char letra in texto.ToCharArray())
        {
            if (textoBalao != null) textoBalao.text += letra;

            if (char.IsLetterOrDigit(letra))
            {
                if (imagemGatinho != null && spriteAbertaAtual != null)
                {
                    imagemGatinho.sprite = spriteAbertaAtual;
                }

                if (imagemGatinho != null && imagemGatinho.gameObject.activeInHierarchy)
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

        if (imagemGatinho != null)
        {
            imagemGatinho.rectTransform.anchoredPosition = posicaoOriginalGato;
        }
    }

    IEnumerator PulinhoRapidoGato()
    {
        if (imagemGatinho == null) yield break;

        RectTransform rect = imagemGatinho.rectTransform;
        rect.anchoredPosition = posicaoOriginalGato + new Vector3(0, alturaPuloLetra, 0);
        yield return new WaitForSeconds(velocidadeEscrita * 0.5f);
        rect.anchoredPosition = posicaoOriginalGato;
    }

    void DefinirSpriteNormal()
    {
        if (imagemGatinho != null && spriteFechadaAtual != null)
        {
            imagemGatinho.sprite = spriteFechadaAtual;
        }
    }

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

    // 🔓 Permite pegar QUALQUER item a qualquer momento (desde que não esteja em um diálogo)
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

            // Ativa os scripts liberados após a Lupa!
            DefinirEstadoScriptsPosLupa(true);

            // Redireciona para a etapa 5 (Fala da descoberta da Lupa)
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

        // Se coletou todos os 3 itens, aciona a fala final (Etapa 8)
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