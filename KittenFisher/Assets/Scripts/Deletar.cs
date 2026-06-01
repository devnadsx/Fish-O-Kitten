using UnityEngine;

public class Deletar : MonoBehaviour
{
    public GameController gameController;

    void Start()
    {
        // Encontra o GameController no objeto pai
        gameController = GetComponentInParent<GameController>();
    }

    void Update()
    {

    }

    public void OnMouseDown()
    {
        // Verifica se o GameController realmente foi encontrado para evitar erros
        if (gameController != null)
        {
            gameController.FoundFish(); // <--- A LINHA QUE FALTAVA! Avisa o controller.
        }
        else
        {
            Debug.LogWarning("GameController não encontrado no pai de: " + gameObject.name);
        }

        // Agora sim, destrói o peixe
        Destroy(gameObject);
    }
}
