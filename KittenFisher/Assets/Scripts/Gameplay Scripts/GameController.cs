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
    //troca cena
    [Header("Troca Direta de Cena")]
    public string nomeProximaCena = "Analyse 1,2,3, ...";
    
    public float tempoEsperaTrocaCena = 1.5f;

    [Header("Efeitos de Vitória")]
    //Particula
    public GameObject prefabParticulaVitoria;

    //Icone
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

        // Ve se quando vc ta na fase 3, ativa o timer quando coleta metade dos peixes
        if (estaNaTerceiraFase && !alertaDisparado)
        {
            int metadeDosPeixes = Mathf.CeilToInt(FishNumber / 2f);

            if (foundedFish >= metadeDosPeixes)
            {
                alertaDisparado = true;
                DispararAlertaOxigenio();
            }
        }

        // CONDIÇÃO DE VITÓRIA / FINALIZAÇÃO DA FASE
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
            Vector3 posSpawn = Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 2f : transform.position;
            Instantiate(prefabParticulaVitoria, posSpawn, Quaternion.identity);
        }

        yield return new WaitForSeconds(tempoEsperaTrocaCena);

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
            scriptDialogoAlerta.falasDoGato.Clear();
            scriptDialogoAlerta.falasDoGato.Add("Meu deus!! Meu oxigênio tá acabando!! Preciso ser rápido.");

            scriptDialogoAlerta.gameObject.SetActive(true);
            scriptDialogoAlerta.IniciarNovoDialogoExterno();

            StartCoroutine(AguardarFimDoDialogoEIniciarTimer());
        }
        else
        {
            IniciarTimerEMusica();
        }
    }
    //Espera o dialogo sumir para tocar o timer
    IEnumerator AguardarFimDoDialogoEIniciarTimer()
    {
        while (scriptDialogoAlerta != null && scriptDialogoAlerta.painelBalaoFala.activeSelf)
        {
            yield return null;
        }

        IniciarTimerEMusica();
    }
    //quando cmc o timer vem a musica tbm
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
    //atuliza conforme o tempo
    void AtualizarTextoTimer()
    {
        if (textoTimerUI != null)
        {
            int segundos = Mathf.CeilToInt(tempoLimite);
            textoTimerUI.text = $"Time: {segundos}s";
        }
    }
    //caso zere, te joga na cena de derrota
    void CarregarGameOver()
    {
        SceneManager.LoadScene(nomeCenaGameOver);
    }

   // Catnip

    //aq ele revela os peixes restantes e spawna os secretos
    public void RevelarPeixesRestantes()
    {
        aurasInstanciadas.Clear();
        GameObject[] peixesNaCena = GameObject.FindGameObjectsWithTag("Peixe");

        foreach (GameObject peixe in peixesNaCena)
        {
            if (peixe != null && auraPeixePrefab != null)
            {
                GameObject aura = Instantiate(auraPeixePrefab, peixe.transform.position, Quaternion.identity, peixe.transform);
                aurasInstanciadas.Add(aura);
            }
        }
    }
    //quando passar o contador/musica, destroi a aura q mostra os peixes
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