using UnityEngine;

public class MenuParallax : MonoBehaviour
{
    public float movimento = 15f;
    public float rotacao = 2f;
    public float suavidade = 5f;

    private RectTransform rectTransform;
    private Vector2 posicaoInicial;
    private Quaternion rotacaoInicial;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        posicaoInicial = rectTransform.anchoredPosition;
        rotacaoInicial = rectTransform.localRotation;
    }

    void Update()
    {
        // Pega a posição do mouse
        Vector2 mouse = Input.mousePosition;

        // Converte para valores de -1 até 1
        float x = (mouse.x / Screen.width) * 2f - 1f;
        float y = (mouse.y / Screen.height) * 2f - 1f;

        // Movimento desejado
        Vector2 movimentoAlvo = new Vector2(
            -x * movimento,
            -y * movimento
        );

        // Rotação desejada
        float rotacaoAlvo = -x * rotacao;

        // Movimento suave
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            posicaoInicial + movimentoAlvo,
            Time.deltaTime * suavidade
        );

        // Rotação suave
        Quaternion novaRotacao = Quaternion.Euler(
            0,
            0,
            rotacaoAlvo
        );

        rectTransform.localRotation = Quaternion.Lerp(
            rectTransform.localRotation,
            rotacaoInicial * novaRotacao,
            Time.deltaTime * suavidade
        );
    }
}