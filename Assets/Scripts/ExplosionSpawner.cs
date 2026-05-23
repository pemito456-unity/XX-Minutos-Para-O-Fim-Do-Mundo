using UnityEngine;

/// <summary>
/// Instancia o prefab MissileExplosion (mesma animação) em qualquer posição do mundo.
/// O prefab é registrado automaticamente pelos mísseis do jogador na cena.
/// </summary>
public static class ExplosionSpawner
{
    private static GameObject cachedPrefab;

    public static void RegisterPrefab(GameObject prefab)
    {
        if (prefab != null)
            cachedPrefab = prefab;
    }

    public static void SpawnAt(Vector3 worldPosition)
    {
        if (cachedPrefab == null)
            return;

        Vector3 pos = worldPosition;
        pos.z = 0f;
        Object.Instantiate(cachedPrefab, pos, Quaternion.identity);
    }
}
