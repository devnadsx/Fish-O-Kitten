using UnityEngine;

public class TituloAnimado : MonoBehaviour
{
    public float movimento = 5f;
    public float escala = 0.02f;
    public float velocidade = 2f;

    private RectTransform rectTransform;
    private Vector2 posicaoInicial;
    private Vector3 escalaInicial;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        posicaoInicial = rectTransform.anchoredPosition;
        escalaInicial = rectTransform.localScale;
    }

    void Update()
    {
        float movimentoAtual = Mathf.Sin(Time.time * velocidade) * movimento;

        rectTransform.anchoredPosition =
            posicaoInicial + new Vector2(0, movimentoAtual);

        float escalaAtual =
            1f + Mathf.Sin(Time.time * velocidade) * escala;

        rectTransform.localScale =
            escalaInicial * escalaAtual;
    }
}