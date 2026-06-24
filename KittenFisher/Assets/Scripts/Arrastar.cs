using UnityEngine;

public class ArrastarCenario : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float sensibilidade = 1.0f;
    private Vector3 mousePosicaoAnterior;
    private bool estaArrastando = false;

    [Header("Limites do Cenário (Dinâmicos)")]
    public bool usarLimites = true;

    // Esses valores agora serão a BASE (com zoom padrão). O código ajustará o resto!
    [Tooltip("O X mínimo quando o zoom está no máximo afastado")]
    public float limiteEsquerdaBase = -37f;
    [Tooltip("O X máximo quando o zoom está no máximo afastado")]
    public float limiteDireitaBase = -6f;
    [Tooltip("O Y mínimo quando o zoom está no máximo afastado")]
    public float limiteBaixoBase = -5f;
    [Tooltip("O Y máximo quando o zoom está no máximo afastado")]
    public float limiteCimaBase = 5f;

    [Header("Configurações de Zoom (Scroll)")]
    [SerializeField] private float velocidadeZoom = 6f;
    [SerializeField] private float zoomMinimo = 1f;
    [SerializeField] private float zoomMaximo = 5f;

    // Guarda qual era o tamanho inicial do zoom para fazer a regra de três matemática
    private float zoomOriginal;

    private Vector3 ObterPosicaoMouseNoMundo()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    void Start()
    {
        // Salva o valor máximo do zoom (totalmente afastado) como nossa base de cálculo
        zoomOriginal = zoomMaximo;
    }

    void Update()
    {
        // ==========================================
        // SISTEMA DE ZOOM (Scroll do Mouse)
        // ==========================================
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0.0f)
        {
            if (Camera.main.orthographic)
            {
                Camera.main.orthographicSize -= scroll * velocidadeZoom;
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, zoomMinimo, zoomMaximo);
            }
        }

        // ==========================================
        // SISTEMA DE ARRASTAR (Botão esquerdo)
        // ==========================================
        if (Input.GetMouseButtonDown(0))
        {
            estaArrastando = true;
            mousePosicaoAnterior = ObterPosicaoMouseNoMundo();
        }

        if (Input.GetMouseButton(0) && estaArrastando)
        {
            Vector3 mousePosicaoAtual = ObterPosicaoMouseNoMundo();
            Vector3 diferenca = mousePosicaoAtual - mousePosicaoAnterior;
            diferenca.z = 0;

            // Aplica o movimento ao objeto
            transform.position += diferenca * sensibilidade;

            // TRAVA OS LIMITES DE FORMA DINÂMICA
            if (usarLimites && Camera.main.orthographic)
            {
                Vector3 posicaoTravada = transform.position;

                // Descobre o fator de modificação (quanto mais perto, maior o fator multiplicador)
                // Se a câmera aproximou, precisamos permitir que o objeto se mova MAIS para os lados
                float proporcaoZoom = zoomOriginal / Camera.main.orthographicSize;

                // Ajusta dinamicamente os limites baseando-se no zoom atual
                float limiteEsqAtual = limiteEsquerdaBase * proporcaoZoom;
                float limiteDirAtual = limiteDireitaBase / proporcaoZoom;
                float limiteBaixoAtual = limiteBaixoBase * proporcaoZoom;
                float limiteCimaAtual = limiteCimaBase * proporcaoZoom;

                // Garante que o limite direita não inverta lógica estranha
                if (limiteEsqAtual > limiteDireitaBase) limiteEsqAtual = limiteEsquerdaBase;

                // Tranca usando os limites recalculados para o frame atual
                posicaoTravada.x = Mathf.Clamp(posicaoTravada.x, limiteEsquerdaBase * proporcaoZoom, limiteDireitaBase * (Camera.main.orthographicSize / zoomOriginal));
                posicaoTravada.y = Mathf.Clamp(posicaoTravada.y, limiteBaixoBase * proporcaoZoom, limiteCimaBase * proporcaoZoom);

                // Correção direta para o seu caso específico de limites assimétricos (-37 a -6)
                float compensacao = (zoomOriginal - Camera.main.orthographicSize) * 4f;
                posicaoTravada.x = Mathf.Clamp(posicaoTravada.x, limiteEsquerdaBase - compensacao, limiteDireitaBase + compensacao);
                posicaoTravada.y = Mathf.Clamp(posicaoTravada.y, limiteBaixoBase - compensacao, limiteCimaBase + compensacao);

                transform.position = posicaoTravada;
            }

            mousePosicaoAnterior = ObterPosicaoMouseNoMundo();
        }

        if (Input.GetMouseButtonUp(0))
        {
            estaArrastando = false;
        }
    }
}