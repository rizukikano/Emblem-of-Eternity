using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum GamePhase
{
    Player,
    Enemy
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Troop selectedTroop;
    private List<Tile> validMovementRange;
    private List<Tile> validAttackRange;
    private GamePhase currentPhase;
    public Button endTurnButton;
    public GameObject gameplayPanel;
    public GameObject resultPanel;

    void Awake()
    {
        instance = this;
        currentPhase = GamePhase.Player; // Start with the player phase
    }

    public GamePhase GetCurrentPhase()
    {
        return currentPhase;
    }
    
    public void SwitchToPlayerPhase()
    {
        currentPhase = GamePhase.Player;
        HandlePlayerPhase();
        
    }

    public void SwitchToEnemyPhase()
    {
        currentPhase = GamePhase.Enemy;
        HandleEnemyPhase();
        // Trigger enemy actions or AI logic here
        StartCoroutine(ExecuteEnemyTurn());
        
    }
    private void HandlePlayerPhase()
    {
        // Implement logic for the player phase
        ResetMoveFlags();
        CheckGameOver();
        endTurnButton.interactable = true;
    }

    private void HandleEnemyPhase()
    {
        // Implement logic for the enemy phase
        ResetMoveFlags();
        CheckGameOver();
        endTurnButton.interactable = false;
    }
    IEnumerator ExecuteEnemyTurn()
    {
        // Assuming you have a reference to the EnemyAI script
        EnemyAI enemyAI = FindObjectOfType<EnemyAI>();

        // Call the EnemyAI logic for the enemy turn
        yield return StartCoroutine(enemyAI.EnemyTurn());

        // After all enemy troops have completed their turns, switch back to the player phase
        SwitchToPlayerPhase();
    }

    public void TileSelected(Tile selectedTile)
    {
        if (selectedTroop != null)
        {
            // Handle logic when a tile is selected
            Debug.Log($"Tile selected: {selectedTile.BoardPosition}");
        }
    }

    public void TroopSelected(Troop troop)
    {
        if(validMovementRange != null && validAttackRange != null){
            ClearRanges();
        }
        selectedTroop = troop;

        if (selectedTroop != null)
        {
            validMovementRange = selectedTroop.GetValidMovementRange();
            validAttackRange = selectedTroop.GetValidAttackRange();
            ShowRanges(validMovementRange, tile => tile.HighlightMoveRange(), validAttackRange, tile => tile.HighlightAttackRange());

            // Deactivate BoxColliders for non-player troops
            ModifyNonPlayerTroopColliders(false);
        }
    }

    public Troop GetSelectedTroop()
    {
        return selectedTroop;
    }

    public bool IsTileInValidMovementRange(Tile tile)
    {
        return validMovementRange.Contains(tile);
    }

    public bool IsTileInValidAttackRange(Tile tile)
    {
        return validAttackRange.Contains(tile);
    }

    public void TileClicked(Tile clickedTile)
    {
        if (selectedTroop == null)
            return;

        // Check if the clicked tile is within the valid movement range
        if (validMovementRange.Contains(clickedTile))
        {
            // Check if there's an enemy in the attack range on the clicked tile
            if (selectedTroop.InAttackRange(clickedTile))
            {
                Troop enemyTroop = clickedTile.OccupyingTroop;

                if (enemyTroop != null && selectedTroop.CanAttack(enemyTroop))
                {
                    // Attack the enemy troop
                    selectedTroop.AttackEnemy(enemyTroop);
                    selectedTroop.SetMovedThisPhase();
                    ClearRanges();
                    // Activate the colliders of non-player troops
                    ModifyNonPlayerTroopColliders(true);
                    return; // Exit the method after attacking
                }
            }

            // Move the troop to the clicked tile
            selectedTroop.MoveTroopToTile(clickedTile);
            // Set the troop as having moved during the current phase
            selectedTroop.SetMovedThisPhase();

            // Clear the movement range and unhighlight tiles
            ClearRanges();
            // Activate the colliders of non-player troops
            ModifyNonPlayerTroopColliders(true);
        }
        else if (selectedTroop.InAttackRange(clickedTile))
        {
            // Check if there's an enemy in the attack range on the clicked tile
            Troop enemyTroop = clickedTile.OccupyingTroop;

            if (enemyTroop != null && selectedTroop.CanAttack(enemyTroop))
            {
                // Get the side tile for attack
                Tile sideTile = GetSideTileForAttack(enemyTroop.currentTile, selectedTroop.AttackRange);

                // Check if the side tile is within the valid movement range
                if (validMovementRange.Contains(sideTile))
                {
                    // Move the troop to the side tile
                    selectedTroop.MoveTroopToTile(sideTile);

                    // Attack the enemy troop
                    selectedTroop.AttackEnemy(enemyTroop);
                    // Set the troop as having moved during the current phase
                    selectedTroop.SetMovedThisPhase();

                    // Clear the movement range and unhighlight tiles
                    ClearRanges();
                    // Activate the colliders of non-player troops
                    ModifyNonPlayerTroopColliders(true);
                }
            }
        }
    }
    private void ResetMoveFlags()
    {
        Troop[] allTroops = FindObjectsOfType<Troop>();

        foreach (Troop troop in allTroops)
        {
            troop.ResetMoveFlag();
        }
    }

    private void ShowRanges(List<Tile> movementRange, System.Action<Tile> movementHighlightAction, List<Tile> attackRange, System.Action<Tile> attackHighlightAction)
    {
        HashSet<Tile> highlightedTiles = new HashSet<Tile>();

        // Highlight the valid attack range first
        foreach (var tile in attackRange)
        {
            attackHighlightAction.Invoke(tile);
            highlightedTiles.Add(tile);
        }

        // Highlight the valid movement range, excluding tiles already highlighted for attack
        foreach (var tile in movementRange)
        {
            if (!highlightedTiles.Contains(tile))
            {
                movementHighlightAction.Invoke(tile);
            }
        }
    }
    

    private void ClearRanges()
    {
        ClearHighlight(validMovementRange, tile => tile.ClearMoveRangeHighlight());
        ClearHighlight(validAttackRange, tile => tile.ClearAttackRangeHighlight());

        validMovementRange.Clear();
        validAttackRange.Clear();
    }

    public void ClearHighlight(List<Tile> tiles, System.Action<Tile> clearHighlightAction)
    {
        foreach (var tile in tiles)
        {
            clearHighlightAction.Invoke(tile);
        }
    }
    private Tile GetSideTileForAttack(Tile enemyTile, int attackRange)
    {
        // Ensure selectedTroop.CurrentTile is not null before accessing its properties
        if (selectedTroop != null && selectedTroop.currentTile != null)
        {
            // Determine the side of the enemy troop to move towards
            int directionX = 0;
            int directionY = 0;

            if (selectedTroop.currentTile.BoardPosition.x < enemyTile.BoardPosition.x)
            {
                directionX = 1; // Move to the right
            }
            else if (selectedTroop.currentTile.BoardPosition.x > enemyTile.BoardPosition.x)
            {
                directionX = -1; // Move to the left
            }

            if (selectedTroop.currentTile.BoardPosition.y < enemyTile.BoardPosition.y)
            {
                directionY = 1; // Move up
            }
            else if (selectedTroop.currentTile.BoardPosition.y > enemyTile.BoardPosition.y)
            {
                directionY = -1; // Move down
            }

            if (attackRange == 1)
                if (Mathf.Abs(directionX) == 1 && Mathf.Abs(directionY) == 1)
                {
                    // If diagonally, set the direction with the larger absolute value to 0
                    if (Mathf.Abs(directionX) > Mathf.Abs(directionY))
                    {
                        directionY = 0;
                    }
                    else
                    {
                        directionX = 0;
                    }
                }

            // Calculate the target tile position based on the enemy's position and attack range
            Vector2Int targetPosition = new Vector2Int(
                enemyTile.BoardPosition.x - (directionX * attackRange),
                enemyTile.BoardPosition.y - (directionY * attackRange)
            );

            // Get the tile at the calculated position
            Tile targetTile = Board.instance.GetTileAtPosition(targetPosition);

            return targetTile;
        }

        // Return null if selectedTroop or its current tile is null
        return null;
    }
    private void ModifyNonPlayerTroopColliders(bool activate)
    {
        Troop[] allTroops = FindObjectsOfType<Troop>();

        foreach (Troop troop in allTroops)
        {
            // Check if the troop is not controlled by the player
            if (!troop.IsPlayer)
            {
                // Get the BoxCollider
                BoxCollider2D collider = troop.GetComponent<BoxCollider2D>();

                // Modify the collider based on the 'activate' parameter
                if (collider != null)
                {
                    collider.enabled = activate;
                }
            }
        }
    }
    public void CheckGameOver()
    {
        // Check if there are no more enemy troops
        if (NoEnemyTroopsLeft())
        {
            Debug.Log("Game Over - Player Wins!");
            gameplayPanel.SetActive(false);
            resultPanel.SetActive(true);
        }
        // Check if there are no more player troops
        else if (NoPlayerTroopsLeft())
        {
            Debug.Log("Game Over - Enemy Wins!");
            gameplayPanel.SetActive(false);
            resultPanel.SetActive(true);
        }
    }

    private bool NoEnemyTroopsLeft()
    {
        Troop[] enemyTroops = FindObjectsOfType<Troop>().Where(troop => !troop.IsPlayer).ToArray();
        return enemyTroops.Length == 0;
    }

    private bool NoPlayerTroopsLeft()
    {
        Troop[] playerTroops = FindObjectsOfType<Troop>().Where(troop => troop.IsPlayer).ToArray();
        return playerTroops.Length == 0;
    }
    public void MoveScene(string sceneName){
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    public void ReloadScene(){
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
