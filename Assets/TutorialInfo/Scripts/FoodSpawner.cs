using UnityEngine;
using System.Collections;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food Prefabs")]
    public GameObject bananaPrefab;
    public GameObject burgerPrefab;

    [Header("Spawn Settings")]
    public Transform spawnAreaCenter; // Empty GameObject at ceiling center
    public float spawnHeight = 3f;
    public float spawnRadius = 1.5f; // Random spawn area radius
    public float spawnInterval = 2f; // Time between spawns

    [Header("Game State")]
    public bool isGameActive = true;

    private bool isSpawning = false;

    void Start()
    {
        if (spawnAreaCenter == null)
        {
            Debug.LogError("Please assign Spawn Area Center!");
            return;
        }

        StartSpawning();
    }

    public void StartSpawning()
    {
        if (!isSpawning && isGameActive)
        {
            isSpawning = true;
            StartCoroutine(SpawnFoodRoutine());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        isGameActive = false;
        StopAllCoroutines();
        Debug.Log("Game Over - Missed a catch!");
    }

    IEnumerator SpawnFoodRoutine()
    {
        while (isGameActive && isSpawning)
        {
            SpawnRandomFood();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomFood()
    {
        // Choose random food
        GameObject foodPrefab = Random.value > 0.5f ? bananaPrefab : burgerPrefab;

        // Random position within spawn radius
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = spawnAreaCenter.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // Spawn the food
        GameObject food = Instantiate(foodPrefab, spawnPos, Random.rotation);

        // Add FoodItem component if not already present
        if (food.GetComponent<FoodItem>() == null)
        {
            food.AddComponent<FoodItem>();
        }

        food.GetComponent<FoodItem>().spawner = this;
    }

    public void RestartGame()
    {
        isGameActive = true;
        // Clean up any existing food
        FoodItem[] foods = FindObjectsOfType<FoodItem>();
        foreach (FoodItem food in foods)
        {
            Destroy(food.gameObject);
        }

        StartSpawning();
    }
}