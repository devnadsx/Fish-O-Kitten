using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Configurações do Inventário")]
    public int peixesNoInventario = 0;
    [Range(0f, 1f)] public float chanceDeEnvenenamento = 0.5f;

    [Header("Efeitos Sonoros")]
    public AudioSource audioSourceSFX;
    public AudioClip somComerPeixe;

    [Header("UI do Inventário")]
    public GameObject painelInventario;

    [Header("Configuração Visual dos Slots")]
    public InventorySlot[] slots;
    public Sprite spriteDoPeixeUI;

    [Header("Referências Extra")]
    public SkillCheckManager skillCheckManager;
    public IconManager iconManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (painelInventario != null)
        {
            slots = painelInventario.GetComponentsInChildren<InventorySlot>(true);
        }
    }

    public void AdicionarPeixe()
    {
        peixesNoInventario++;

        if (slots != null)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot != null && !slot.estaOcupado)
                {
                    slot.OcuparSlot(spriteDoPeixeUI);
                    break;
                }
            }
        }
        // A verificação de vitória foi removida daqui para não disparar antes da hora!
    }

    public void ComerPeixe()
    {
        if (peixesNoInventario <= 0) return;

        peixesNoInventario--;

        if (Random.value < chanceDeEnvenenamento)
        {
            Debug.LogWarning("O peixe estava estragado! A iniciar SKILL CHECK!");

            if (iconManager != null) iconManager.MudarParaEnvenenado();
            if (painelInventario != null) painelInventario.SetActive(false);
            if (skillCheckManager != null) skillCheckManager.IniciarSequenciaSkillCheck();
        }
        else
        {
            Debug.Log("Peixe delicioso!");

            if (audioSourceSFX != null && somComerPeixe != null)
            {
                audioSourceSFX.PlayOneShot(somComerPeixe);
            }

            if (iconManager != null) iconManager.MudarParaFeliz();
        }
    }

    public void AlternarPainelInventario()
    {
        if (painelInventario != null)
        {
            painelInventario.SetActive(!painelInventario.activeSelf);
        }
    }

    public void LimparSlotsPorPunicao(int quantidade)
    {
        int removidos = 0;
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (removidos >= quantidade) break;
            if (slots[i] != null && slots[i].estaOcupado)
            {
                slots[i].LimparSlot();
                removidos++;
            }
        }
    }
}