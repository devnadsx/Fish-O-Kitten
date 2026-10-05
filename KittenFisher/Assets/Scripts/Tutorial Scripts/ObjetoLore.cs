using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObjetoLore : MonoBehaviour
{
    public string autorOuPersonagem = "Myke";

    [TextArea(2, 4)]
    public string textoLore =
        "An old photo of Paxton and me when we won the university science fair... feels like ages ago.";

    public AudioSource somClique;

    private bool loreExibida = false;


    void OnMouseDown()
    {
        if (TutorialManager.Instance == null)
            return;

        // Não pode interagir durante diálogos ou Skill Check.
        if (!TutorialManager.Instance.PodeInteragir())
            return;

        // A lore só pode ser vista uma vez.
        if (loreExibida)
            return;

        loreExibida = true;


        if (somClique != null)
            somClique.Play();


        TutorialManager.Instance.ExibirLoreObjeto(
            autorOuPersonagem,
            textoLore
        );
    }
}