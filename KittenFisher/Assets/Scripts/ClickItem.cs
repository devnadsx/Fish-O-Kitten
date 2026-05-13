using UnityEngine;

public class ClickItem : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        // Procura o GameManager na cena automaticamente
        gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    private void OnMouseDown()
    {
        if (gameManager != null)
        {
            gameManager.RegistrarItemEncontrado();
            Destroy(gameObject); // Remove o item da tela
        }
    }
}