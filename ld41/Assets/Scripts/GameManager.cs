using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour {
	
	// Properties
	// =====================================================================

	public static GameManager i;
	public const int PLAYER_LAYER = 8;

	public Player player;

	public GridLayout grid;
	public Tilemap backgroundTilemap;

	// Unity
	// =====================================================================

	private void Awake ()
	{
		i = this;
		
		backgroundTilemap.CompressBounds();
	}
	
}
