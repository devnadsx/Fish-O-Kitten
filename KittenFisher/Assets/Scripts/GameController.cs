using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    public int foundedFish;
    [Tooltip("Defina a quantidade de peixes necessários no Inspector")]
    public int FishNumber = 3;
    public UnityEvent OnVictory;

    [Header("Gerenciador do Gatinho")]
    public IconManager iconManager;

    [Header("Efeitos Sonoros")]
    public AudioSource audioSource;
    public AudioClip somColetaPeixe;

    [Header("Música de Tensão (Fase 3)")]
    public AudioSource audioSourceMusicaFundo; // Componente que toca a música principal
    public AudioClip musicaTensa;              // Troca para essa música ao iniciar o timer

    [Header("Configurações da Fase 3 (Timer)")]
    public string nomeCenaFase3 = "Game3";
    public string nomeCenaGameOver = "GameOver";
    public TextMeshProUGUI textoTimerUI;
    public float tempoLimite = 60f;

    [Header("Diálogo de Alerta de Oxigênio")]
    public SceneIntroDialogue scriptDialogoAlerta; // Arraste o componente de diálogo aqui

    private bool timerAtivo = false;
    private bool estaNaTerceiraFase = false;
    private bool alertaDisparado = false;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == nomeCenaFase3)
        {
            estaNaTerceiraFase = true;
        }

        if (textoTimerUI != null)
        {
            textoTimerUI.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (timerAtivo)
        {
            if (tempoLimite > 0)
            {
                tempoLimite -= Time.deltaTime;
                AtualizarTextoTimer();
            }
            else
            {
                tempoLimite = 0;
                timerAtivo = false;
                AtualizarTextoTimer();
                CarregarGameOver();
            }
        }
    }

    public void FoundFish()
    {
        foundedFish += 1;

        if (audioSource != null && somColetaPeixe != null)
        {
            audioSource.PlayOneShot(somColetaPeixe);
        }

        if (iconManager != null)
        {
            iconManager.MudarParaFeliz();
        }

        // Checa gatilho de metade dos peixes na Fase 3
        if (estaNaTerceiraFase && !alertaDisparado)
        {
            int metadeDosPeixes = Mathf.CeilToInt(FishNumber / 2f);

            if (foundedFish >= metadeDosPeixes)
            {
                alertaDisparado = true;
                DispararAlertaOxigenio();
            }
        }

        if (foundedFish >= FishNumber)
        {
            timerAtivo = false;
            if (textoTimerUI != null) textoTimerUI.gameObject.SetActive(false);

            OnVictory.Invoke();
        }
    }

    void DispararAlertaOxigenio()
    {
        // 1. Exibe o Timer travado no valor total (ex: 60s)
        if (textoTimerUI != null)
        {
            textoTimerUI.gameObject.SetActive(true);
            AtualizarTextoTimer();
        }

        // 2. Chama o diálogo do gato
        if (scriptDialogoAlerta != null)
        {
            // Limpa falas antigas e insere a fala de emergência
            scriptDialogoAlerta.falasDoGato.Clear();
            scriptDialogoAlerta.falasDoGato.Add("Oh no! My oxygen tank is running low, I need to hurry up!");

            // Inicia o diálogo na tela
            scriptDialogoAlerta.gameObject.SetActive(true);
            scriptDialogoAlerta.IniciarNovoDialogoExterno();

            // Inicia Coroutine que espera a fala fechar para rodar o timer e a música
            StartCoroutine(AguardarFimDoDialogoEIniciarTimer());
        }
        else
        {
            // Caso não tenha script de diálogo atribuído, inicia o timer direto
            IniciarTimerEMusica();
        }
    }

    IEnumerator AguardarFimDoDialogoEIniciarTimer()
    {
        // Aguarda enquanto a janela de diálogo estiver visível/ativa
        while (scriptDialogoAlerta != null && scriptDialogoAlerta.painelBalaoFala.activeSelf)
        {
            yield return null;
        }

        // Começa a contagem e troca a música assim que o jogador fechar o balão de fala
        IniciarTimerEMusica();
    }

    void IniciarTimerEMusica()
    {
        timerAtivo = true;

        // Troca a trilha sonora para a versão tensa
        if (audioSourceMusicaFundo != null && musicaTensa != null)
        {
            audioSourceMusicaFundo.Stop();
            audioSourceMusicaFundo.clip = musicaTensa;
            audioSourceMusicaFundo.Play();
        }
    }

    void AtualizarTextoTimer()
    {
        if (textoTimerUI != null)
        {
            int segundos = Mathf.CeilToInt(tempoLimite);
            textoTimerUI.text = $"Time: {segundos}s";
        }
    }

    void CarregarGameOver()
    {
        SceneManager.LoadScene(nomeCenaGameOver);
    }
}