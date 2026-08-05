using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("Estilo Visual Novel (Gatinho)")]
    public Image imagemGatinho;          // Arraste a Image do Gatinho aqui!
    public float alturaPulo = 15f;        // Quantos pixels ele pula
    public float velocidadePulo = 12f;    // Velocidade do pulinho
    private Vector3 posicaoOriginalGato;  // Guarda a posição base do gatinho
    private Coroutine coroutinePulo;

    [Header("UI da Lista de Tarefas")]
    public GameObject painelListaItens;
    public TextMeshProUGUI textoLista;

    [Header("Configuração de Cenas")]
    public string nomeCenaJogoPrincipal = "SampleScene";

    // Estados do Tutorial
    private int etapaFala = 0;
    private bool falaAtiva = true;

    // Controle de Itens
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
        MostrarFalaAtual();
    }

    public void AvancarTexto()
    {
        etapaFala++;
        MostrarFalaAtual();
    }

    void MostrarFalaAtual()
    {
        falaAtiva = true;

        // Ativa o balão e o gatinho no estilo Visual Novel
        if (painelBalaoFala != null) painelBalaoFala.SetActive(true);
        if (imagemGatinho != null)
        {
            imagemGatinho.gameObject.SetActive(true);
            DarPulinhoNoGato(); // Faz o gatinho pular a cada nova frase!
        }

        if (botaoAvancarFala != null) botaoAvancarFala.gameObject.SetActive(true);

        switch (etapaFala)
        {
            case 0:
                textoBalao.text = "Ah, preciso fazer um experimento novo hoje!";
                break;

            case 1:
                textoBalao.text = "Para isso, preciso coletar alguns materiais no laboratório...";
                break;

            case 2:
                textoBalao.text = "Procure a lupa, o aquário e a tesoura!";
                break;

            case 3:
                // FECHA O DIÁLOGO E ESCONDE O GATINHO PARA PEGAR A LUPA
                EsconderDialogoEGato();
                if (painelListaItens != null) painelListaItens.SetActive(true);
                AtualizarTextoLista();
                break;

            case 4:
                // FALA DA LUPA
                textoBalao.text = "Encontrei a lupa! Agora consigo olhar melhor para ver se encontro outras coisas.";
                break;

            case 5:
                // DICA DE MOUSE / ZOOM
                textoBalao.text = "💡 <b>Dica:</b> Você pode aproximar usando a <b>Scroll Wheel</b> do mouse e arrastar o cenário!";
                break;

            case 6:
                // FECHA O DIÁLOGO E ESCONDE O GATINHO PARA PEGAR OS OUTROS ITENS
                EsconderDialogoEGato();
                break;

            case 7:
                // FALA FINAL
                textoBalao.text = "Ótimo, peguei tudo! Agora estou pronto para o experimento!";
                if (botaoAvancarFala != null)
                {
                    botaoAvancarFala.onClick.RemoveAllListeners();
                    botaoAvancarFala.onClick.AddListener(IrParaOJogo);
                }
                break;
        }
    }

    void EsconderDialogoEGato()
    {
        falaAtiva = false;
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
        if (imagemGatinho != null) imagemGatinho.gameObject.SetActive(false); // Sumiço no estilo VN!
    }

    // --- EFEITO DE PULINHO (BOUNCE) ---
    void DarPulinhoNoGato()
    {
        if (coroutinePulo != null) StopCoroutine(coroutinePulo);
        coroutinePulo = StartCoroutine(AnimarPulo());
    }

    IEnumerator AnimarPulo()
    {
        RectTransform rect = imagemGatinho.rectTransform;
        rect.anchoredPosition = posicaoOriginalGato;

        // Sobe o gatinho
        Vector3 posTopo = posicaoOriginalGato + new Vector3(0, alturaPulo, 0);
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * velocidadePulo;
            rect.anchoredPosition = Vector3.Lerp(posicaoOriginalGato, posTopo, t);
            yield return null;
        }

        // Volta o gatinho para o chão
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadePulo;
            rect.anchoredPosition = Vector3.Lerp(posTopo, posicaoOriginalGato, t);
            yield return null;
        }

        rect.anchoredPosition = posicaoOriginalGato;
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

        textoLista.text = $"<b>Materiais:</b>\n" +
            $"{(pegouLupa ? "<s>• Lupa</s>" : "• Lupa")}\n" +
            $"{(pegouAquario ? "<s>• Aquário</s>" : "• Aquário")}\n" +
            $"{(pegouTesoura ? "<s>• Tesoura</s>" : "• Tesoura")}";
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