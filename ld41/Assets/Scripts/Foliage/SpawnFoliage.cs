using UnityEngine;
using UnityEngine.Tilemaps;

namespace Foliage
{
	public class SpawnFoliage : MonoBehaviour
	{
		
		// Properties
		// =====================================================================

		public GameObject grass;

		private GridLayout _grid;
		private Tilemap _tilemap;
		private BoundsInt _bounds;

		private Transform _grassParent;
		
		// Unity
		// =====================================================================

		private void Awake ()
		{
			GameObject grassParent = new GameObject {name = "Grass"};
			_grassParent = grassParent.transform;
		}

		private void Start ()
		{
			_grid = GameManager.i.grid;
			_tilemap = GameManager.i.backgroundTilemap;
			
			_bounds = _tilemap.cellBounds;
			TileBase[] tiles = _tilemap.GetTilesBlock(_bounds);

			for (int x = 0; x < _bounds.size.x; x++)
			{
				for (int y = 0; y < _bounds.size.y; y++)
				{
					TileBase tile = tiles[x + y * _bounds.size.x];
					
					if (tile == null)
						continue;

					switch (tile.name)
					{
						case "Grass":
							SpawnGrass(x, y);
							break;
					}
				}
			}
		}
		
		// Actions
		// =====================================================================

		private void SpawnGrass (int x, int y)
		{
			Vector3 worldPos = GetCellWorldPosition(x, y);

			int bladesTotal = Random.Range(5, 10);
			for (int i = 0; i < bladesTotal; i++)
			{
				GameObject blade = Instantiate(grass, _grassParent);
				blade.transform.position = new Vector3(
					worldPos.x + Random.Range(0f, 1f),
					worldPos.y + 1,
					-1
				);
				
				blade.transform.localScale = new Vector3(
					Random.Range(0.75f, 1f),
					Random.Range(0.5f, 1f),
					1
				);
			}
		}
		
		// Helpers
		// =====================================================================

		private Vector3 GetCellWorldPosition (int x, int y)
		{
			return _grid.CellToWorld(new Vector3Int(x, y, 0)) + _bounds.position;
		}
		
	}
}
