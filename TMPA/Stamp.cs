using System;
using System.Collections.Generic;
using System.Linq;
namespace Tmpa {
	public static class Stamp {
		public static IEnumerable<Stamp<Tile>> SelectTiles<Tile>(this IEnumerable<Stamp<Tile>> stamps, Func<Tile[,], Tile[,]> selectTiles) { return stamps.Select(stamp => stamp.WithTiles(selectTiles(stamp.Tiles))); } }
	public class Stamp<Tile> {
		public readonly uint RelativeLayerIndex;
		public readonly Tile/*immutable, can have nulls*/[,] Tiles;
		public Stamp(uint relativeLayerIndex, Tile/*immutable, can have nulls*/[,] tiles) {
			RelativeLayerIndex = relativeLayerIndex;
			Tiles = tiles; }

		public Stamp<Tile> WithTiles(Tile/*immutable, can have nulls*/[,] tiles) { return new Stamp<Tile>(RelativeLayerIndex, tiles); } } }