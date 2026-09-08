using System.Collections;
using UnityEngine;

public class GerenciadorAnalisePeixes : MonoBehaviour
{
    public static GerenciadorAnalisePeixes Instance;

    [Header("Referência da UI")]
    public MonoBehaviour uiManager;

    [Header("Controle da Mesa")]
    public int totalPeixesNaMesa = 3;
    private int peixesAnalisados = 0;

    private string peixeSendoAnalisado = "";
    private bool peixeAtualEhVenenoso = false;
    private GameObject peixeObjetoAtual;

    void Awake()
    {
        if (Instance == null) Instance = this;
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
        string textoDialogo = peixeAtualEhVenenoso
            ? $"Analysis complete! The {peixeSendoAnalisado} contains dangerous toxins. Do not eat it!"
            : $"Excellent! The {peixeSendoAnalisado} is clean and completely safe to eat.";

        ExibirTextoEProcessarPeixe(textoDialogo);
    }

    public void OnSkillCheckFalha()
    {
        string textoDialogo = $"You messed up the analysis of the {peixeSendoAnalisado}! Bad cut.";
        ExibirTextoEProcessarPeixe(textoDialogo);
    }

    void ExibirTextoEProcessarPeixe(string texto)
    {
        if (uiManager != null)
        {
            uiManager.SendMessage("MostrarFala", texto, SendMessageOptions.DontRequireReceiver);
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
        yield return new WaitForSeconds(3f);

        if (uiManager != null)
        {
            uiManager.SendMessage("MostrarFala", "We analyzed all the fish! Let's move on...", SendMessageOptions.DontRequireReceiver);
            yield return new WaitForSeconds(2f);
            uiManager.SendMessage("FinalizarAnalyseETrocarCena", SendMessageOptions.DontRequireReceiver);
        }
    }
}