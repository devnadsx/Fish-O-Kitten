using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Configurações do Inventário")]
    public int peixesNoInventario = 0;
    [Range(0f, 1f)] public float chanceDeEnvenenamento = 0.5f;

    [Header("UI do Inventário")]
    public GameObject painelInventario;
    public TextMeshProUGUI textoContadorPeixes;

    [Header("Configuração Visual dos Slots")]
    public InventorySlot[] slots;
    public Sprite spriteDoPeixeUI;

    [Header("Referências Extra")]
    public SkillCheckManager skillCheckManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        AtualizarUI();
    }

    public void AdicionarPeixe()
    {
        peixesNoInventario++;
        AtualizarUI();

        foreach (InventorySlot slot in slots)
        {
            if (slot != null && slot.OcuparSlot(spriteDoPeixeUI))
            {
                break;
            }
        }
    }

    public void AlternarPainelInventario()
    {
        if (painelInventario != null)
        {
            painelInventario.SetActive(!painelInventario.activeSelf);
            AtualizarUI();
        }
    }

    public void AtualizarUI()
    {
        if (textoContadorPeixes != null)
            textoContadorPeixes.text = "Peixes: " + peixesNoInventario;
    }

    public void ComerPeixe()
    {
        if (peixesNoInventario <= 0) return;

        peixesNoInventario--;
        AtualizarUI();

        if (Random.value < chanceDeEnvenenamento)
        {
            Debug.LogWarning("O peixe estava estragado! A iniciar SKILL CHECK!");

            if (painelInventario != null) painelInventario.SetActive(false);
            if (skillCheckManager != null) skillCheckManager.IniciarSequenciaSkillCheck();
        }
        else
        {
            Debug.Log("Peixe delicioso! Nada de mau aconteceu.");
        }
    }

    public void LimparSlotsPorPunicao(int quantidade)
    {
        int removidos = 0;
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