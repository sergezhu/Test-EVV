namespace Code.MergeSystem
{
	using UnityEngine;
	using Utilities.Extensions;

	public class CellsBounds : MonoBehaviour
	{
		public Vector3 CalculatePosition(int posX, int posY, Vector2Int boardSize,Vector2 cellSize, Vector2 cellSpace)
		{
			Vector2 boundsSize = transform.localScale.xz();
			
			float totalCellsSizeX = boardSize.x * cellSize.x + (boardSize.x - 1) * cellSpace.x;
			float totalCellsSizeY = boardSize.y * cellSize.y + (boardSize.y - 1) * cellSpace.y;
			
			float spaceX = posX == 0 ? 0 : cellSpace.x;
			float localPosX = (cellSize.x + spaceX) * posX - totalCellsSizeX * 0.5f + cellSize.x * 0.5f;
			
			float spaceY = posY == 0 ? 0 : cellSpace.y;
			float localPosY = (cellSize.y + spaceY) * posY - totalCellsSizeY * 0.5f + cellSize.y * 0.5f;
			
			Vector3 worldCenter = transform.position;
			Vector3 cellPosition = worldCenter + new Vector3(localPosX, 0, localPosY);
			
			return cellPosition;
		}
	}
}