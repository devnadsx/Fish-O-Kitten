using UnityEngine;

public class ObjetoLore : MonoBehaviour
{
    public string autorOuPersonagem = "Myke";
    [TextArea(2, 4)]
    public string textoLore = "An old photo of Paxton and me when we won the university science fair... feels like ages ago.";
    public AudioSource somClique;

    void OnMouseDown()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.PodeColetarItem())
        {
            if (somClique != null) somClique.Play();
            TutorialManager.Instance.ExibirLoreObjeto(autorOuPersonagem, textoLore);
        }
    }
}