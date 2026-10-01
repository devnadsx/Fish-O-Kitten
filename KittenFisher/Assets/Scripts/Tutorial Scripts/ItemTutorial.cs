using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Garante que o objeto tem um Collider 2D para detetar o clique
public class TutorialItem : MonoBehaviour
{
    [Header("Identificação do Item")]
    [Tooltip("Usa exatamente: Capacete, Tanque ou Ferramenta")]
    public string nomeDoItem = "Capacete";

    [Header("Efeitos")]
    public AudioSource audioSourceSFX;
    public AudioClip somAoColetar;
    public GameObject efeitoParticulas; // Opcional: efeito visual ao pegar no item

    private void OnMouseDown()
    {
        // Verifica se o TutorialManager existe e se o jogador pode interagir (sem diálogos abertos)
        if (TutorialManager.Instance != null && TutorialManager.Instance.PodeColetarItem())
        {
            Coletar();
        }
    }

    private void Coletar()
    {
        // Toca o efeito sonoro no local do objeto
        if (somAoColetar != null)
        {
            AudioSource.PlayClipAtPoint(somAoColetar, Camera.main.transform.position);
        }
        else if (audioSourceSFX != null && audioSourceSFX.clip != null)
        {
            audioSourceSFX.Play();
        }

        // Instancia o efeito visual de partículas, se configurado
        if (efeitoParticulas != null)
        {
            Instantiate(efeitoParticulas, transform.position, Quaternion.identity);
        }

        // Informa o TutorialManager sobre a coleta
        TutorialManager.Instance.ColetarItem(nomeDoItem);

        // Desativa ou destrói o objeto da cena
        gameObject.SetActive(false);
    }
}