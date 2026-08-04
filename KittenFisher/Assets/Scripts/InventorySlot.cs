using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconeDoPeixe;
    public Button botaoComer;

    [HideInInspector] public bool estaOcupado = false;

    void Awake()
    {
        // Executa no exato momento da criação na memória (antes de abrir a tela)
        // Só deixa transparente SE ainda não tiver peixe nele!
        if (!estaOcupado)
        {
            EsconderSlotVisualmente();
        }
    }

    public bool OcuparSlot(Sprite spritePeixe)
    {
        if (estaOcupado) return false;

        estaOcupado = true;

        if (iconeDoPeixe != null)
        {
            iconeDoPeixe.sprite = spritePeixe;
            Color cor = iconeDoPeixe.color;
            cor.a = 1f; // Visível
            iconeDoPeixe.color = cor;
        }

        if (botaoComer != null)
            botaoComer.interactable = true;

        return true;
    }

    public void LimparSlot()
    {
        estaOcupado = false;
        EsconderSlotVisualmente();
    }

    private void EsconderSlotVisualmente()
    {
        if (iconeDoPeixe != null)
        {
            Color cor = iconeDoPeixe.color;
            cor.a = 0f; // Transparente
            iconeDoPeixe.color = cor;
        }

        if (botaoComer != null)
            botaoComer.interactable = false;
    }

    public void ClicouNoSlot()
    {
        if (!estaOcupado) return;

        // Limpa apenas ESTE peixe específico
        LimparSlot();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ComerPeixe();
        }
    }
}