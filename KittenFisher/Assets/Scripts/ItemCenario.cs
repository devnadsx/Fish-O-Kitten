using UnityEngine;

public class ItemCenario : MonoBehaviour
{
    [Header("Efeitos Visuais (Opcional)")]
    public GameObject efeitoAoClicar; // Um prefab de poeira ou bolhas ao interagir

    void OnMouseDown()
    {
        Vasculhar();
    }

    void Vasculhar()
    {
        Debug.Log(gameObject.name + " foi vasculhado!");

        // Se você tiver um efeito visual de poeira/bolhas, ele nasce aqui
        if (efeitoAoClicar != null)
        {
            Instantiate(efeitoAoClicar, transform.position, Quaternion.identity);
        }

        // Destrói o objeto do cenário para revelar o que está atrás
        Destroy(gameObject);
    }
}