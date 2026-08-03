using UnityEngine;

public class SkillCheckManager : MonoBehaviour
{
    [Header("UI Elementos (Barra Horizontal)")]
    public GameObject painelSkillCheck;
    public RectTransform barraFundo;     // A barra preta
    public RectTransform zonaDeAcerto;   // A barra branca
    public RectTransform ponteiro;       // A linha vermelha

    [Header("Configurações")]
    public float velocidade = 600f;
    public int peixesPerdidosAoFalhar = 2;

    private bool movendoParaDireita = true;
    private bool jogoAtivo = false;
    private int errosCometidos = 0;

    private float limiteEsquerda;
    private float limiteDireita;

    [Header("Gerenciadores")]
    public IconManager iconManager;
    public GameController gameController;
    public FishRespawnManager respawnManager;

    void Start()
    {
        // Calcula os limites da barra no início
        AtualizarLimites();
    }

    void AtualizarLimites()
    {
        if (barraFundo != null)
        {
            float larguraBarra = barraFundo.rect.width;
            limiteEsquerda = -larguraBarra / 2f;
            limiteDireita = larguraBarra / 2f;
        }
    }

    void Update()
    {
        if (!jogoAtivo) return;

        // 1. Move o ponteiro para esquerda e direita
        float deslocamento = velocidade * Time.deltaTime;

        if (movendoParaDireita)
        {
            ponteiro.anchoredPosition += new Vector2(deslocamento, 0);
            if (ponteiro.anchoredPosition.x >= limiteDireita)
                movendoParaDireita = false;
        }
        else
        {
            ponteiro.anchoredPosition -= new Vector2(deslocamento, 0);
            if (ponteiro.anchoredPosition.x <= limiteEsquerda)
                movendoParaDireita = true;
        }

        // 2. Detecta o clique no Espaço
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ValidarClique();
        }
    }

    // Função para iniciar o Skill Check (pode ser chamada por peixes estragados, baús, etc)
    public void IniciarSequenciaSkillCheck()
    {
        errosCometidos = 0;
        ProximoSkillCheck();
    }

    void ProximoSkillCheck()
    {
        AtualizarLimites();
        painelSkillCheck.SetActive(true);
        jogoAtivo = true;

        // Reseta o ponteiro para a esquerda
        ponteiro.anchoredPosition = new Vector2(limiteEsquerda, ponteiro.anchoredPosition.y);
        movendoParaDireita = true;

        // Sortear a barra branca em um lugar aleatório dentro da barra preta
        float metadeZona = zonaDeAcerto.rect.width / 2f;
        float xAleatorio = Random.Range(limiteEsquerda + metadeZona, limiteDireita - metadeZona);
        zonaDeAcerto.anchoredPosition = new Vector2(xAleatorio, zonaDeAcerto.anchoredPosition.y);
    }

    void ValidarClique()
    {
        jogoAtivo = false;

        float posPonteiroX = ponteiro.anchoredPosition.x;
        float zonaInicioX = zonaDeAcerto.anchoredPosition.x - (zonaDeAcerto.rect.width / 2f);
        float zonaFimX = zonaDeAcerto.anchoredPosition.x + (zonaDeAcerto.rect.width / 2f);

        // Verifica se o ponteiro X está dentro da área X da barra branca
        if (posPonteiroX >= zonaInicioX && posPonteiroX <= zonaFimX)
        {
            Debug.Log("🎯 ACERTOU O SKILL CHECK!");
            painelSkillCheck.SetActive(false);

            if (iconManager != null)
                iconManager.MudarParaNormal();
        }
        else
        {
            Debug.Log("❌ ERROU O SKILL CHECK!");
            errosCometidos++;

            if (errosCometidos >= 3)
            {
                FinalizarComDerrota();
            }
            else
            {
                ProximoSkillCheck();
            }
        }
    }

    void FinalizarComDerrota()
    {
        jogoAtivo = false;
        painelSkillCheck.SetActive(false);
        Debug.LogError("Você falhou 3 vezes no Skill Check!");

        if (iconManager != null)
            iconManager.MudarParaNormal();

        int perdidos = Mathf.Min(peixesPerdidosAoFalhar, InventoryManager.Instance.peixesNoInventario);
        InventoryManager.Instance.peixesNoInventario -= perdidos;

        if (gameController != null)
        {
            gameController.foundedFish -= perdidos;
            if (gameController.foundedFish < 0) gameController.foundedFish = 0;
        }

        if (respawnManager != null)
        {
            respawnManager.SpawnarPeixesEscondidos(perdidos);
        }
    }
}