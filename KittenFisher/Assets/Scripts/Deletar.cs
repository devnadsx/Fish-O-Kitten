using UnityEngine;

public class Deletar : MonoBehaviour
{
    public GameController gameController;

    [Header("Bloqueio de Esconderijo")]
    [Tooltip("Arraste aqui o objeto do cenário (ex: porta do armário) que esconde este peixe.")]
    public GameObject objetoEsconderijo;

    void Start()
    {
        gameController = GetComponentInParent<GameController>();
    }

    public void OnMouseDown()
    {
        // 🔒 Se o esconderijo existe e o SpriteRenderer dele está ativo (porta fechada), bloqueia o clique!
        if (objetoEsconderijo != null)
        {
            SpriteRenderer spriteEsconderijo = objetoEsconderijo.GetComponent<SpriteRenderer>();
            if (spriteEsconderijo != null && spriteEsconderijo.enabled)
            {
                // A porta do armário ainda está cobrindo o peixe! Ignora o clique.
                return;
            }
        }

        // 1. Avisa o GameController original que o peixe foi achado
        if (gameController != null)
        {
            gameController.FoundFish();
        }

        // 2. Adiciona o peixe ao inventário
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AdicionarPeixe();
        }

        Destroy(gameObject);
    }
}