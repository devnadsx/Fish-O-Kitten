using System.Collections;
using System.Collections.Generic;
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

    [Header("Efeito Visão Catnip / Baú")]
    public GameObject auraPeixePrefab; // Arraste o Prefab da Aura no Inspector

    private bool timerAtivo = false;
    private bool estaNaTerceiraFase = false;
    private bool alertaDisparado = false;

    // Guardará as auras criadas para poder destruí-las depois
    private List<GameObject> aurasInstanciadas = new List<GameObject>();

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

    // -----------------------------------------------------------
    // 🐱 SISTEMA DE REVELAR PEIXES (VISÃO CATNIP DO BAÚ)
    // -----------------------------------------------------------

    /// <summary>
    /// Encontra todos os peixes com a Tag "Peixe" ativos no cenário e coloca o prefab de aura neles.
    /// </summary>
    public void RevelarPeixesRestantes()
    {
        aurasInstanciadas.Clear();

        // Procura todos os GameObjects na cena com a Tag "Peixe"
        GameObject[] peixesNaCena = GameObject.FindGameObjectsWithTag("Peixe");

        foreach (GameObject peixe in peixesNaCena)
        {
            if (peixe != null && auraPeixePrefab != null)
            {
                // Instancia o prefab da aura e torna ele FILHO do peixe (para acompanhar se o peixe se mover)
                GameObject aura = Instantiate(auraPeixePrefab, peixe.transform.position, Quaternion.identity, peixe.transform);
                aurasInstanciadas.Add(aura);
            }
        }
    }

    /// <summary>
    /// Remove o efeito de iluminação/aura dos peixes restantes.
    /// </summary>
    public void EsconderAuraPeixes(float tempoFade)
    {
        foreach (GameObject auraObj in aurasInstanciadas)
        {
            if (auraObj != null)
            {
                EfeitoAura auraScript = auraObj.GetComponent<EfeitoAura>();
                if (auraScript != null)
                {
                    auraScript.DestruirComFade(tempoFade);
                }
                else
                {
                    Destroy(auraObj);
                }
            }
        }

        aurasInstanciadas.Clear();
    }
}