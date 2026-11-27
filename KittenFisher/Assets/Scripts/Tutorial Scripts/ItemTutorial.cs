
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemTutorial : MonoBehaviour
{
    [Header("Identificação do Item")]
    [Tooltip("Usa exatamente: Capacete, Tanque ou Ferramenta")]
    public string nomeDoItem = "Capacete";

    [Header("Verificação de Objetos")]
    public LayerMask camadaBloqueio;
    public float distanciaVerificacao = 10f;

    [Header("Efeitos")]
    public AudioSource audioSourceSFX;
    public AudioClip somAoColetar;
    public GameObject efeitoParticulas;

    private Collider2D meuColisor;

    private void Start()
    {
        meuColisor = GetComponent<Collider2D>();
    }

    private void OnMouseDown()
    {
        if (TutorialManager.Instance == null) return;
        if (!TutorialManager.Instance.PodeColetarItem()) return;

        if (TemObjetoNaFrente()) return;

        Coletar();
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

    private void Coletar()
    {
        if (somAoColetar != null)
        {
            AudioSource.PlayClipAtPoint(
                somAoColetar,
                Camera.main.transform.position
            );
        }
        else if (audioSourceSFX != null && audioSourceSFX.clip != null)
        {
            audioSourceSFX.Play();
        }

        if (efeitoParticulas != null)
        {
            Instantiate(efeitoParticulas, transform.position, Quaternion.identity);
        }

        TutorialManager.Instance.ColetarItem(nomeDoItem);

        gameObject.SetActive(false);
    }
}
