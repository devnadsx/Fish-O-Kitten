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
    public float margemDeAcerto = 20f; // Tamanho da zona (em graus)

    [Header("Punição")]
    public int peixesPerdidosAoFalhar = 2;
    public GameController gameController;
    public FishRespawnManager respawnManager;

    void Update()
    {
       
            if (!jogoAtivo) return;

            // Gira o ponteiro suavemente no eixo Z
            // Usando Vector3.forward * -velocidadeRotacao garante que ele gire perfeitamente no sentido horário
            ponteiro.Rotate(0, 0, -velocidadRotacao * Time.deltaTime, Space.Self);
        

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

        // 1. Escolhe um ângulo aleatório
        anguloAlvo = Random.Range(60f, 300f);

        // 2. Aplica a rotação no objeto visual da Zona de Acerto
        zonaDeAcerto.localEulerAngles = new Vector3(0, 0, anguloAlvo);

        // 🌟 CORREÇÃO AQUI: Força o 'anguloAlvo' a ser exatamente a rotação z real do objeto.
        // Isso evita qualquer bug de diferença de hierarquia do Canvas!
        anguloAlvo = zonaDeAcerto.localEulerAngles.z;
    }

    void ValidarClique()
    {
        jogoAtivo = false;

        // Lemos a rotação local do Eixo, que agora gira perfeitamente de 0 a 360 graus
        float anguloAtual = ponteiro.localEulerAngles.z;
        float anguloZAlvo = zonaDeAcerto.localEulerAngles.z;

        // Calcula a distância real entre os dois ângulos
        float diferencaAngulo = Mathf.Abs(Mathf.DeltaAngle(anguloAtual, anguloZAlvo));

        if (diferencaAngulo <= margemDeAcerto)
        {
            Debug.Log("Acertou o Skill Check! Diferença real: " + diferencaAngulo + " graus.");
            painelSkillCheck.SetActive(false);

            if (CatIconManager.Instance != null)
            {
                CatIconManager.Instance.ResetarNormal();
            }
        }
        else
        {
            Debug.Log("Errou o Skill Check! Diferença real: " + diferencaAngulo + " graus.");
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

        // 🌟 NOVO: O jogo acabou por derrota, limpa a carinha de envenenado do gatinho também
        if (CatIconManager.Instance != null)
        {
            CatIconManager.Instance.ResetarNormal();
        }

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