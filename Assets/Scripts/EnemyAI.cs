using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Troop[] allTroops;

    public IEnumerator EnemyTurn()
    {
        if(allTroops !=null){
            allTroops = new Troop[0];
        }
        allTroops = FindObjectsOfType<Troop>();
        // Check if it's the enemy phase
        if (GameManager.instance.GetCurrentPhase() == GamePhase.Enemy)
        {
            // Iterate through all enemy troops
            foreach (Troop enemyTroop in allTroops.Where(troop => !troop.IsPlayer && troop.CanMoveThisPhase()))
            {
                // Get valid movement and attack ranges
                List<Tile> validMovementRange = enemyTroop.GetValidMovementRange();
                List<Tile> validAttackRange = enemyTroop.GetValidAttackRange();

                // Find the closest player troop
                Troop closestPlayerTroop = FindClosestPlayerTroop(enemyTroop);
                // Find the movement tile closest to the player's current tile
                Tile closestMovementTile = FindClosestMovementTile(enemyTroop, closestPlayerTroop.currentTile);

                // Check if a player troop is found
                if (closestPlayerTroop != null)
                {
                    
                    // Check if the player troop is within attack range
                    if (validAttackRange.Contains(closestPlayerTroop.currentTile))
                    {
                        // Attack the player troop
                        enemyTroop.AttackEnemy(closestPlayerTroop);
                        enemyTroop.SetMovedThisPhase(); 
                        yield return new WaitForSeconds(2f);
                    }
                    else
                    {
                        // Move towards the closest player troop
                        yield return StartCoroutine(MoveTowardsTile(enemyTroop, closestMovementTile));
                    }
                }
            }
        }
    }

    IEnumerator MoveTowardsTile(Troop enemyTroop, Tile targetTile)
    {
        // Implement logic to move the enemy troop towards the target tile
        // This might involve calculating a path, using a pathfinding algorithm, etc.
        // For simplicity, you can move towards the target tile's current position.

        // Move the enemy troop to the target tile
        enemyTroop.MoveTroopToTile(targetTile);
        enemyTroop.SetMovedThisPhase(); 


        // You can add a delay here if needed
        yield return new WaitForSeconds(2f);
    }
    

    Troop FindClosestPlayerTroop(Troop enemyTroop)
    {
        // Use LINQ to find the closest player troop
        Troop closestPlayerTroop = allTroops
            .Where(troop => troop.IsPlayer) // Filter player troops
            .OrderBy(troop => Vector2.Distance(enemyTroop.currentTile.transform.position, troop.currentTile.transform.position))
            .FirstOrDefault();

        return closestPlayerTroop;
    }
    Tile FindClosestMovementTile(Troop enemyTroop, Tile playerCurrentTile)
    {
        // Get the valid movement range for the enemy troop
        List<Tile> validMovementRange = enemyTroop.GetValidMovementRange();

        // Find the closest tile within the valid movement range
        return validMovementRange
            .OrderBy(tile => Vector2.Distance(tile.transform.position, playerCurrentTile.transform.position))
            .FirstOrDefault();
    }
}
