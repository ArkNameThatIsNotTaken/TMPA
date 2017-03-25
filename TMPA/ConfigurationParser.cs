using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
namespace Tmpa {
	public class ConfigurationParser<Tile> {
		public struct TileTheme {
			public readonly int ThemesIndex;
			public readonly int ThemeIndex;
			public readonly int TileIndex;
			public TileTheme(int themesIndex, int themeIndex, int tileIndex) {
				ThemesIndex = themesIndex;
				ThemeIndex = themeIndex;
				TileIndex = tileIndex; } }

		public readonly Tile NoDefinitionTile;

		public readonly Tile WallsDeclarationTile;
		public readonly Tile CornersDeclarationTile;
		public readonly Tile ThemesDeclarationTile;
		public readonly IReadOnlyList<Tile> ModifierDeclarationTiles;

		public readonly Tile PlainPatternDeclarationTile;
		public readonly Tile ThemedPatternDeclarationTile;
		public readonly Tile ThemedDynamicPatternDeclarationTile;
		public readonly Tile DynamicPatternDeclarationTile;
		public readonly IReadOnlyList<Tile> PatternDeclarationTiles;

		public readonly IReadOnlyDictionary<Tile, int> NumberByTile;

		public readonly IReadOnlyList<Tile> OwnLegendTiles;

		public readonly IReadOnlyDictionary<Tile, bool> IsThemedByDeclarationTile;
		public readonly IReadOnlyDictionary<Tile, bool> IsDynamicByDeclarationTile;

		public IReadOnlyList<Tile[/*no write*/, /*no write*/]> TilesByIndex;
		public IReadOnlyList<string> NameByIndex;
		private IReadOnlyList<bool[,]> isParsedEachTileByIndex_;

		private string name_;
		private EachArray2dItem<Tile> tile_;
		private bool[,] isParsedEachTile_;
		private Vector2d definitionIndex_;
		private string parseeName_;
		private Vector2d areaSize_;
		private Action<string> error_;

		private readonly Dictionary<Tile, Tile> tileXFlipByTile_ = new Dictionary<Tile, Tile>();
		private readonly Dictionary<Tile, Tile> tileClockwiseQuarterTurnByTile_ = new Dictionary<Tile, Tile>();

		private readonly List<List<List<Tile>>> manyTileByTileIndexByThemeIndex_ = new List<List<List<Tile>>>();
		private readonly Dictionary<Tile, TileTheme> tileThemeByTile_ = new Dictionary<Tile, TileTheme>();

		public ConfigurationParser(LegendParser<Tile> legendParser) {
			Func<string, Tile> getNextDeclarationTile = name => legendParser.GetNextTile(name + " declaration");

			NoDefinitionTile = legendParser.NoDefinitionTile;

			WallsDeclarationTile = getNextDeclarationTile("walls");
			CornersDeclarationTile = getNextDeclarationTile("corners");
			ThemesDeclarationTile = getNextDeclarationTile("themes");
			ModifierDeclarationTiles = new[] { WallsDeclarationTile, CornersDeclarationTile, ThemesDeclarationTile }.GetCache();

			PlainPatternDeclarationTile = getNextDeclarationTile("plain pattern");
			ThemedPatternDeclarationTile = getNextDeclarationTile("themed pattern");
			ThemedDynamicPatternDeclarationTile = getNextDeclarationTile("themed dynamic pattern");
			DynamicPatternDeclarationTile = getNextDeclarationTile("dynamic pattern");
			PatternDeclarationTiles = new[] { PlainPatternDeclarationTile, ThemedPatternDeclarationTile, ThemedDynamicPatternDeclarationTile, DynamicPatternDeclarationTile }.GetCache();

			NumberByTile = legendParser.NumberByTile;

			OwnLegendTiles = ModifierDeclarationTiles.Concat(PatternDeclarationTiles).GetCache();

			IsThemedByDeclarationTile = new Dictionary<Tile, bool> {
				{ PlainPatternDeclarationTile, false },
				{ ThemedPatternDeclarationTile, true },
				{ ThemedDynamicPatternDeclarationTile, true },
				{ DynamicPatternDeclarationTile, false } };
			IsDynamicByDeclarationTile = new Dictionary<Tile, bool> {
				{ PlainPatternDeclarationTile, false },
				{ ThemedPatternDeclarationTile, false },
				{ ThemedDynamicPatternDeclarationTile, true },
				{ DynamicPatternDeclarationTile, true } }; }
	
