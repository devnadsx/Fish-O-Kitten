using UnityEngine;

public class Deletar : MonoBehaviour
{
    public GameController gameController;

    void Start()
    {
        gameController = GetComponentInParent<GameController>();
    }

    public void OnMouseDown()
    {
        // 1. Avisa o GameController original que o peixe foi achado
        if (gameController != null)
        {
            gameController.FoundFish();
        }

        // 2. Adiciona o peixe ao inventário criado acima
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AdicionarPeixe();
        }

        Destroy(gameObject);
    }
}