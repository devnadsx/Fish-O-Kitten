using UnityEngine;

public class ArrastarCenario : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float sensibilidade = 1.0f;
    private Vector3 ultimaPosicaoMouseTela;
    private bool estaArrastando = false;

    [Header("Limites do Cenário (Base)")]
    public bool usarLimites = true;

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

    private float zoomOriginal;

    void Start()
    {
        zoomOriginal = zoomMaximo;

        // Garante que a câmera comece no zoom máximo configurado
        if (Camera.main != null && Camera.main.orthographic)
        {
            Camera.main.orthographicSize = zoomMaximo;
        }
    }

    void Update()
    {
        if (Camera.main == null || !Camera.main.orthographic) return;

        // ==========================================
        // 1. SISTEMA DE ZOOM (Scroll do Mouse)
        // ==========================================
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0.0f)
        {
            Camera.main.orthographicSize -= scroll * velocidadeZoom;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, zoomMinimo, zoomMaximo);

            // Re-aplica a trava de limites no frame do zoom para evitar que o cenário fique fora da tela
            AplicarLimites();
        }

        // ==========================================
        // 2. SISTEMA DE ARRASTAR (Botão Esquerdo)
        // ==========================================
        if (Input.GetMouseButtonDown(0))
        {
            estaArrastando = true;
            ultimaPosicaoMouseTela = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && estaArrastando)
        {
            Vector3 deltaMouseTela = Input.mousePosition - ultimaPosicaoMouseTela;

            if (deltaMouseTela.sqrMagnitude > 0.001f)
            {
                // Converte a variação em pixels para unidades do mundo proporcional ao zoom atual
                float alturaCameraMundo = Camera.main.orthographicSize * 2f;
                float conversaoPixelParaMundo = alturaCameraMundo / Screen.height;

                Vector3 deslocamentoMundo = deltaMouseTela * conversaoPixelParaMundo * sensibilidade;
                deslocamentoMundo.z = 0;

                transform.position += deslocamentoMundo;

                AplicarLimites();
            }

            ultimaPosicaoMouseTela = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            estaArrastando = false;
        }
    }

    // ==========================================
    // CÁLCULO DINÂMICO DE LIMITES
    // ==========================================
    private void AplicarLimites()
    {
        if (!usarLimites) return;

        // Calcula a folga extra permitida pelo zoom
        float fatorZoom = zoomOriginal - Camera.main.orthographicSize;

        // Limites expandidos proporcionalmente conforme você aproxima a câmera
        float limiteEsq = limiteEsquerdaBase - (fatorZoom * 2f);
        float limiteDir = limiteDireitaBase + (fatorZoom * 2f);
        float limiteBaixo = limiteBaixoBase - (fatorZoom * 2f);
        float limiteCima = limiteCimaBase + (fatorZoom * 2f);

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, limiteEsq, limiteDir);
        pos.y = Mathf.Clamp(pos.y, limiteBaixo, limiteCima);

        transform.position = pos;
    }
}