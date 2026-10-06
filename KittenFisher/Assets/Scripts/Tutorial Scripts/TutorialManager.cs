using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[Serializable]
public class PersonagemTutorial
{
    public string nome;
    public Sprite spriteBocaFechada;
    public Sprite spriteBocaAberta;
    public AudioClip somFala;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoBalao;
    public Button botaoAvancarFala;

    [Header("Banco de Personagens")]
    public List<PersonagemTutorial> personagens = new List<PersonagemTutorial>();

    [Header("Imagens dos Personagens")]
    public Image imagemMyke;
    public Image imagemPaxton;

    [Header("Animação")]
    public float alturaPuloLetra = 8f;

    [Header("Áudio")]
    public AudioSource audioSourceFala;
    [Range(0f, 1f)]
    public float volumeFala = 0.5f;

    [Header("Escolhas")]
    public GameObject painelBotoesEscolha;
    public Button botaoAceitarPaxton;
    public Button botaoRecusarPaxton;
    public TextMeshProUGUI textoBotaoAceitar;
    public TextMeshProUGUI textoBotaoRecusar;

    [Header("Gatinho")]
    public GameObject objetoGatinhoUI;

    [Header("Texto")]
    public float velocidadeEscrita = 0.04f;

    [Header("Skill Check")]
    public GameObject skillCheckObjeto;

    [Header("Cena")]
    public string nomeCenaJogoPrincipal = "SampleScene";

    private bool capacete;
    private bool tanque;
    private bool ferramenta;

    private int etapa;

    private bool dialogoAtivo;
    private bool escrevendo;
    private bool lendoLore;

    private string textoAtual;
    private Coroutine digitacao;

    private PersonagemTutorial personagemAtivo;
    private Image imagemAtivaAtual;
    private Vector3 posicaoOriginal;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (painelBotoesEscolha != null)
            painelBotoesEscolha.SetActive(false);

        if (skillCheckObjeto != null)
            skillCheckObjeto.SetActive(false);

        if (imagemMyke != null)
            imagemMyke.gameObject.SetActive(false);

        if (imagemPaxton != null)
            imagemPaxton.gameObject.SetActive(false);

        // Configura os botões pelo código
        if (botaoAceitarPaxton != null)
        {
            botaoAceitarPaxton.onClick.RemoveAllListeners();
            botaoAceitarPaxton.onClick.AddListener(EscolhaAceitarPaxton);
        }

        if (botaoRecusarPaxton != null)
        {
            botaoRecusarPaxton.onClick.RemoveAllListeners();
            botaoRecusarPaxton.onClick.AddListener(EscolhaRecusarPaxton);
        }

