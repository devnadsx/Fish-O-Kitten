
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObjetoLore : MonoBehaviour
{
    public string autorOuPersonagem = "Myke";

    [TextArea(2, 4)]
    public string textoLore =
        "An old photo of Paxton and me when we won the university science fair... feels like ages ago.";

    public AudioSource somClique;

    [Header("Verificação de Objetos")]
    public LayerMask camadaBloqueio;
    public float distanciaVerificacao = 10f;

    private bool loreExibida = false;
    private Collider2D meuColisor;

    void Start()
    {
        meuColisor = GetComponent<Collider2D>();
    }

    void OnMouseDown()
    {
        if (TutorialManager.Instance == null)
            return;

        if (!TutorialManager.Instance.PodeInteragir())
            return;

        if (loreExibida)
            return;

        if (TemObjetoNaFrente())
            return;

        loreExibida = true;

        if (somClique != null)
            somClique.Play();

        TutorialManager.Instance.ExibirLoreObjeto(
            autorOuPersonagem,
            textoLore
        );
    }

    private bool TemObjetoNaFrente()
    {
        Vector2 direcao = Camera.main.transform.forward;

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            direcao,
            distanciaVerificacao,
            camadaBloqueio
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != meuColisor && hit.collider != null)
            {
                return true;
            }
        }

        return false;
    }
}
