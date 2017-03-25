using System.Collections.Generic;
namespace Tmpa {
	public interface Map<Tile> {
		Vector2d Size { get; }

		IReadOnlyList<Tile[,]> LayerTilesByLayerIndex { get; }
		IReadOnlyList<string> LayerNameOrNullByLayerIndex { get; }

		void InsertNewLayer(int layerIndex, Vector2d sizeMinimum, string layerNameOrNull = null);
		void RemoveLayer(int layerIndex);

		void Grow(int growthLength, Tile growthTile);

		void Save(string path);
		void Load(string path); } }