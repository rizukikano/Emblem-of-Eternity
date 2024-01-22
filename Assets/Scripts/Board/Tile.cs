using UnityEngine;
public enum TileType
{
    Plains,
    Forest,
    Mountain,
    Ruins
}
public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private GameObject highlightMoveRange;
    [SerializeField] private GameObject highlightAttackRange;

    public bool Occupied { get; set; }
    public Troop OccupyingTroop { get; private set; }
    public Vector2Int BoardPosition { get; private set; }
    public TileType Type { get; private set; }

    void Awake()
    {
        Occupied = false;
    }
    public void OccupyTile(Troop troop)
    {
        OccupyingTroop = troop;
        Occupied = true;
    }

    public void VacateTile()
    {
        OccupyingTroop = null;
        Occupied = false;
    }

    public void Init(TileType type, Vector2Int boardPosition)
    {
        Type = type;
        BoardPosition = boardPosition;

        // Adjust properties based on the tile type if needed
        switch (Type)
        {
            case TileType.Plains:
                renderer.color = Color.white;
                break;
            case TileType.Forest:
                renderer.color = Color.green;
                break;
            case TileType.Mountain:
                renderer.color = Color.grey;
                break;
            case TileType.Ruins:
                renderer.color = Color.red;
                break;
            default:
                break;
        }
    }
    public bool IsPassableForTroop(Troop troop)
    {
        switch (Type)
        {
            case TileType.Plains:
                // Plains are traversable for everyone
                return true;
            case TileType.Forest:
                // Forest reduces infantry movement by 1, impassable for cavalry but unhindered for fliers
                if (troop.TroopMovementType == MovementType.Infantry)
                {
                    // Reduce infantry movement by 1
                    return troop.MovementRange > 1;
                }
                else if (troop.TroopMovementType == MovementType.Cavalry)
                {
                    // Impassable for cavalry
                    return false;
                }
                else if (troop.TroopMovementType == MovementType.Flier)
                {
                    // Unhindered for fliers
                    return true;
                }
                break;
            case TileType.Mountain:
                // Mountain is traversable for fliers only
                return troop.TroopMovementType == MovementType.Flier;
            case TileType.Ruins:
                // Ruins are impassable for all unit types
                return false;
            default:
                break;
        }

        // Default to impassable
        return false;
    }


    void OnMouseDown()
    {
        GameManager.instance.TileClicked(this);
    }

    public void HighlightMoveRange()
    {
        highlightMoveRange.SetActive(true);
    }

    public void ClearMoveRangeHighlight()
    {
        highlightMoveRange.SetActive(false);
    }
    public void HighlightAttackRange()
    {

        highlightAttackRange.SetActive(true);
    }

    public void ClearAttackRangeHighlight()
    {

        highlightAttackRange.SetActive(false);
    }

}
