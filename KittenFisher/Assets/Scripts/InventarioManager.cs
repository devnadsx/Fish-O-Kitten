
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Importante para mexer com imagens

public class InventarioManager : MonoBehaviour
{
    public static InventarioManager Instancia; // Permite que o peixe ache o inventário facilmente

    public List<ItemData> listaDeItens = new List<ItemData>(); // Sua lista de itens guardados
    public Transform painelDoInventario; // Onde os slots de imagem ficam na UI
    public GameObject slotPrefab; // Um quadrado de UI com um componente <Image>

    private void Awake()
    {
        // Garante que só exista um inventário na tela
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    public bool AdicionarItem(ItemData novoItem)
    {
        // Adiciona o item à lista lógica
        listaDeItens.Add(novoItem);

        // Atualiza a interface visual do inventário
        AtualizarInterfaceVisual(novoItem);

        return true;
    }

    void AtualizarInterfaceVisual(ItemData item)
    {
        // Cria um novo slot visual dentro do seu painel de inventário
        GameObject novoSlot = Instantiate(slotPrefab, painelDoInventario);

        // Pega o componente de Imagem desse slot e joga a foto do peixe nele
        Image imagemDoSlot = novoSlot.GetComponentInChildren<Image>();
        if (imagemDoSlot != null)
        {
            imagemDoSlot.sprite = item.iconeDoItem;
        }
    }
}
