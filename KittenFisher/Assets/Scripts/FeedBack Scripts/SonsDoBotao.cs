using UnityEngine;
using UnityEngine.EventSystems; // OBRIGATÓRIO para detectar mouse e cliques

public class SonsDoBotao : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Configurações de Áudio")]
    public AudioSource audioSource;
    public AudioClip somHover; // Som ao passar o mouse
    public AudioClip somClick; // Som ao clicar

    // Detecta quando o mouse ENTRA na área do botão
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioSource != null && somHover != null)
        {
            audioSource.PlayOneShot(somHover);
        }
    }

    // Detecta quando o jogador CLICA no botão
    public void OnPointerClick(PointerEventData eventData)
    {
        if (audioSource != null && somClick != null)
        {
            audioSource.PlayOneShot(somClick);
        }
    }
}