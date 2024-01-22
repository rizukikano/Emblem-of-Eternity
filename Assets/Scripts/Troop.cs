using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Build.Content;

public enum MovementType
{
    Infantry,
    Cavalry,
    Flier
}

public enum AttackType
{
    Physical,
    Magic
}

public class Troop : MonoBehaviour
{
    // Troop characteristics
    [SerializeField] private MovementType troopMovementType;
    public AttackType Attack;
    public int MovementRange = 2;
    public int AttackRange = 2;
    public bool IsPlayer;
    private bool hasMovedThisPhase = false;

    // Troop stats
    public int HP;
    public int ATK;
    public int DEF;
    public int RES;

    // Item slot
    public Item equippedItem;

    public Tile currentTile;
    private int currentHP;

    public MovementType TroopMovementType => troopMovementType;

    // Constructor to initialize a troop
    public void InitializeTroop(MovementType movement, AttackType attack,int hp, int atk, int def, int res,bool isPlayer)
    {
        troopMovementType = movement;
        Attack = attack;
        ATK = atk;
        DEF = def;
        RES = res;
        IsPlayer = isPlayer;
        HP = hp;
    }
    public void SetInitialTile(Tile initialTile)
    {
        currentTile = initialTile;
        transform.position = initialTile.transform.position;
        initialTile.OccupyTile(this);
    }
    void Start()
    {
        currentHP = HP;
    }

    // Method to equip an item
    public void EquipItem(Item item)
    {
        equippedItem = item;

        // Apply item bonuses to stats
        HP += equippedItem.bonusHP;
        ATK += equippedItem.bonusATK;
        DEF += equippedItem.bonusDEF;
        RES += equippedItem.bonusRES;
        
    }

    // Method to calculate damage based on attack type
    private int CalculateDamage(Troop enemyTroop)
    {
        int enemyDefense = (Attack == AttackType.Physical) ? enemyTroop.DEF : enemyTroop.RES;
        int damage = Mathf.Max(0, ATK - enemyDefense);
        return damage;
    }
    public bool CanAttack(Troop enemyTroop)
    {
        // Check if the enemy troop is an opponent
        if (IsEnemyTroop(enemyTroop))
        {
            return true;
        }

        return false;
    }
    public void AttackEnemy(Troop enemyTroop)
    {
        int damage = CalculateDamage(enemyTroop);

        // Apply damage to the enemy troop
        enemyTroop.TakeDamage(damage);
    }
    public void TakeDamage(int damage)
    {
        if (damage < 0)
        {
            Debug.LogError("Damage value should be non-negative.");
            return;
        }

        // Subtract damage from HP
        currentHP -= damage;

        // Check if the troop is defeated
        if (currentHP <= 0)
        {
            Defeat();
        }
        else
        {
            Debug.Log($"Troop took {damage} damage. Current HP: {currentHP}");
        }
    }
    private void Defeat()
    {
        Debug.Log("Troop defeated!");
        Destroy(gameObject);
    }
    public void ResetMoveFlag()
    {
        hasMovedThisPhase = false;
    }

    // Method to check if the troop can move during the current phase
    public bool CanMoveThisPhase()
    {
        return !hasMovedThisPhase;
    }

    // Method to set the troop as having moved during the current phase
    public void SetMovedThisPhase()
    {
        hasMovedThisPhase = true;
    }


    public void MoveTroopToTile(Tile destinationTile)
    {
        if (IsPlayer && !GameManager.instance.IsTileInValidMovementRange(destinationTile))
        {
            Debug.Log("Destination is beyond the troop's movement range.");
            return;
        }

        // Move the troop to the destination tile
        transform.position = new Vector3(destinationTile.transform.position.x, destinationTile.transform.position.y, transform.position.z);

        // Update the current tile
        currentTile.VacateTile();
        destinationTile.OccupyTile(this);
        currentTile = destinationTile;

        Debug.Log($"Troop moved to tile {destinationTile.BoardPosition}");
    }


    void OnMouseDown()
    {
        //Update UI Details
        UIManager.instance.UpdateDetailUI(currentHP,ATK,DEF,RES);
        if (!hasMovedThisPhase && IsPlayer && GameManager.instance.GetCurrentPhase() == GamePhase.Player)
        {
            // Handle troop selection
            GameManager.instance.TroopSelected(this);
        }
    }
    public List<Tile> GetValidAttackRange()
    {
        List<Tile> validMovementRange = GetValidMovementRange();
        List<Tile> validAttackRange = new List<Tile>();

        foreach (Tile tile in validMovementRange)
        {
            // Loop through adjacent tiles based on the attack range
            List<Tile> adjacentTiles = GetAdjacentTiles(tile, AttackRange);

            foreach (Tile adjacentTile in adjacentTiles)
            {
                // Check if the adjacent tile is not already in the valid movement range
                if (!validMovementRange.Contains(adjacentTile) && !validAttackRange.Contains(adjacentTile))
                {
                    validAttackRange.Add(adjacentTile);
                }

            }
            // Add enemies within the movement range to the attack range
            if (tile.OccupyingTroop != null && tile.OccupyingTroop.IsEnemyTroop(this))
            {
                validAttackRange.Add(tile);
            }
        }

        return validAttackRange;
    }


