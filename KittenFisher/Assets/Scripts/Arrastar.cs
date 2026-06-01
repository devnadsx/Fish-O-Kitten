using UnityEngine;


public class ArrastarCenario : MonoBehaviour
{
    private Vector3 mousePosicaoAnterior;
    private bool estaArrastando = false;

    // Sensibilidade opcional para ajustes finos
    [SerializeField] private float sensibilidade = 1.0f;


    // Função auxiliar para converter a posição do mouse de pixels para coordenadas do mundo
    private Vector3 ObterPosicaoMouseNoMundo()
    {
        Vector3 mousePos = Input.mousePosition;
        // Importante: a distância Z deve ser a distância da câmera até o objeto
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            estaArrastando = true;
            // Armazena a posição do mouse no mundo no momento do clique
            mousePosicaoAnterior = ObterPosicaoMouseNoMundo();
        }
        if (Input.GetMouseButton(0))
        {
            if (estaArrastando)
            {
                Vector3 mousePosicaoAtual = ObterPosicaoMouseNoMundo();

                // Calcula o quanto o mouse se moveu desde o último frame
                Vector3 diferenca = mousePosicaoAtual - mousePosicaoAnterior;

                // Remove qualquer movimento no eixo Z
                diferenca.z = 0;

                // Aplica o movimento ao objeto (multiplicado pela sensibilidade se desejar)
                transform.position += diferenca * sensibilidade;

                // ATUALIZA a posição anterior para o próximo frame
                // Sem isso, o objeto "teletransporta" de volta para o clique inicial
                mousePosicaoAnterior = mousePosicaoAtual;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            estaArrastando = false;
        }
    }
}
