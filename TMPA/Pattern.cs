using System.Collections.Generic;
namespace Tmpa {
	public class Pattern<Tile> {
		public readonly string Name;

		public readonly uint Precedence;

		public readonly Tile/*immutable, can have nulls*/[,] Tiles;
		public readonly Vector2d FirstTileIndex;
		public readonly Tile FirstTile;

		public readonly /*immutable*/ IReadOnlyList<Stamp<Tile>> Stamps; //Sorted by relative layer index then descendingly by precedence.

		public Pattern(string name, uint precedence, Tile/*immutable, can have nulls*/[,] tiles, /*immutable*/ IReadOnlyList<Stamp<Tile>> stamps /*Sorted by relative layer index then descendingly by precedence.*/) {
			Name = name;

			Precedence = precedence;

			Tiles = tiles;

			foreach(var tile in Tiles.GetItems())
				if(tile.Value != null) {
					FirstTileIndex = tile.Index;
					FirstTile = tile.Value;
					break; }
			if(FirstTile == null) throw new Error("{0} has no non-null tiles.", this);

			Stamps = stamps; }

		public override string ToString() { return "Pattern: " + Name; } } }