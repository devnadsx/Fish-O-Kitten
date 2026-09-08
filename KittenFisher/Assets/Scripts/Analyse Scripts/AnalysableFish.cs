using UnityEngine;
using UnityEngine.EventSystems;

public class AnalysableFish : MonoBehaviour, IPointerClickHandler
{
    public string nomeDoPeixe = "Yellow Fish";
    public bool ehVenenoso = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GerenciadorAnalisePeixes.Instance != null)
        {
            GerenciadorAnalisePeixes.Instance.IniciarAnalise(nomeDoPeixe, ehVenenoso, gameObject);
        }
    }
}