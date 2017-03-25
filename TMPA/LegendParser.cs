using System;
using System.Collections.Generic;
using System.Linq;
namespace Tmpa {
	public class LegendParser<Tile> {
		public readonly int Version;

		public readonly Tile EmptyTile;
		public readonly Tile NoDefinitionTile;
		public readonly IReadOnlyDictionary<Tile, int> NumberByTile;
		public readonly IReadOnlyList<Tile> OwnLegendTiles;

		private readonly EachArray2dItem<Tile> tile_;

		public LegendParser(Tile[,] tiles) {
			tile_ = tiles.Each();

			var versionTiles = tile_.Remainder.Take(5).GetCache();
			tile_.BeNext();
		
			EmptyTile = GetNextTile("empty");
			NoDefinitionTile = GetNextTile("no-definition");
			NumberByTile = ParseNumberByTile("number", 0, NoDefinitionTile);
			OwnLegendTiles = new[] { EmptyTile, NoDefinitionTile }.Concat(NumberByTile.Keys).GetCache();
		
			var digitIndex = -1;
			var digitFactor = 1;
			Version = versionTiles.Reverse().Aggregate(0, (accumulate, tile) => {
				digitIndex++;
				try { accumulate += NumberByTile[tile] * digitFactor; }
				catch(KeyNotFoundException) { throw new Error("Could not parse version digit {0}.", Index.GetNice(digitIndex)); }
				digitFactor *= 10;
				return accumulate; }); }
	
		public Tile GetNextTile(string name) {
			if(tile_.IsNil) throw new Error("Legend layer defines no {0} tile at {1}.", name, tile_.Index);
			var result = tile_.Value;
			Console.WriteLine("Legend defines {0} as {1}.", result, name);
			tile_.BeNext();
			return result; }

		public IReadOnlyDictionary<Tile, int> ParseNumberByTile(string name, int firstNumber, Tile terminator) {
			var numberByTile = new Dictionary<Tile, int>();
			var nextNumber = firstNumber;
			foreach(var tile in tile_.Remainder.TakeWhile(tile => !Equals(tile, terminator))) numberByTile.Add(tile, nextNumber++);
			tile_.BeNext();
			if(numberByTile.Count == 0) throw new Error("Legend defines no {0}.", name);
			Console.WriteLine("Legend defines {0} from {1} to {2}.", name, firstNumber, nextNumber - 1);
			return numberByTile; } } }