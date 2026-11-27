
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    public int foundedFish;
    public int FishNumber = 3;

    [Header("Troca Direta de Cena")]
    public string nomeProximaCena = "Analyse 1,2,3, ...";
    public float tempoEsperaTrocaCena = 1.5f;

    [Header("Efeitos de Vitória")]
    public GameObject prefabParticulaVitoria;
    public IconManager iconManager;

    [Header("Efeitos Sonoros")]
    public AudioSource audioSource;
    public AudioClip somColetaPeixe;

    [Header("Música de Tensão (Fase 3)")]
    public AudioSource audioSourceMusicaFundo;
    public AudioClip musicaTensa;

    [Header("Configurações da Fase 3 (Timer)")]
    public string nomeCenaFase3 = "Game3";
    public string nomeCenaGameOver = "GameOver";
    public TextMeshProUGUI textoTimerUI;
    public float tempoLimite = 60f;

    [Header("Diálogo de Alerta de Oxigênio")]
    public SceneIntroDialogue scriptDialogoAlerta;

    [Header("Diálogo após coletar todos os peixes")]
    public SceneIntroDialogue scriptDialogoFinal;

    [Header("Paxton")]
    public GameObject paxton;
    public bool ocultarPaxtonDuranteDialogo = false;

    [Header("Efeito Visão Catnip / Baú")]
    public GameObject auraPeixePrefab;

    private bool timerAtivo = false;
    private bool estaNaTerceiraFase = false;
    private bool alertaDisparado = false;
    private bool jogoFinalizado = false;

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
        if (jogoFinalizado) return;

        foundedFish += 1;

        if (audioSource != null && somColetaPeixe != null)
        {
            audioSource.PlayOneShot(somColetaPeixe);
        }

        if (iconManager != null)
        {
            iconManager.MudarParaFeliz();
        }

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
            jogoFinalizado = true;
            timerAtivo = false;

            if (textoTimerUI != null)
                textoTimerUI.gameObject.SetActive(false);

            StartCoroutine(ExecutarEfeitosETrocarCena());
        }
    }

    IEnumerator ExecutarEfeitosETrocarCena()
    {
        if (prefabParticulaVitoria != null)
        {
            Vector3 posSpawn = Camera.main != null
                ? Camera.main.transform.position + Camera.main.transform.forward * 2f
                : transform.position;

            Instantiate(prefabParticulaVitoria, posSpawn, Quaternion.identity);
        }

        yield return new WaitForSeconds(tempoEsperaTrocaCena);

        if (scriptDialogoFinal != null)
        {
            scriptDialogoFinal.gameObject.SetActive(true);

            OcultarPaxton();

            scriptDialogoFinal.IniciarDialogoComFalas(
                new List<string>
                {
                    "Ufa... consegui encontrar todos os peixes!",
                    "Agora preciso organizar minhas descobertas e voltar para apresentar os resultados."
                }
            );

            while (scriptDialogoFinal != null &&
                   scriptDialogoFinal.painelBalaoFala != null &&
                   scriptDialogoFinal.painelBalaoFala.activeSelf)
            {
                yield return null;
            }

            MostrarPaxton();
        }

        if (!string.IsNullOrEmpty(nomeProximaCena))
        {
            SceneManager.LoadScene(nomeProximaCena);
        }
    }

    void DispararAlertaOxigenio()
    {
        if (textoTimerUI != null)
        {
            textoTimerUI.gameObject.SetActive(true);
            AtualizarTextoTimer();
        }

        if (scriptDialogoAlerta != null)
        {
            scriptDialogoAlerta.gameObject.SetActive(true);

            OcultarPaxton();

            scriptDialogoAlerta.IniciarDialogoComFalas(
                new List<string>
                {
                    "Meu deus!! Meu oxigênio tá acabando!! Preciso ser rápido."
                }
            );

            StartCoroutine(AguardarFimDoDialogoEIniciarTimer());
        }
        else
        {
            IniciarTimerEMusica();
        }
    }

    IEnumerator AguardarFimDoDialogoEIniciarTimer()
    {
        while (scriptDialogoAlerta != null &&
               scriptDialogoAlerta.painelBalaoFala != null &&
               scriptDialogoAlerta.painelBalaoFala.activeSelf)
        {
            yield return null;
        }

        MostrarPaxton();
        IniciarTimerEMusica();
    }

    void OcultarPaxton()
    {
        if (ocultarPaxtonDuranteDialogo && paxton != null)
        {
            paxton.SetActive(false);
        }
    }

    void MostrarPaxton()
    {
        if (ocultarPaxtonDuranteDialogo && paxton != null)
        {
            paxton.SetActive(true);
        }
    }

    void IniciarTimerEMusica()
    {
        timerAtivo = true;

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

    // Catnip

    public void RevelarPeixesRestantes()
    {
        aurasInstanciadas.Clear();
        GameObject[] peixesNaCena = GameObject.FindGameObjectsWithTag("Peixe");

        foreach (GameObject peixe in peixesNaCena)
        {
            if (peixe != null && auraPeixePrefab != null)
            {
                GameObject aura = Instantiate(
                    auraPeixePrefab,
                    peixe.transform.position,
                    Quaternion.identity,
                    peixe.transform
                );

                aurasInstanciadas.Add(aura);
            }
        }
    }

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