		private Tile[,] tiles_ { get { return tile_.Array; } }
		private Tile declarationTile_ { get { return tiles_.GetValue(definitionIndex_); } }

		private int nextNumberParse_ {
			get {
				parseeName_ = parseeName_ ?? "number";
				var number = -1;
				if(tile_.IsNil || !NumberByTile.TryGetValue(tile_.Value, out number)) error_("");
				markTileParsed_();
				tile_.Y++;
				parseeName_ = null;
				return number; } }
		private Tile[,] nextAreaParse_ {
			get {
				parseeName_ = parseeName_ ?? "area";
				tile_.X++;
				tile_.Y = definitionIndex_.Y;
				var result = tiles_.GetCopy(tile_.Index, areaSize_);
				if(!Equals(result.GetSize(), areaSize_)) error_("Part is out of range.");
				markTilesParsed_(areaSize_);
				tile_.X += areaSize_.X;
				parseeName_ = null;
				return result; } }

		private void parse_(Func<Tile, bool> isDeclarationTile, Action parseDefinition) {
			var index = -1;
			error_ = message => { throw new Error("{0}: Couldn't parse {1} at {2} (tile: {3}). {4}", name_, parseeName_, tile_.Index, tile_.IsNil ? "out of range" : tile_.Value.ToString(), message); };
			foreach(var tiles in TilesByIndex) {
				index++;
				name_ = NameByIndex[index];
				isParsedEachTile_ = isParsedEachTileByIndex_[index];
				for(tile_ = tiles.Each(); tile_.IsntNil; tile_.BeNext())
					if(isDeclarationTile(tile_.Value)) {
						markTileParsed_();
						definitionIndex_ = tile_.Index;
						parseeName_ = null;
						areaSize_ = Vector2d.Zero;
						tile_.Y++;
						parseDefinition();
						tile_.Index = definitionIndex_; } } }
		private void parseAreaSize_() {
			parseeName_ = "width";
			var width = nextNumberParse_;
			parseeName_ = "height";
			var height = nextNumberParse_;
			areaSize_ = new Vector2d(width, height); }
		private Tile getTileParse_(int x, int y, string name = "tile") {
			parseeName_ = name;
			var formerIndex = tile_.Index;
			tile_.Index = definitionIndex_ + new Vector2d(x, y);
			if(tile_.IsNil) error_("");
			markTileParsed_();
			var result = tile_.Value;
			tile_.Index = formerIndex;
			parseeName_ = null;
			return result; }
		private void markTileParsed_() { isParsedEachTile_.SetValue(tile_.Index, true); }
		private void markTilesParsed_(Vector2d size) { isParsedEachTile_.FillWith(true, tile_.Index, size); }

