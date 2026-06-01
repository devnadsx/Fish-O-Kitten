using UnityEngine;

public class Bau : MonoBehaviour
{
    public GameController gameController; // Arraste o GameController aqui no Inspector
    public GameObject peixeSecretoPrefab; // O Prefab do seu peixe secreto
    public Transform[] spawnPoints;       // Os locais onde os peixes vão aparecer

    private bool jaAberto = false;

    void OnMouseDown()
    {
        if (!jaAberto)
        {
            AbrirBau();
        }
    }

    void AbrirBau()
    {
        jaAberto = true;
        Debug.Log("Catnip encontrado! Spawnando peixes secretos...");

        // Cria os peixes em cada ponto demarcado
        foreach (Transform ponto in spawnPoints)
        {
            if (ponto != null && gameController != null)
            {
                // Instancia o peixe como FILHO do GameController para o sistema de contagem funcionar
                Instantiate(peixeSecretoPrefab, ponto.position, ponto.rotation, gameController.transform);
            }
        }

        // Opcional: Mudar a cor do baú ou desativar o colisor dele para não clicar de novo
        GetComponent<Collider>().enabled = false;
    }
}
