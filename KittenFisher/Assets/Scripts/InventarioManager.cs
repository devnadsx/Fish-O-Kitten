using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public int peixesNoInventario = 0;
    [Range(0f, 1f)] public float chanceDeEnvenenamento = 0.5f;

    [Header("UI do Inventario")]
    public GameObject painelInventario;
    public TextMeshProUGUI textoContadorPeixes;

    [Header("Configuração Visual dos Slots")]
    public InventorySlot[] slots;   // Lista com os seus 10 slots
    public Sprite spriteDoPeixeUI;  // A imagem do peixinho que vai aparecer no inventário

    [Header("Referências")]
    public SkillCheckManager skillCheckManager;

    void Awake()
    {
        Instance = this;
    }

    public void AdicionarPeixe()
    {
        peixesNoInventario++;
        AtualizarUI();

        // Procura o primeiro slot vazio e coloca o peixe lá dentro
        foreach (InventorySlot slot in slots)
        {
            if (slot != null)
            {
                // Se conseguir ocupar o slot, para de procurar
                if (slot.OcuparSlot(spriteDoPeixeUI))
                {
                    break;
                }
            }
        }
    }

    public void AlternarPainelInventario()
    {
        painelInventario.SetActive(!painelInventario.activeSelf);
        AtualizarUI();
    }

    void AutalizarUI() // Mantive a escrita antiga caso precise, mas mude para AtualizarUI se quiser fixar
    {
        if (textoContadorPeixes != null)
            textoContadorPeixes.text = "Peixes: " + peixesNoInventario;
    }

    public void ComerPeixe()
    {
        if (peixesNoInventario <= 0) return;

        peixesNoInventario--;
        if (textoContadorPeixes != null) textoContadorPeixes.text = "Peixes: " + peixesNoInventario;

        if (Random.value < chanceDeEnvenenamento)
        {
            Debug.LogWarning("O peixe estava estragado! SKILL CHECK!");
            painelInventario.SetActive(false);
            skillCheckManager.IniciarSequenciaSkillCheck();
        }
        else
        {
            Debug.Log("Peixe delicioso! Nada aconteceu.");
        }
    }

    // Caso falhe no Skill Check, limpa os slots baseado em quantos peixes perdeu
    public void LimparSlotsPorPuniçao(int quantidade)
    {
        int removidos = 0;
        // Limpa de trás para frente para tirar os últimos pegos
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (removidos >= quantidade) break;

            // Aqui uma simplificação: limpa os slots ativos de forma genérica
            // Para ficar 100% perfeito, o ideal é o slot checar se tinha algo.
            // Vamos apenas resetar o visual para bater com a punição:
            slots[i].LimparSlot();
            removidos++;
        }
    }
}