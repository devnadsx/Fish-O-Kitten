using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconeDoPeixe; // O próprio componente Image do botão
    public Button botaoComer;  // O próprio componente Button

    private bool estaOcupado = false;

    void Start()
    {
        LimparSlot();
    }

    public bool OcuparSlot(Sprite spritePeixe)
    {
        if (estaOcupado) return false;

        estaOcupado = true;
        iconeDoPeixe.sprite = spritePeixe;

        // Deixa a imagem visível (Alpha em 100%)
        Color cor = iconeDoPeixe.color;
        cor.a = 1f;
        iconeDoPeixe.color = cor;

        botaoComer.interactable = true;

        return true;
    }

    public void LimparSlot()
    {
        estaOcupado = false;

        // Deixa a imagem transparente (Alpha em 0%) para sumir o peixe
        Color cor = iconeDoPeixe.color;
        cor.a = 0f;
        iconeDoPeixe.color = cor;

        botaoComer.interactable = false;
    }

    public void ClicouNoSlot()
    {
        if (!estaOcupado) return;

        if (InventoryManager.Instance != null)
        {
            LimparSlot();
            InventoryManager.Instance.ComerPeixe();
        }
    }
}