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
    public List<PersonagemTutorial> personagens =
        new List<PersonagemTutorial>();

    [Header("Imagem do Personagem")]
    public Image imagemPersonagem;

    [Header("Animação")]
    public float alturaPuloLetra = 8f;

    [Header("Áudio")]
    public AudioSource audioSourceFala;
    [Range(0f, 1f)]
    public float volumeFala = 0.5f;

    [Header("Escolha do Paxton")]
    public GameObject painelBotoesEscolha;
    public Button botaoAceitarPaxton;
    public Button botaoRecusarPaxton;

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

    private int etapa = 0;

    private bool dialogoAtivo;
    private bool escrevendo;
    private bool lendoLore;

    private string textoAtual;

    private Coroutine digitacao;

    private Vector3 posicaoPersonagem;

    private PersonagemTutorial personagemAtivo;


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


    void Start()
    {
        if (imagemPersonagem != null)
            posicaoPersonagem =
                imagemPersonagem.rectTransform.anchoredPosition;

        if (painelBotoesEscolha != null)
            painelBotoesEscolha.SetActive(false);

        if (skillCheckObjeto != null)
            skillCheckObjeto.SetActive(false);

        if (botaoAceitarPaxton != null)
        {
            botaoAceitarPaxton.onClick.AddListener(
                () => EscolherPaxton(true));
        }

        if (botaoRecusarPaxton != null)
        {
            botaoRecusarPaxton.onClick.AddListener(
                () => EscolherPaxton(false));
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
                Fala(
                    "Myke",
                    "Right... The abyss pressure is extreme, but my dive suit is almost ready."
                );
                break;

            case 1:
                Fala(
                    "Myke",
                    "I need to find 3 essential gear pieces left in the lab before I head to the boat."
                );
                break;

            case 2:
                Fala(
                    "Myke",
                    "Let's see: I need the Helmet, the Oxygen Tank, and the Calibrator Tool."
                );
                break;

            case 3:
                FecharDialogo();
                break;

            case 4:
                Fala(
                    "Myke",
                    "Great! I found everything I need."
                );
                break;

            case 5:
                Fala(
                    "Narrator",
                    "Now it's time to put everything together."
                );
                break;

            case 6:
                Fala(
                    "Myke",
                    "Alright, let's assemble and calibrate the suit."
                );
                break;

            case 7:
                FecharDialogo();
                IniciarSkillCheck();
                break;

            case 8:
                Fala(
                    "Myke",
                    "Perfect! Calibration complete. The suit is fully operational."
                );
                break;

            case 9:
                Fala(
                    "Paxton",
                    "Wait! Myke, don't leave yet!"
                );
                break;

            case 10:
                Fala(
                    "Myke",
                    "Paxton?! What are you doing here in my workshop?"
                );
                break;

            case 11:
                Fala(
                    "Paxton",
                    "I looked over your depth schematics. You're going down to the trench alone? That's insane!"
                );
                break;

            case 12:
                Fala(
                    "Myke",
                    "I don't need your help, Paxton. Not after what happened at the institute."
                );
                break;

            case 13:
                Fala(
                    "Paxton",
                    "I was wrong about the credit back then... I'm sorry. Just let me handle telemetry from the surface boat. Please."
                );
                break;

            case 14:
                botaoAvancarFala?.gameObject.SetActive(false);
                painelBotoesEscolha?.SetActive(true);
                break;

            case 15:
                Fala(
                    "Myke",
                    "Oops, a valve slipped! I need to try the calibration again."
                );
                break;

            case 16:
                FecharDialogo();
                IniciarSkillCheck();
                break;
        }
    }


    void Fala(string personagem, string texto)
    {
        personagemAtivo = BuscarPersonagem(personagem);

        if (textoNomePersonagem != null)
        {
            textoNomePersonagem.text =
                personagem == "Narrator"
                ? ""
                : personagem;
        }

        AtualizarPersonagem();

        IniciarDigitacao(texto);
    }


    PersonagemTutorial BuscarPersonagem(string nome)
    {
        foreach (PersonagemTutorial personagem in personagens)
        {
            if (personagem.nome.ToLower() ==
                nome.ToLower())
            {
                return personagem;
            }
        }

        return null;
    }


    void AtualizarPersonagem()
    {
        if (imagemPersonagem == null)
            return;

        if (personagemAtivo == null)
        {
            imagemPersonagem.gameObject.SetActive(false);
            return;
        }

        if (personagemAtivo.spriteBocaFechada != null)
        {
            imagemPersonagem.sprite =
                personagemAtivo.spriteBocaFechada;
        }

        imagemPersonagem.gameObject.SetActive(true);
    }


    void FecharDialogo()
    {
        dialogoAtivo = false;

        painelBalaoFala?.SetActive(false);
        objetoGatinhoUI?.SetActive(false);
        botaoAvancarFala?.gameObject.SetActive(false);

        if (imagemPersonagem != null)
        {
            imagemPersonagem.rectTransform
                .anchoredPosition =
                posicaoPersonagem;
        }
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

        if (capacete &&
            tanque &&
            ferramenta)
        {
            etapa = 4;
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
            etapa = 8;
            MostrarFala();
        }
        else
        {
            etapa = 15;
            MostrarFala();
        }
    }


    public void EscolherPaxton(bool aceitou)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.paxtonAcompanha =
                aceitou;
        }

        SceneManager.LoadScene(
            nomeCenaJogoPrincipal
        );
    }


    void IniciarDigitacao(string texto)
    {
        textoAtual = texto;

        if (digitacao != null)
            StopCoroutine(digitacao);

        digitacao =
            StartCoroutine(Digitar());
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

            if (imagemPersonagem != null &&
                imagemPersonagem.gameObject.activeSelf &&
                char.IsLetterOrDigit(letra))
            {
                if (personagemAtivo != null &&
                    personagemAtivo.spriteBocaAberta != null)
                {
                    imagemPersonagem.sprite =
                        personagemAtivo.spriteBocaAberta;
                }

                StartCoroutine(
                    PularPersonagem()
                );

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

            yield return new WaitForSeconds(
                velocidadeEscrita
            );

            if (imagemPersonagem != null &&
                personagemAtivo != null &&
                personagemAtivo.spriteBocaFechada != null)
            {
                imagemPersonagem.sprite =
                    personagemAtivo.spriteBocaFechada;
            }
        }

        escrevendo = false;

        RestaurarPersonagem();
    }


    IEnumerator PularPersonagem()
    {
        if (imagemPersonagem == null)
            yield break;

        RectTransform rect =
            imagemPersonagem.rectTransform;

        rect.anchoredPosition =
            posicaoPersonagem +
            Vector3.up * alturaPuloLetra;

        yield return new WaitForSeconds(
            velocidadeEscrita * 0.5f
        );

        rect.anchoredPosition =
            posicaoPersonagem;
    }


    void RestaurarPersonagem()
    {
        if (imagemPersonagem == null)
            return;

        imagemPersonagem.rectTransform
            .anchoredPosition =
            posicaoPersonagem;

        if (personagemAtivo != null &&
            personagemAtivo.spriteBocaFechada != null)
        {
            imagemPersonagem.sprite =
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


    public void ExibirLoreObjeto(
        string autor,
        string texto)
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
        {
            textoNomePersonagem.text =
                autor == "Narrator"
                ? ""
                : autor;
        }

        AtualizarPersonagem();

        IniciarDigitacao(texto);
    }
}