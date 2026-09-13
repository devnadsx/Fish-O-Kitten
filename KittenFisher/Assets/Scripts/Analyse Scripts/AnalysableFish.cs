using UnityEngine;
using UnityEngine.EventSystems;

public class AnalysableFish : MonoBehaviour, IPointerClickHandler
{
    public string nomeDoPeixe = "Yellow Fish";
    public bool ehVenenoso = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. Bloqueia o clique se o Skill Check estiver acontecendo
        if (SkillCheckManager.Instance != null && SkillCheckManager.Instance.estaAtivo)
        {
            return;
        }

        // 2. Bloqueia o clique se houver um diálogo ativo
        if (GerenciadorAnalisePeixes.Instance != null && GerenciadorAnalisePeixes.Instance.estaEmDialogo)
        {
            return;
        }

        // Se a tela estiver livre, inicia a análise
        if (GerenciadorAnalisePeixes.Instance != null)
        {
            GerenciadorAnalisePeixes.Instance.IniciarAnalise(nomeDoPeixe, ehVenenoso, gameObject);
        }
    }
}