using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorAnalisePeixes : MonoBehaviour
{
    public static GerenciadorAnalisePeixes Instance;

    [Header("Referência da UI da Análise")]
    public AnalyseUIManager uiManager;

    [Header("Controle da Mesa")]
    public int totalPeixesNaMesa = 3;
    private int peixesAnalisados = 0;

    [Header("Próxima Cena")]
    public string nomeProximaCena = "End";

    private string peixeSendoAnalisado = "";
    private bool peixeAtualEhVenenoso = false;
    private GameObject peixeObjetoAtual;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<AnalyseUIManager>();
        }
    }

    public void IniciarAnalise(string nomePeixe, bool ehVenenoso, GameObject peixeObj)
    {
        peixeSendoAnalisado = nomePeixe;
        peixeAtualEhVenenoso = ehVenenoso;
        peixeObjetoAtual = peixeObj;

        if (SkillCheckManager.Instance != null)
        {
            SkillCheckManager.Instance.IniciarSequenciaSkillCheck(nomePeixe);
        }
    }

    public void OnSkillCheckSucesso()
    {
        List<FalaItem> sequencia = new List<FalaItem>();

        if (peixeAtualEhVenenoso)
        {
            // Reação ao Peixe Venenoso
            sequencia.Add(new FalaItem { texto = $"Phew! Good thing I was careful with this {peixeSendoAnalisado}!", ehNarracao = false });
            sequencia.Add(new FalaItem { texto = $"*Mike carefully marks a red warning symbol in his notebook.*", ehNarracao = true });
            sequencia.Add(new FalaItem { texto = $"It contains dangerous toxins. Completely unsafe to eat!", ehNarracao = false });
        }
        else
        {
            // Reação ao Peixe Seguro
            sequencia.Add(new FalaItem { texto = $"Excellent cut! The {peixeSendoAnalisado} looks pristine.", ehNarracao = false });
            sequencia.Add(new FalaItem { texto = $"*Mike writes down the clean inspection details in his notebook.*", ehNarracao = true });
            sequencia.Add(new FalaItem { texto = $"It's clean and safe for consumption.", ehNarracao = false });
        }

        StartCoroutine(AguardarEProcessar(sequencia));
    }

    public void OnSkillCheckFalha()
    {
        List<FalaItem> sequencia = new List<FalaItem>()
        {
            new FalaItem { texto = $"Ouch! I messed up the cut on the {peixeSendoAnalisado}!", ehNarracao = false },
            new FalaItem { texto = $"*Mike shakes his head in disappointment and crosses out his notes.*", ehNarracao = true },
            new FalaItem { texto = $"Bad slice... I need to stay focused on the next ones.", ehNarracao = false }
        };

        StartCoroutine(AguardarEProcessar(sequencia));
    }

    IEnumerator AguardarEProcessar(List<FalaItem> sequencia)
    {
        yield return new WaitForSeconds(0.1f);

        if (uiManager != null)
        {
            uiManager.IniciarSequenciaDialogo(sequencia);
        }

        if (peixeObjetoAtual != null)
        {
            peixeObjetoAtual.SetActive(false);
        }

        peixesAnalisados++;
        ChecarFimDaCena();
    }

    void ChecarFimDaCena()
    {
        if (peixesAnalisados >= totalPeixesNaMesa)
        {
            StartCoroutine(AguardarETrocarCena());
        }
    }

    IEnumerator AguardarETrocarCena()
    {
        yield return new WaitForSeconds(4f);

        List<FalaItem> sequenciaFinal = new List<FalaItem>()
        {
            new FalaItem { texto = "That's all of them! I've analyzed every fish on the table.", ehNarracao = false },
            new FalaItem { texto = "*Mike closes his notebook and organizes his desk.*", ehNarracao = true },
            new FalaItem { texto = "Time to move on to the next step!", ehNarracao = false }
        };

        if (uiManager != null)
        {
            uiManager.IniciarSequenciaDialogo(sequenciaFinal);
            yield return new WaitForSeconds(4.5f);
            uiManager.FinalizarAnalyseETrocarCena();
        }
    }
}