        MostrarFala();
    }

    void Update()
    {
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

    public void AvancarTexto()
    {
        if (escrevendo)
        {
            CompletarTexto();
            return;
        }

        if (lendoLore)
        {
            lendoLore = false;
            FecharDialogo();
            return;
        }

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
            case 0:
                Fala("Myke",
                    "Certo... preciso pensar em como entraria água abaixo, como eu faria isso?");
                break;

            case 1:
                Fala("Narrator",
                    "Ele anota algumas ideias, até se decidir por completo.");
                break;

            case 2:
                Fala("Myke",
                    "Beleza, acho mais seguro construir um traje de mergulho. É uma boa ideia até.");
                break;

            case 3:
                Fala("Myke",
                    "Vamos ver... Eu precisaria dos tubos de oxigênios, daquele traje velho e da caixa de ferramentas.");
                break;

            case 4:
                FecharDialogo();
                break;

            case 5:
                Fala("Myke",
                    "Ótimo! Encontrei tudo o que precisava.");
                break;

            case 6:
                Fala("Narrator",
                    "Myke coloca os itens na bancada, e começa a abrir uma caixa de ferramentas.");
                break;

            case 7:
                Fala("Myke",
                    "Certo, vamos montar e calibrar o traje.");
                break;

            case 8:
                FecharDialogo();
                IniciarSkillCheck();
                break;

            case 9:
                Fala("Myke",
                    "Perfeito! Calibração concluída. O traje está funcionando perfeitamente.");
                break;

            case 10:
                Fala("Paxton",
                    "Espera! Myke, não vá ainda!");
                break;

            case 11:
                Fala("Myke",
                    "Paxton?! O que você está fazendo aqui no meu laboratório?");
                break;

            case 12:
                Fala("Paxton",
                    "Eu vi seus cálculos de profundidade. Você pretende descer sozinho até as águas profundas? Isso é loucura!");
                break;

            case 13:
                Fala("Myke",
                    "Eu não preciso da sua ajuda, Paxton. Não depois do que você fez...");
                break;

            case 14:
                Fala("Paxton",
                    "Eu estava errado de ter pego todo o crédito... Me desculpa. Só me deixe te ajudar com os oxigênios do traje. Por favor! Sua vida tá em risco!");
                break;

            case 15:
                AbrirEscolhas();
                break;

            // Caminho: aceitar ajuda
            case 16:
                Fala("Myke",
                    "Tudo bem. Pode cuidar da telemetria enquanto eu termino os últimos preparativos.");
                break;

            case 17:
                Fala("Paxton",
                    "Pode deixar. Vou ficar de olho em tudo lá de cima. E, por favor, toma cuidado.");
                break;

            case 18:
                Fala("Myke",
                    "Eu vou tomar. Obrigado por vir até aqui, Paxton.");
                break;

            case 19:
                Fala("Paxton",
                    "Então estamos combinados. Te encontro no barco.");
                break;

            case 20:
                FinalizarTutorial();
                break;

            // Caminho: recusar ajuda
            case 21:
                Fala("Myke",
                    "Não. Eu vou fazer isso sozinho.");
                break;

            case 22:
                Fala("Paxton",
                    "Myke, isso não é uma boa ideia. Você não precisa provar nada para ninguém.");
                break;

            case 23:
                Fala("Myke",
                    "Não, Paxton. Eu não vou mudar de ideia, eu disse não.");
                break;

            case 24:
                Fala("Paxton",
                    "M-Mas, Myke! Eu só quero ajudar.");
                break;

            case 25:
                Fala("Myke",
                    "Paxton. Saia. Agora.");
                break;

            case 26:
                FinalizarTutorial();
                break;

            // Falha no Skill Check
            case 27:
                Fala("Myke",
                    "Ugh! Uma válvula escapou... preciso tentar calibrar o traje novamente.");
                break;

            case 28:
                FecharDialogo();
                IniciarSkillCheck();
                break;
        }
    }

    void Fala(string personagem, string texto)
    {
        personagemAtivo = BuscarPersonagem(personagem);

        if (textoNomePersonagem != null)
            textoNomePersonagem.text =
                personagem == "Narrator" ? "" : personagem;

        AtualizarPersonagem();
        IniciarDigitacao(texto);
    }

    PersonagemTutorial BuscarPersonagem(string nome)
    {
        foreach (PersonagemTutorial personagem in personagens)
        {
            if (personagem.nome.ToLower() == nome.ToLower())
                return personagem;
        }

        return null;
    }

    void AbrirEscolhas()
    {
        dialogoAtivo = false;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(false);

        if (botaoAvancarFala != null)
            botaoAvancarFala.gameObject.SetActive(false);

        if (painelBotoesEscolha != null)
            painelBotoesEscolha.SetActive(true);

        if (textoBotaoAceitar != null)
            textoBotaoAceitar.text = "Pode contar comigo.";

        if (textoBotaoRecusar != null)
            textoBotaoRecusar.text = "Não. Vou fazer isso sozinho.";

        if (botaoAceitarPaxton != null)
            botaoAceitarPaxton.interactable = true;

        if (botaoRecusarPaxton != null)
            botaoRecusarPaxton.interactable = true;
    }


    public void EscolhaAceitarPaxton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.paxtonAcompanha = true;

        FecharEscolhas();

        etapa = 16;
        MostrarFala();
    }

    public void EscolhaRecusarPaxton()
    {

        if (GameManager.Instance != null)
            GameManager.Instance.paxtonAcompanha = false;

        FecharEscolhas();

        etapa = 21;
        MostrarFala();
    }

    void FecharEscolhas()
    {
        if (painelBotoesEscolha != null)
            painelBotoesEscolha.SetActive(false);

        if (botaoAceitarPaxton != null)
            botaoAceitarPaxton.interactable = false;

        if (botaoRecusarPaxton != null)
            botaoRecusarPaxton.interactable = false;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        if (botaoAvancarFala != null)
            botaoAvancarFala.gameObject.SetActive(true);
    }

    void AtualizarPersonagem()
    {
        if (imagemMyke != null)
            imagemMyke.gameObject.SetActive(false);

        if (imagemPaxton != null)
            imagemPaxton.gameObject.SetActive(false);

        imagemAtivaAtual = null;

        if (personagemAtivo == null)
            return;

        if (personagemAtivo.nome.ToLower() == "myke")
            imagemAtivaAtual = imagemMyke;
        else if (personagemAtivo.nome.ToLower() == "paxton")
            imagemAtivaAtual = imagemPaxton;

        if (imagemAtivaAtual == null)
            return;

        if (personagemAtivo.spriteBocaFechada != null)
            imagemAtivaAtual.sprite = personagemAtivo.spriteBocaFechada;

        imagemAtivaAtual.gameObject.SetActive(true);

        posicaoOriginal =
            imagemAtivaAtual.rectTransform.anchoredPosition;
    }

    void FecharDialogo()
    {
        dialogoAtivo = false;

        painelBalaoFala?.SetActive(false);
        objetoGatinhoUI?.SetActive(false);
        botaoAvancarFala?.gameObject.SetActive(false);

        if (imagemMyke != null)
            imagemMyke.gameObject.SetActive(false);

        if (imagemPaxton != null)
            imagemPaxton.gameObject.SetActive(false);
    }

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

        if (capacete && tanque && ferramenta)
        {
            etapa = 5;
            MostrarFala();
        }
    }

    void IniciarSkillCheck()
    {
        if (skillCheckObjeto == null)
            return;

        skillCheckObjeto.SetActive(true);

        if (SkillCheckManager.Instance == null)
            return;

        SkillCheckManager.Instance.IniciarSequenciaMultipla(
            3,
            1f,
            1.35f,
            ResultadoSkillCheck
        );
    }

    void ResultadoSkillCheck(bool sucesso)
    {
        if (skillCheckObjeto != null)
            skillCheckObjeto.SetActive(false);

        if (sucesso)
        {
            etapa = 9;
            MostrarFala();
        }
        else
        {
            etapa = 27;
            MostrarFala();
        }
    }

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

            if (imagemAtivaAtual != null &&
                imagemAtivaAtual.gameObject.activeSelf &&
                char.IsLetterOrDigit(letra))
            {
                if (personagemAtivo != null &&
                    personagemAtivo.spriteBocaAberta != null)
                {
                    imagemAtivaAtual.sprite =
                        personagemAtivo.spriteBocaAberta;
                }

                StartCoroutine(PularPersonagem());

                if (audioSourceFala != null &&
                    personagemAtivo != null &&
                    personagemAtivo.somFala != null)
                {
                    audioSourceFala.PlayOneShot(
                        personagemAtivo.somFala,
                        volumeFala
                    );
                }
            }

            yield return new WaitForSeconds(velocidadeEscrita);

            if (imagemAtivaAtual != null &&
                personagemAtivo != null &&
                personagemAtivo.spriteBocaFechada != null)
            {
                imagemAtivaAtual.sprite =
                    personagemAtivo.spriteBocaFechada;
            }
        }

        escrevendo = false;
        RestaurarPersonagem();
    }

    IEnumerator PularPersonagem()
    {
        if (imagemAtivaAtual == null)
            yield break;

        RectTransform rect =
            imagemAtivaAtual.rectTransform;

        rect.anchoredPosition =
            posicaoOriginal +
            Vector3.up * alturaPuloLetra;

        yield return new WaitForSeconds(
            velocidadeEscrita * 0.5f
        );

        rect.anchoredPosition = posicaoOriginal;
    }

    void RestaurarPersonagem()
    {
        if (imagemAtivaAtual == null)
            return;

        imagemAtivaAtual.rectTransform
            .anchoredPosition = posicaoOriginal;

        if (personagemAtivo != null &&
            personagemAtivo.spriteBocaFechada != null)
        {
            imagemAtivaAtual.sprite =
                personagemAtivo.spriteBocaFechada;
        }
    }

    void CompletarTexto()
    {
        if (digitacao != null)
            StopCoroutine(digitacao);

        if (textoBalao != null)
            textoBalao.text = textoAtual;

        escrevendo = false;
        RestaurarPersonagem();
    }

    public void ExibirLoreObjeto(string autor, string texto)
    {
        if (dialogoAtivo)
            return;

        lendoLore = true;
        dialogoAtivo = true;

        painelBalaoFala?.SetActive(true);
        objetoGatinhoUI?.SetActive(true);
        botaoAvancarFala?.gameObject.SetActive(true);

        personagemAtivo = BuscarPersonagem(autor);

        if (textoNomePersonagem != null)
            textoNomePersonagem.text =
                autor == "Narrator" ? "" : autor;

        AtualizarPersonagem();
        IniciarDigitacao(texto);
    }

    void FinalizarTutorial()
    {
        FecharDialogo();
        SceneManager.LoadScene(nomeCenaJogoPrincipal);
    }
}