    public List<Tile> GetValidMovementRange()
    {
        List<Tile> validMovementRange = new List<Tile>();

        switch (troopMovementType)
        {
            case MovementType.Infantry:
                validMovementRange = CalculateInfantryMovementRange();
                break;
            case MovementType.Cavalry:
                validMovementRange = CalculateCavalryMovementRange();
                break;
            case MovementType.Flier:
                validMovementRange = CalculateFlierMovementRange();
                break;
        }

        return validMovementRange;
    }
    private List<Tile> CalculateInfantryMovementRange()
    {
        List<Tile> validMovementRange = new List<Tile>();

        // Get adjacent tiles based on the normal move range
        List<Tile> adjacentTiles = GetAdjacentTiles(currentTile, MovementRange);

        foreach (var tile in adjacentTiles)
        {
            if (tile.IsPassableForTroop(this))
            {
                validMovementRange.Add(tile);
            }
        }

        return validMovementRange;
    }
    private List<Tile> CalculateCavalryMovementRange()
    {
        List<Tile> validMovementRange = new List<Tile>();

        // Get adjacent tiles based on the normal move range
        List<Tile> adjacentTiles = GetAdjacentTiles(currentTile, MovementRange);

        foreach (var tile in adjacentTiles)
        {
            // Check if the tile is passable for cavalry
            if (tile.IsPassableForTroop(this))
            {
                validMovementRange.Add(tile);
            }
        }

        return validMovementRange;
    }
    private List<Tile> CalculateFlierMovementRange()
    {
        List<Tile> validMovementRange = new List<Tile>();

        // Get adjacent tiles based on the normal move range
        List<Tile> adjacentTiles = GetAdjacentTiles(currentTile, MovementRange);

        foreach (var tile in adjacentTiles)
        {
            // Check if the tile is passable for fliers
            if (tile.IsPassableForTroop(this))
            {
                validMovementRange.Add(tile);
            }
        }

        return validMovementRange;
    }
    

    private List<Tile> GetAdjacentTiles(Tile currentTile, int moveRange)
    {
        List<Tile> adjacentTiles = new List<Tile>();

        Vector2Int currentPosition = currentTile.BoardPosition;

        // Define all possible directions (up, down, left, right, up-left, up-right, down-left, down-right)
        Vector2Int[] directions =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(-1, 1), new Vector2Int(1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1)
        };

        // Loop through each direction
        foreach (Vector2Int direction in directions)
        {
            int maxMove = moveRange;

            // If moving diagonally, reduce the move range by 1
            if (Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1)
            {
                maxMove--;
            }

            // Check if the troop can move in the current direction
            for (int i = 1; i <= maxMove; i++)
            {
                // Calculate the adjacent position in the current direction
                Vector2Int adjacentPosition = currentPosition + direction * i;

                // Check if the adjacent position is within the board boundaries
                if (IsPositionInsideBoard(adjacentPosition))
                {
                    Tile adjacentTile = Board.instance.GetTileAtPosition(adjacentPosition);

                    // Check if the adjacent tile is not null, not occupied, and is passable for the troop
                    if (adjacentTile != null && (!adjacentTile.Occupied || (adjacentTile.OccupyingTroop != null && IsEnemyTroop(adjacentTile.OccupyingTroop))) && adjacentTile.IsPassableForTroop(this))
                    {
                        adjacentTiles.Add(adjacentTile);

                        // If the adjacent tile is a Forest, reduce the move range by 1
                        if (adjacentTile.Type == TileType.Forest && troopMovementType == MovementType.Infantry)
                        {
                            maxMove--;
                        }
                    }
                    else
                    {
                        // If the tile is occupied, out of bounds, or not passable, break the loop for this direction
                        break;
                    }
                }
                else
                {
                    // If the adjacent position is out of bounds, break the loop for this direction
                    break;
                }
            }
        }

        return adjacentTiles;
    }
    public bool InAttackRange(Tile targetTile)
    {
        return GetValidAttackRange().Contains(targetTile);
    }
    private bool IsEnemyTroop(Troop troop)
    {
        return troop.IsPlayer != this.IsPlayer; 
    }


    private bool IsPositionInsideBoard(Vector2Int position)
    {
        Board board = FindObjectOfType<Board>();  

        if (board != null)
        {
            int boardWidth = board.boardWidth;
            int boardHeight = board.boardHeight;

            return position.x >= 0 && position.x < boardWidth &&
                position.y >= 0 && position.y < boardHeight;
        }

        Debug.LogError("Board script not found!");
        return false;
    }
}
