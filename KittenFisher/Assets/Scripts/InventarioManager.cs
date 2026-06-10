using UnityEngine;
using TMPro; // Mantém se usares TextMeshPro, ou muda para 'using UnityEngine.UI;' se for o texto comum

public class InventoryManager : MonoBehaviour
{
    // Permite que outros scripts (como o Deletar.cs e o InventorySlot.cs) acessem o gerente facilmente
    public static InventoryManager Instance;

    [Header("Configurações do Inventário")]
    public int peixesNoInventario = 0;
    [Range(0f, 1f)] public float chanceDeEnvenenamento = 0.5f; // 0.5f significa 50% de chance

    [Header("UI do Inventário")]
    public GameObject painelInventario;
    public TextMeshProUGUI textoContadorPeixes; // Muda para 'public Text textoContadorPeixes;' se usares o UI Text antigo

    [Header("Configuração Visual dos Slots")]
    public InventorySlot[] slots;   // Lista onde vais arrastar os teus 10 botões invisíveis
    public Sprite spriteDoPeixeUI;  // A imagem do peixinho que vai aparecer dentro do slot

    [Header("Referências Extra")]
    public SkillCheckManager skillCheckManager;

    void Awake()
    {
        // Configura a Instância para o funcionamento dos outros scripts
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        AtualizarUI();
    }

    // Função chamada pelo script 'Deletar.cs' quando apanhas um peixe no cenário
    public void AdicionarPeixe()
    {
        peixesNoInventario++;
        AtualizarUI();

        // Percorre a lista de botões invisíveis e coloca o peixe no primeiro que estiver livre
        foreach (InventorySlot slot in slots)
        {
            if (slot != null)
            {
                if (slot.OcuparSlot(spriteDoPeixeUI))
                {
                    break; // Encontrou um slot vazio, colocou o peixe e para a busca
                }
            }
        }
    }

    // Abre e fecha o inventário (podes ligar esta função ao teu Botão de Ícone do Inventário)
    public void AlternarPainelInventario()
    {
        if (painelInventario != null)
        {
            painelInventario.SetActive(!painelInventario.activeSelf);
            AtualizarUI();
        }
    }

    // Atualiza o texto de contagem (ex: "Peixes: 1")
    public void AtualizarUI()
    {
        if (textoContadorPeixes != null)
        {
            textoContadorPeixes.text = "Peixes: " + peixesNoInventario;
        }
    }

    // Função chamada pelo 'InventorySlot.cs' quando clicas num peixe para o comer
    public void ComerPeixe()
    {
        if (peixesNoInventario <= 0) return;

        peixesNoInventario--;
        AtualizarUI();

        // Sorteio de envenenamento (Gera um número entre 0.0 e 1.0)
        if (Random.value < chanceDeEnvenenamento)
        {
            Debug.LogWarning("O peixe estava estragado! A iniciar SKILL CHECK!");

            if (painelInventario != null)
                painelInventario.SetActive(false); // Fecha o inventário para focar no minigame

            if (skillCheckManager != null)
                skillCheckManager.IniciarSequenciaSkillCheck();
        }
        else
        {
            Debug.Log("Peixe delicioso! Nada de mau aconteceu.");
        }
    }

    // Função de punição: caso o jogador falte ao Skill Check, remove o visual dos slots
    public void LimparSlotsPorPunicao(int quantidade)
    {
        int removidos = 0;

        // Percorre os slots de trás para a frente para remover os últimos peixes que entraram
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (removidos >= quantidade) break;

            if (slots[i] != null)
            {
                slots[i].LimparSlot();
                removidos++;
            }
        }
    }
}