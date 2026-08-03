using System.Collections;
using UnityEngine;

public class SkillCheckManager : MonoBehaviour
{
    [Header("UI do Skill Check")]
    public GameObject painelSkillCheck;
    public RectTransform ponteiro;
    public RectTransform zonaDeAcerto;

    [Header("Configurações")]
    public float velocidadRotacao = 250f;

    private int errosCometidos = 0;
    private bool jogoAtivo = false;
    private float anguloAlvo;
    private float margemDeAcerto = 20f; // Tamanho da zona (em graus)

    [Header("Punição")]
    public int peixesPerdidosAoFalhar = 2;
    public GameController gameController;
    public FishRespawnManager respawnManager;

    void Update()
    {
        if (!jogoAtivo) return;

        // Faz o ponteiro rodar no sentido horário
        ponteiro.Rotate(0, 0, -velocidadRotacao * Time.deltaTime);

        // Se o jogador apertar Espaço
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ValidarClique();
        }

        // Se der uma volta completa e o jogador não apertar nada, conta como erro automático
        if (ponteiro.localEulerAngles.z > 358f || (ponteiro.localEulerAngles.z < 2f && velocidadRotacao > 0 && ponteiro.localEulerAngles.z != 0))
        {
            // Opcional: Adicionar lógica de auto-falha se passar direto da zona
        }
    }

    public void IniciarSequenciaSkillCheck()
    {
        errosCometidos = 0;
        ProximoSkillCheck();
    }

    void ProximoSkillCheck()
    {
        painelSkillCheck.SetActive(true);
        jogoAtivo = true;

        // Reseta o ponteiro para o topo (0 graus)
        ponteiro.localEulerAngles = Vector3.zero;

        // Escolhe um ângulo aleatório na roleta (evitando o topo inicial para dar tempo de reagir)
        anguloAlvo = Random.Range(60f, 300f);
        zonaDeAcerto.localEulerAngles = new Vector3(0, 0, anguloAlvo);
    }

    void ValidarClique()
    {
        jogoAtivo = false;
        float anguloAtual = ponteiro.localEulerAngles.z;

        // Verifica se o ponteiro está dentro do limite da zona alvo
        if (anguloAtual >= anguloAlvo - margemDeAcerto && anguloAtual <= anguloAlvo + margemDeAcerto)
        {
            Debug.Log("Acertou o Skill Check!");
            painelSkillCheck.SetActive(false);

            
        }
        else
        {
            Debug.Log("Errou o Skill Check!");
            errosCometidos++;

            if (errosCometidos >= 3)
            {
                FinalizarComDerrota();
            }
            else
            {
                // Se ainda não errou 3 vezes, manda o próximo instantaneamente
                ProximoSkillCheck();
            }
        }
    }

    void FinalizarComDerrota()
    {
        jogoAtivo = false;
        painelSkillCheck.SetActive(false);
        Debug.LogError("Você falhou 3 vezes no Skill Check!");

     

        // 1. Remove os peixes do Inventário
        int perdidos = Mathf.Min(peixesPerdidosAoFalhar, InventoryManager.Instance.peixesNoInventario);
        InventoryManager.Instance.peixesNoInventario -= perdidos;

        // 2. Desconta os peixes do GameController para o jogador não ganhar o jogo
        if (gameController != null)
        {
            gameController.foundedFish -= perdidos;
            if (gameController.foundedFish < 0) gameController.foundedFish = 0;
        }

        // 3. Manda os peixes de volta para o cenário
        if (respawnManager != null)
        {
            respawnManager.SpawnarPeixesEscondidos(perdidos);
        }
    }
}