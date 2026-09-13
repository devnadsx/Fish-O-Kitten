using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ControleCamera2D : MonoBehaviour
{
    [Header("Limites do Mapa (Transform dos Objetos)")]
    public Transform limiteEsquerda;
    public Transform limiteDireita;
    public Transform limiteCima;
    public Transform limiteBaixo;

    [Header("Configurações de Zoom")]
    public float velocidadeZoom = 5f;
    public float zoomMinimo = 2f;
    public float zoomMaximo = 5f;

    [Header("Sensibilidade de Arraste")]
    public float sensibilidadeArrasta = 1f;

    private Camera cam;
    private Vector3 posicaoInicialMouse;
    private Vector3 posicaoInicialCam;
    private bool estaArrastando = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        cam.orthographicSize = zoomMaximo;
        CentralizarNoMapa();
    }

    void LateUpdate()
    {
        TratarZoom();
        TratarArrasta();
        AplicarLimitesCamera();
    }

    public void CentralizarNoMapa()
    {
        if (limiteEsquerda == null || limiteDireita == null || limiteCima == null || limiteBaixo == null) return;

        float centroX = (limiteEsquerda.position.x + limiteDireita.position.x) / 2f;
        float centroY = (limiteBaixo.position.y + limiteCima.position.y) / 2f;

        transform.position = new Vector3(centroX, centroY, transform.position.z);
    }

    private void TratarZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize -= scroll * velocidadeZoom;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, zoomMinimo, zoomMaximo);
        }
    }

    private void TratarArrasta()
    {
        if (Input.GetMouseButtonDown(0))
        {
            estaArrastando = true;
            posicaoInicialMouse = Input.mousePosition;
            posicaoInicialCam = transform.position;
        }

        if (Input.GetMouseButton(0) && estaArrastando)
        {
            Vector3 deltaMouse = Input.mousePosition - posicaoInicialMouse;

            float alturaMundo = cam.orthographicSize * 2f;
            float conversao = alturaMundo / Screen.height;

            Vector3 deslocamento = new Vector3(-deltaMouse.x, -deltaMouse.y, 0) * conversao * sensibilidadeArrasta;

            transform.position = posicaoInicialCam + deslocamento;
        }

        if (Input.GetMouseButtonUp(0))
        {
            estaArrastando = false;
        }
    }

    private void AplicarLimitesCamera()
    {
        if (limiteEsquerda == null || limiteDireita == null || limiteCima == null || limiteBaixo == null) return;

        // Garante que o minX seja sempre o menor valor e maxX o maior
        float minX = Mathf.Min(limiteEsquerda.position.x, limiteDireita.position.x);
        float maxX = Mathf.Max(limiteEsquerda.position.x, limiteDireita.position.x);
        float minY = Mathf.Min(limiteBaixo.position.y, limiteCima.position.y);
        float maxY = Mathf.Max(limiteBaixo.position.y, limiteCima.position.y);

        float vertExtent = cam.orthographicSize;
        float horizExtent = vertExtent * cam.aspect;

        float camMinX = minX + horizExtent;
        float camMaxX = maxX - horizExtent;
        float camMinY = minY + vertExtent;
        float camMaxY = maxY - vertExtent;

        Vector3 pos = transform.position;

        // Se o tamanho da tela for maior que o próprio mapa, força a ficar centralizado
        pos.x = (camMinX < camMaxX) ? Mathf.Clamp(pos.x, camMinX, camMaxX) : (minX + maxX) / 2f;
        pos.y = (camMinY < camMaxY) ? Mathf.Clamp(pos.y, camMinY, camMaxY) : (minY + maxY) / 2f;

        transform.position = pos;
    }

    // Desenha as caixas de limite na janela Scene para depuração
    private void OnDrawGizmos()
    {
        if (limiteEsquerda == null || limiteDireita == null || limiteCima == null || limiteBaixo == null) return;

        Gizmos.color = Color.red;
        float minX = Mathf.Min(limiteEsquerda.position.x, limiteDireita.position.x);
        float maxX = Mathf.Max(limiteEsquerda.position.x, limiteDireita.position.x);
        float minY = Mathf.Min(limiteBaixo.position.y, limiteCima.position.y);
        float maxY = Mathf.Max(limiteBaixo.position.y, limiteCima.position.y);

        Vector3 centro = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0);
        Vector3 tamanho = new Vector3(maxX - minX, maxY - minY, 1);
        Gizmos.DrawWireCube(centro, tamanho);
    }
}