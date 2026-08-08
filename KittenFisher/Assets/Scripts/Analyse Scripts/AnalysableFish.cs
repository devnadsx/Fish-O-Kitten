using UnityEngine;

public class AnalysableFish : MonoBehaviour
{
    public string nomeDoPeixe = "Yellow Fish";
    public bool ehVenenoso = true;

    // Função nativa da Unity para cliques em Sprites 2D com Collider2D
    void OnMouseDown()
    {
        Debug.Log($"Clicou no peixe: {nomeDoPeixe}"); // Isso vai mostrar no Console se o clique funcionou!

        if (FishAnalyseManager.Instance != null)
        {
            FishAnalyseManager.Instance.IniciarTesteDoPeixe(nomeDoPeixe, ehVenenoso, gameObject);
        }
        else
        {
            Debug.LogError("FishAnalyseManager não foi encontrado na cena!");
        }
    }
}