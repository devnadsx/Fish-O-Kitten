using UnityEngine;

public class FishRespawnManager : MonoBehaviour
{
    public GameObject peixePrefab; // Prefab do seu peixe interativo/escondido
    public Transform[] pontosDeEsconderijo; // Locais no cenário onde eles podem reaparecer

    public void SpawnarPeixesEscondidos(int quantidade)
    {
        int spawnados = 0;

        // Tenta spawnar nos pontos disponíveis
        for (int i = 0; i < quantidade; i++)
        {
            // Escolhe um ponto aleatório da lista
            int indiceAleatorio = Random.Range(0, pontosDeEsconderijo.Length);
            Transform pontoEscolhido = pontosDeEsconderijo[indiceAleatorio];

            if (pontoEscolhido != null)
            {
                // Spawna o peixe de volta na cena
                Instantiate(peixePrefab, pontoEscolhido.position, pontoEscolhido.rotation, transform);
                spawnados++;
            }
        }

        Debug.Log(spawnados + " peixes fugiram e se esconderam no cenário novamente!");
    }
}