﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private float timer;
    public EnemyBehaviour enemyPrefab;
    public List<BossBehaviour> bossList;
    public ObjectPool objectPool;

    void Start()
    {
        timer = 0;

        objectPool.GenerateEnemyBullets(Color.yellow);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= 60.0f / GameManager.enemySpawnRPM)
        {
            timer = 0;
            if(!GameManager.bossFightInProgress && !GameManager.isScreenOcluded)
                SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        EnemyBehaviour enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, transform);
        Bounds bounds = enemy.GetComponent<SpriteRenderer>().bounds;

        Vector2 ceilingPoint = Vector2.one * float.NegativeInfinity;
        Vector2 floorPoint = Vector2.one * float.PositiveInfinity;
        Vector2 screedEdge = new Vector3(Boundary.visibleWorldMax.x + (Boundary.visibleWorldSize.x * 0.1f), Boundary.visibleWorldCentre.y);
        Vector2 raycastOrigin = new Vector2(screedEdge.x + bounds.extents.x, Boundary.visibleWorldMin.y);

        while (raycastOrigin.y <= Boundary.visibleWorldMax.y)
        {
            RaycastHit2D rayHitUp = Physics2D.Raycast(raycastOrigin, Vector2.up);
            RaycastHit2D rayHitDown = Physics2D.Raycast(raycastOrigin, Vector2.down);

            raycastOrigin += Vector2.up * 0.1f;

            if (!rayHitUp.transform || !rayHitDown.transform) continue;

            ceilingPoint = rayHitUp.transform.CompareTag("BoundaryTilemap") ? rayHitUp.point : ceilingPoint;
            floorPoint = rayHitDown.transform.CompareTag("BoundaryTilemap") ? rayHitDown.point : floorPoint;

            if (rayHitUp.transform.CompareTag("DestructibleObstacle") || rayHitDown.transform.CompareTag("DestructibleObstacle"))
                raycastOrigin += new Vector2(1, -1) * 0.1f;

            if (ceilingPoint.y > floorPoint.y)
            {
                //Debug.Log("Cave height resolved. Spawning enemy");
                break;
            }
        }

        if (ceilingPoint.y <= floorPoint.y)
        {
            //Debug.Log("enemy spawn failed. Could not find resolve cave height.");
            Destroy(enemy.gameObject);
            return;
        }

        Vector2 enemyPos = new Vector2(floorPoint.x, Random.Range(floorPoint.y + bounds.extents.y, ceilingPoint.y - bounds.extents.y));

        enemy.TakeOrders(enemyPos);
    }

    public void SpawnBoss(GameManager.Environment environment)
    {
        BossBehaviour boss = Instantiate(GetBoss(environment), transform.position, Quaternion.identity, transform);

        Bounds bounds = boss.GetComponent<SpriteRenderer>().bounds;

        Vector2 screedEdge = new Vector3(Boundary.visibleWorldMax.x + (Boundary.visibleWorldSize.x * 0.1f), Boundary.visibleWorldCentre.y);
        Vector2 raycastOrigin = new Vector2(screedEdge.x, screedEdge.y);

        int layerMask = LayerMask.GetMask("Tilemap");

        RaycastHit2D rayHitUp = Physics2D.Raycast(raycastOrigin, Vector2.up, Boundary.visibleWorldSize.y, layerMask);
        RaycastHit2D rayHitDown = Physics2D.Raycast(raycastOrigin, Vector2.down, Boundary.visibleWorldSize.y, layerMask);

        float bossHeight = rayHitUp.point.y - rayHitDown.point.y;

        float scale = bossHeight * 1.1f / bounds.size.y;

        float bossWidth = bounds.size.x * scale;

        Vector2 bossPos = new Vector2(rayHitDown.point.x + bounds.extents.x, rayHitDown.point.y + 0.9f * bounds.extents.y * scale);

        boss.TakeOrders(bossPos, Vector3.one * scale, bossWidth, 50);
    }

    private BossBehaviour GetBoss(GameManager.Environment environment)
    {
        List<int> candidates = new List<int>();

        for (int i = 0; i < bossList.Count; i++)
        {
            if (bossList[i].environment == environment)
            {
                candidates.Add(i);
            }
        }

        if (candidates.Count < 1) Debug.LogError("This environment does not have any bosses");

        int raffle = Random.Range(0, candidates.Count);

        return bossList[candidates[raffle]];
    }
}
