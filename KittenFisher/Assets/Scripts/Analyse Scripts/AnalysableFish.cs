using UnityEngine;
using UnityEngine.EventSystems;

public class AnalysableFish : MonoBehaviour, IPointerClickHandler
{
    public string nomeDoPeixe = "Yellow Fish";
    public bool ehVenenoso = true;

    [Tooltip("Multiplicador de velocidade para peixes venenosos")]
    public float multiplicadorVelocidadeVenenoso = 1.5f;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SkillCheckManager.Instance != null && SkillCheckManager.Instance.estaAtivo)
            return;

        if (GerenciadorAnalisePeixes.Instance != null && GerenciadorAnalisePeixes.Instance.estaEmDialogo)
            return;

        if (GerenciadorAnalisePeixes.Instance != null)
        {
            // Determina a velocidade com base na variável ehVenenoso
            float velocidadeFinal = ehVenenoso ? multiplicadorVelocidadeVenenoso : 1.0f;

            GerenciadorAnalisePeixes.Instance.IniciarAnalise(nomeDoPeixe, ehVenenoso, gameObject, velocidadeFinal);
        }
    }
}