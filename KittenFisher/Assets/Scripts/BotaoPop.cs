using UnityEngine;
using UnityEngine.EventSystems;

public class BotaoPop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float aumento = 1.08f;
    public float inclinacao = 2f;
    public float velocidade = 12f;

    private RectTransform rectTransform;

    private Vector3 escalaOriginal;
    private Quaternion rotacaoOriginal;

    private Vector3 escalaAlvo;
    private Quaternion rotacaoAlvo;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        escalaOriginal = rectTransform.localScale;
        rotacaoOriginal = rectTransform.localRotation;

        escalaAlvo = escalaOriginal;
        rotacaoAlvo = rotacaoOriginal;
    }

    void Update()
    {
        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            escalaAlvo,
            Time.deltaTime * velocidade
        );

        rectTransform.localRotation = Quaternion.Lerp(
            rectTransform.localRotation,
            rotacaoAlvo,
            Time.deltaTime * velocidade
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cresce mantendo o tamanho original
        escalaAlvo = escalaOriginal * aumento;

        // Dá uma leve inclinada
        rotacaoAlvo = rotacaoOriginal * Quaternion.Euler(0, 0, inclinacao);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Volta ao tamanho original
        escalaAlvo = escalaOriginal;

        // Volta à rotação original
        rotacaoAlvo = rotacaoOriginal;
    }
}