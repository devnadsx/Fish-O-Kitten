using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class FishAnalyseManager : MonoBehaviour
{
    public static FishAnalyseManager Instance;

    [Header("Aviso de Controles (Canva)")]
    public GameObject imagemAvisoControles;

    [Header("UI do Diálogo (Estilo Visual Novel)")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoBalao;
    public Image imagemGatinho;
    public Button botaoAvancarFala;

    [Header("Estilo Efeitos Visual Novel / RPG")]
    public float velocidadeEscrita = 0.03f;
    public float alturaPuloLetra = 8f;
    public AudioSource audioSourceSFX;
    public AudioClip somFalaGatinho;

    [Header("Configuração da Cena")]
    public string nomeProximaFase = "Game2";
    public int totalPeixesNaMesa = 3;
    private int peixesProcessados = 0;

    private Vector3 posicaoOriginalGato;
    private Coroutine coroutineDigitacao;
    private bool estaEscrevendo = false;
    private string textoCompletoAtual = "";

    // Guarda o peixe em teste no momento
    private GameObject peixeAtualObjeto;
    private string peixeAtualNome;
    private bool peixeAtualEhVenenoso;

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

        if (imagemAvisoControles != null)
        {
            imagemAvisoControles.SetActive(true);
        }

        Falar("Time to test and taste these fish! Select one from the table.");
    }

    void Update()
    {
        // Avançar diálogo pressionando ENTER ou Keypad ENTER
        if (painelBalaoFala != null && painelBalaoFala.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            AvancarTexto();
        }
    }

    // Chamado pelo Botão / ENTER / Clique
    public void AvancarTexto()
    {
        // Esconde o aviso de controles no primeiro clique/enter
        if (imagemAvisoControles != null && imagemAvisoControles.activeSelf)
        {
            imagemAvisoControles.SetActive(false);
        }

        // Se ainda está digitando, completa o texto imediatamente
        if (estaEscrevendo)
        {
            CompletarTextoImediatamente();
            return;
        }

        // Se todos os peixes já foram analisados, avança para a próxima cena
        if (peixesProcessados >= totalPeixesNaMesa)
        {
            IrParaProximaFase();
        }
        else
        {
            if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
        }
    }

    public void IniciarTesteDoPeixe(string nome, bool ehVenenoso, GameObject objetoPeixe)
    {
        // Esconde o aviso de controles se ainda estiver ativo
        if (imagemAvisoControles != null && imagemAvisoControles.activeSelf)
        {
            imagemAvisoControles.SetActive(false);
        }

        peixeAtualNome = nome;
        peixeAtualEhVenenoso = ehVenenoso;
        peixeAtualObjeto = objetoPeixe;

        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);

        // 🎯 AQUI ESTÁ A MUDANÇA:
        if (ehVenenoso)
        {
            // Se for VENENOSO -> Ativa o Skill Check!
            if (SkillCheckManager.Instance != null)
            {
                SkillCheckManager.Instance.IniciarSequenciaSkillCheck();
            }
        }
        else
        {
            // Se NÃO for venenoso -> Pula o Skill Check e considera Sucesso Direto!
            OnSkillCheckSucesso();
        }
    }
    public void OnSkillCheckSucesso()
    {
        if (peixeAtualObjeto != null) peixeAtualObjeto.SetActive(false);
        peixesProcessados++;

        if (peixeAtualEhVenenoso)
        {
            Falar($"Ugh! The {peixeAtualNome} was super poisonous! Good thing I tested it carefully!");
        }
        else
        {
            Falar($"Yum! The {peixeAtualNome} is delicious and perfectly safe!");
        }

        VerificarFimDaAnalise();
    }

    public void OnSkillCheckFalha()
    {
        if (peixeAtualObjeto != null) peixeAtualObjeto.SetActive(false);
        peixesProcessados++;

        Falar($"Oops! I dropped the {peixeAtualNome}! It fell off the table...");

        VerificarFimDaAnalise();
    }

    void VerificarFimDaAnalise()
    {
        if (peixesProcessados >= totalPeixesNaMesa)
        {
            if (botaoAvancarFala != null)
            {
                botaoAvancarFala.onClick.RemoveAllListeners();
                botaoAvancarFala.onClick.AddListener(IrParaProximaFase);
            }
        }
    }

    public void Falar(string texto)
    {
        if (painelBalaoFala != null) painelBalaoFala.SetActive(true);
        if (imagemGatinho != null) imagemGatinho.gameObject.SetActive(true);

        textoCompletoAtual = texto;

        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        coroutineDigitacao = StartCoroutine(EfeitoDigitar(texto));
    }

    IEnumerator EfeitoDigitar(string texto)
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
        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
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

    // Função para botão de Pular Fase/Cena
    public void PularAnalise()
    {
        IrParaProximaFase();
    }

    public void IrParaProximaFase()
    {
        SceneManager.LoadScene(nomeProximaFase);
    }
}