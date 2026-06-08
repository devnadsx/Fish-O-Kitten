using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public int peixesNoInventario = 0;
    [Range(0f, 1f)] public float chanceDeEnvenenamento = 0.5f; // 50% de chance de vir premiado

    [Header("UI do Inventario")]
    public GameObject painelInventario;
    public Text textoContadorPeixes;

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
    }

    public void AlternarPainelInventario()
    {
        painelInventario.SetActive(!painelInventario.activeSelf);
        AtualizarUI();
    }

    void AtualizarUI()
    {
        if (textoContadorPeixes != null)
            textoContadorPeixes.text = "Peixes: " + peixesNoInventario;
    }

    // Função que será colocada no Botão "Comer" da UI
    public void ComerPeixe()
    {
        if (peixesNoInventario <= 0) return;

        peixesNoInventario--;
        AtualizarUI();

        // Sorteio de envenenamento
        if (Random.value < chanceDeEnvenenamento)
        {
            Debug.LogWarning("O peixe estava estragado! SKILL CHECK!");
            painelInventario.SetActive(false); // Fecha o inventário para focar no minigame
            skillCheckManager.IniciarSequenciaSkillCheck();
        }
        else
        {
            Debug.Log("Peixe delicioso! Nada aconteceu.");
        }
    }
}