		public IReadOnlyList<Pattern<Tile>> ParseAndGetPatterns() {
			isParsedEachTileByIndex_ = TilesByIndex.Select(tiles => Array.GetNew<bool>(tiles.GetSize())).GetCache();

			parseTileModifiers_();

			var patterns = new List<Pattern<Tile>>();
			Action<string, Tile[,], IReadOnlyList<Stamp<Tile>>> registerPattern;
			{	var nextPrecedence = uint.MaxValue;
				registerPattern = (nameAddition, tiles, stamps) => patterns.AddRange(getPatterns_(name_ + "[" + definitionIndex_ + "]" + nameAddition, () => nextPrecedence--, tiles, stamps, IsThemedByDeclarationTile[declarationTile_])); }

			parse_(isDeclarationTile: PatternDeclarationTiles.Contains, parseDefinition: () => {
				parseAreaSize_();

				parseeName_ = "pattern";
				var tiles = nextAreaParse_;

				var stamps = new List<Stamp<Tile>>();
				while(tile_.IsntNil && NumberByTile.ContainsKey(tile_.Value)) {
					var relativeLayerIndex = nextNumberParse_;

					parseeName_ = "relative layer index";
					if(relativeLayerIndex < 0) error_("Index is negative.");
					parseeName_ = null;

					parseeName_ = "stamp";
					var stampTiles = nextAreaParse_;

					stamps.Add(new Stamp<Tile>((uint)relativeLayerIndex, stampTiles)); }
				stamps = stamps.OrderBy(stamp => stamp.RelativeLayerIndex).ToList();

				registerPattern("", tiles, stamps);

				if(!IsDynamicByDeclarationTile[declarationTile_]) return;

				Action<string> registerXFlip = nameAddition => registerPattern(nameAddition, getXFlip_(tiles), stamps.SelectTiles(getXFlip_).GetCache());
				Action<string> turnAQuarterClockwiseAndRegister = nameAddition => {
					tiles = getQuarterClockwiseTurn_(tiles);
					stamps = stamps.SelectTiles(getQuarterClockwiseTurn_).ToList();
					registerPattern(nameAddition, tiles, stamps); };
				registerXFlip("↔");
				turnAQuarterClockwiseAndRegister("↷");
				registerXFlip("↷↔");
				turnAQuarterClockwiseAndRegister("↻");
				registerXFlip("↻↔");
				turnAQuarterClockwiseAndRegister("↶");
				registerXFlip("↶↔"); });
			return patterns; }
		private void parseTileModifiers_() {
			tileXFlipByTile_.Clear();
			tileClockwiseQuarterTurnByTile_.Clear();
			manyTileByTileIndexByThemeIndex_.Clear();
			tileThemeByTile_.Clear();

			var parseDefinitionByDeclarationTile = new Dictionary<Tile, Action> {
				{ WallsDeclarationTile, parseWalls_ },
				{ CornersDeclarationTile, parseCorners_ },
				{ ThemesDeclarationTile, parseThemes_ } };
			parse_(isDeclarationTile: ModifierDeclarationTiles.Contains, parseDefinition: () => parseDefinitionByDeclarationTile[declarationTile_]()); }
		private void parseWalls_() {
			var upY = 0;
			var downY = 1;

			var leftX = 0;
			var middleX = 1;
			var rightX = 2;

			var up = getTileParse_(middleX, upY, "upper edge");
			var down = getTileParse_(middleX, downY, "lower edge");
			var left = getTileParse_(leftX, downY, "left edge");
			var right = getTileParse_(rightX, downY, "right edge");

			try {
				tileXFlipByTile_.AddBidirectionally(up, down);
				tileXFlipByTile_.AddBidirectionally(left, right);
				tileClockwiseQuarterTurnByTile_.AddChain(new[] { up, right, down, left }); }
			catch(ArgumentException) { error_("At least one tile was already registered as an edge (wall or corner)."); } }
		private void parseCorners_() {
			var upY = 0;
			var downY = 1;

			var leftX = 1;
			var rightX = 2;

			var upLeft = getTileParse_(leftX, upY, "up-left edge");
			var upRight = getTileParse_(rightX, upY, "up-right edge");
			var downLeft = getTileParse_(leftX, downY, "down-left edge");
			var downRight = getTileParse_(rightX, downY, "down-right edge");

			try {
				tileXFlipByTile_.AddBidirectionally(upLeft, upRight);
				tileXFlipByTile_.AddBidirectionally(downLeft, downRight);
				tileClockwiseQuarterTurnByTile_.AddChain(new[] { upLeft, upRight, downRight, downLeft }); }
			catch(ArgumentException) { error_("At least one tile has already been registered as an edge (wall or corner)."); } }
		private void parseThemes_() {
			parseAreaSize_();
			parseeName_ = "themes";
			var definition = nextAreaParse_;
			var tileByTileIndexByThemeIndex = new List<List<Tile>>();
			var themesIndex = Index.GetAfter(manyTileByTileIndexByThemeIndex_.Count);
			manyTileByTileIndexByThemeIndex_.Add(tileByTileIndexByThemeIndex);
			foreach(var tile in definition.GetItems()) {
				var themeIndex = tile.X;
				var tileIndex = tile.Y;
				if(tileIndex == 0) tileByTileIndexByThemeIndex.Add(new List<Tile>());
				tileByTileIndexByThemeIndex[themeIndex].Add(tile.Value);
				try { tileThemeByTile_.Add(tile.Value, new TileTheme(themesIndex, themeIndex, tileIndex)); }
				catch(ArgumentException) { error_("Tile " + tile.Value + " (" + definitionIndex_ + new Vector2d(1, 0) + tile.Index + ") has already been registered in a theme."); } } }

