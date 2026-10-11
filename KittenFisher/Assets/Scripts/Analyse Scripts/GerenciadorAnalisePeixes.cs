
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [HideInInspector]
    public bool estaEmDialogo = false;

    private string peixeSendoAnalisado = "";
    private bool peixeAtualEhVenenoso = false;
    private GameObject peixeObjetoAtual;

    private bool finalizacaoIniciada = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (uiManager == null)
            uiManager = FindFirstObjectByType<AnalyseUIManager>();
    }

    public void IniciarAnalise(
        string nomePeixe,
        bool ehVenenoso,
        GameObject peixeObj,
        float multiplicadorVelocidade = 1.0f)
    {
        if (estaEmDialogo || finalizacaoIniciada)
            return;

        peixeSendoAnalisado = nomePeixe;
        peixeAtualEhVenenoso = ehVenenoso;
        peixeObjetoAtual = peixeObj;

        if (SkillCheckManager.Instance != null)
        {
            SkillCheckManager.Instance.IniciarSequenciaSkillCheck(
                nomePeixe,
                multiplicadorVelocidade);
        }
    }

    public void OnSkillCheckSucesso()
    {
        List<FalaItem> sequencia = new List<FalaItem>();

        if (peixeAtualEhVenenoso)
        {
            sequencia.Add(new FalaItem
            {
                texto = $"Eita! Ainda bem que tomei cuidado porque esse {peixeSendoAnalisado} é venenoso!",
                ehNarracao = false
            });

            sequencia.Add(new FalaItem
            {
                texto = "*Myke se acalma, e logo em seguida, escreve os detalhes do peixe, deixando bem claro suas substâncias*",
                ehNarracao = true
            });

            sequencia.Add(new FalaItem
            {
                texto = "Tá bom, tem substâncias tóxicas. Extremamente perigosas se consumidas sem tratamento!",
                ehNarracao = false
            });
        }
        else
        {
            sequencia.Add(new FalaItem
            {
                texto = $"Aee! O {peixeSendoAnalisado} é seguro.",
                ehNarracao = false
            });

            sequencia.Add(new FalaItem
            {
                texto = "*Myke escreve contente no caderno, anotando seus detalhes.*",
                ehNarracao = true
            });

            sequencia.Add(new FalaItem
            {
                texto = "É totalmente seguro e perfeito para consumo.",
                ehNarracao = false
            });
        }

        StartCoroutine(AguardarEProcessar(sequencia));
    }

    public void OnSkillCheckFalha()
    {
        List<FalaItem> sequencia = new List<FalaItem>()
        {
            new FalaItem
            {
                texto = $"Oww!! Não consegui investigar adequadamente o {peixeSendoAnalisado}!",
                ehNarracao = false
            },
            new FalaItem
            {
                texto = "*Myke, frustrado, traça as informações do peixe, porque não possui dados verdadeiros.*",
                ehNarracao = true
            },
            new FalaItem
            {
                texto = "Que droga.., preciso tomar mais cuidado, se não isso irá influenciar no relatório.",
                ehNarracao = false
            }
        };

        StartCoroutine(AguardarEProcessar(sequencia));
    }

    IEnumerator AguardarEProcessar(List<FalaItem> sequencia)
    {
        yield return new WaitForSeconds(0.1f);

        if (uiManager == null)
        {
            Debug.LogError("AnalyseUIManager não foi encontrado!");
            yield break;
        }

        estaEmDialogo = true;

        if (peixeObjetoAtual != null)
            peixeObjetoAtual.SetActive(false);

        uiManager.IniciarSequenciaDialogo(
            sequencia,
            ConcluirAnalisePeixe);
    }

    private void ConcluirAnalisePeixe()
    {
        peixesAnalisados++;

        peixeObjetoAtual = null;

        ChecarFimDaCena();
    }

    public void FinalizarDialogo()
    {
        estaEmDialogo = false;
    }

    private void ChecarFimDaCena()
    {
        if (peixesAnalisados >= totalPeixesNaMesa &&
            !finalizacaoIniciada)
        {
            finalizacaoIniciada = true;
            estaEmDialogo = true;
            StartCoroutine(AguardarETrocarCena());
        }
    }

    IEnumerator AguardarETrocarCena()
    {
        yield return new WaitForSeconds(0.5f);

        if (uiManager == null)
        {
            Debug.LogError("AnalyseUIManager não foi encontrado!");
            yield break;
        }

        List<FalaItem> sequenciaFinal = new List<FalaItem>()
        {
            new FalaItem
            {
                texto = "Fechou! Analisei todos os peixes, agora posso ir para a próxima etapa.",
                ehNarracao = false
            },
            new FalaItem
            {
                texto = "*Myke fecha o caderno e organiza suas coisas.*",
                ehNarracao = true
            },
            new FalaItem
            {
                texto = "Ok, hora de ir para a próxima etapa!",
                ehNarracao = false
            }
        };

        uiManager.IniciarSequenciaDialogo(
            sequenciaFinal,
            TrocarParaFase3);
    }

    private void TrocarParaFase3()
    {
        if (uiManager != null)
            uiManager.FinalizarAnalyseETrocarCena();
    }
}
