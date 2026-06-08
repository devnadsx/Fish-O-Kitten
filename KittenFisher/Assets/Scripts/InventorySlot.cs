using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconeDoPeixe; // Arraste a imagem que fica dentro do slot
    public Button botaoComer;   // O próprio componente Button do slot

    private bool estaOcupado = false;

    void Start()
    {
        // Garante que o slot começa vazio visualmente
        LimparSlot();
    }

    public bool OcuparSlot(Sprite spritePeixe)
    {
        if (estaOcupado) return false; // Se já tem peixe aqui, pula para o próximo slot

        estaOcupado = true;
        iconeDoPeixe.sprite = spritePeixe;
        iconeDoPeixe.enabled = true;   // Mostra o desenho do peixe
        botaoComer.interactable = true; // Permite clicar no slot para comer

        return true;
    }

    public void LimparSlot()
    {
        estaOcupado = false;
        if (iconeDoPeixe != null) iconeDoPeixe.enabled = false; // Esconde o desenho
        if (botaoComer != null) botaoComer.interactable = false; // Bloqueia o clique
    }

    // Função que será chamada quando o jogador clicar NESTE slot específico
    public void ClicouNoSlot()
    {
        if (!estaOcupado) return;

        // Avisa o InventoryManager para gastar o peixe e rodar a chance de veneno
        if (InventoryManager.Instance != null)
        {
            LimparSlot(); // Remove o peixe deste slot visualmente
            InventoryManager.Instance.ComerPeixe();
        }
    }
}