using UnityEngine;

public class GerenciadorInventario : MonoBehaviour
{
    public static GerenciadorInventario Instance;

    [Header("Itens Coletados")]
    public bool temPicareta = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
}