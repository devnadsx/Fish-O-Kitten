using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Configurações do Inventário")]
    public int peixesNoInventario = 0;
    [Range(0f, 1f)] public float chanceDeEnvenenamento = 0.5f;

    [Header("Configurações de Vitória")]
    [Tooltip("Quantidade total de peixes que existem no cenário para vencer")]
    public int totalPeixesParaVitoria = 3;
    [Tooltip("Arraste aqui o objeto da sua Janela de Vitória (Victory Window)")]
    public GameObject victoryWindow;
    [Tooltip("Arraste aqui o seu Objeto de Particle System de Confete")]
    public ParticleSystem particulaVitoria;
    [Tooltip("Arraste aqui o AudioSource que contém o som de vitória")]
    public AudioSource somVitoria; // NOVO CAMPO

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

        // Garante que a janela de vitória e o som comecem escondidos/parados
        if (victoryWindow != null) victoryWindow.SetActive(false);
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

        if (peixesNoInventario >= totalPeixesParaVitoria)
        {
            GanharJogo();
        }
    }

    private void GanharJogo()
    {
        Debug.Log("🏆 Todos os peixes coletados! VITÓRIA!");

        // 1. Ativa a janela de vitória
        if (victoryWindow != null)
        {
            victoryWindow.SetActive(true);
        }

        // 2. Toca as partículas de confete
        if (particulaVitoria != null)
        {
            particulaVitoria.gameObject.SetActive(true);
            particulaVitoria.Play();
        }

        // 3. Toca o som de vitória (NOVO)
        if (somVitoria != null)
        {
            somVitoria.Play();
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