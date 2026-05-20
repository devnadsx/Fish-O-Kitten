using UnityEngine;

public class ItemColetavel : MonoBehaviour
{
    public ItemData dadosDoItem; // Arraste o arquivo do Peixe aqui no Inspector

    // Esta função do Unity detecta automaticamente o clique do mouse
    private void OnMouseDown()
    {
        // Envia o item para o inventário
        bool foiAdicionado = InventarioManager.Instancia.AdicionarItem(dadosDoItem);

        if (foiAdicionado)
        {
            // Se o inventário aceitou, o peixe some do cenário!
            Destroy(gameObject);
        }
    }
}