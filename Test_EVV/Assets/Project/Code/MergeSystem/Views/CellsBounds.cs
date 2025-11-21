namespace Code.MergeSystem
{
	using UnityEngine;

	public class CellsBounds : MonoBehaviour
	{
		public Vector3 CalculatePosition(int posX, int posY, Vector2Int boardSize,Vector2 cellSize, Vector2 cellSpace)
		{
			float spaceX = posX == 0 ? 0 : cellSpace.x;
			float localPosX = (posX - (boardSize.x - cellSize.x) * 0.5f) + (cellSize.x + spaceX) * posX;
			
			float spaceY = posY == 0 ? 0 : cellSpace.y;
			float localPosY = (posY - (boardSize.y - cellSize.y) * 0.5f) + (cellSize.y + spaceY) * posY;
			
			Vector3 worldCenter = transform.position;
			Vector3 cellPosition = worldCenter + new Vector3(localPosX, 0, localPosY);
			
			return cellPosition;
		}
	}
}