		private Tile[,] getXFlip_(Tile[,] tiles) { return tiles.GetXFlip().GetCopy(tileXFlipByTile_.GetValueOrKey); }
		private Tile[,] getQuarterClockwiseTurn_(Tile[,] tiles) { return tiles.GetQuarterClockwiseTurn().GetCopy(tileClockwiseQuarterTurnByTile_.GetValueOrKey); }

		private IEnumerable<Pattern<Tile>> getPatterns_(string name, Func<uint> getNextPrecedence, Tile[,] tiles, IReadOnlyList<Stamp<Tile>> stamps, bool allowThemes) {
			Func<string, Tile[,], IReadOnlyList<Stamp<Tile>>, Pattern<Tile>> newPattern;
			{	Func<Tile[,], Tile[,]> getWithoutNoDefinitionTiles = tiles2 => tiles2.GetCopy(tile => Equals(tile, NoDefinitionTile) ? default(Tile) : tile);
				newPattern = (nameAddition, tiles2, stamps2) => new Pattern<Tile>(name + nameAddition, getNextPrecedence(), getWithoutNoDefinitionTiles(tiles2), stamps.SelectTiles(getWithoutNoDefinitionTiles).GetCache()); }

			if(!allowThemes) return new[] { newPattern("", tiles, stamps) };

			var distinctTilesThemeInfos = new[] { tiles }.Concat(stamps.Select(stamp => stamp.Tiles))
				.SelectMany(Array.GetValues)
				.Where(tileThemeByTile_.ContainsKey)
				.Select(tileThemeByTile_.GetValue)
				.Select(tileTheme => new { ThemesIndex = tileTheme.ThemesIndex, ThemeIndex = tileTheme.ThemeIndex })
				.Distinct()
				.GetCache();

			if(distinctTilesThemeInfos.Count != 1) return new[] { newPattern("Multithemed", tiles, stamps) };

			var tilesThemeInfo = distinctTilesThemeInfos.Single();

			var themesIndex = tilesThemeInfo.ThemesIndex;
			var themes = manyTileByTileIndexByThemeIndex_[themesIndex];
			name += "Theme" + Index.GetNice(themesIndex);

			var tilesThemeIndex = tilesThemeInfo.ThemeIndex;
			var tilesTheme = themes[tilesThemeIndex];

			return themes.Select((theme, themeIndex) => {
				Func<Tile[,], Tile[,]> getThemed = tiles2 => {
					if(themeIndex == tilesThemeIndex) return tiles2;
					return tiles2.GetCopy(tile => {
						var tileIndex = tilesTheme.IndexOf(tile);
						return tileIndex == -1 ? tile : theme[tileIndex]; }); };
				return newPattern(Index.GetNice(themeIndex), getThemed(tiles), stamps.SelectTiles(getThemed).GetCache()); }); } } }
