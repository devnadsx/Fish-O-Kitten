using UnityEngine;

[CreateAssetMenu(fileName = "Novo Item", menuName = "Inventario/Item")]
public class ItemData : ScriptableObject
{
    public string nomeDoItem;
    public Sprite iconeDoItem; // Aqui vai a imagem do peixe para o inventário
}

