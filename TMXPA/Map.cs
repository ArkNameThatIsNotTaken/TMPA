using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Tmpa;
namespace Tmxpa {
	public class Map : Map<int> {
		private static readonly string nodeName_ = "map";
		private static readonly string layerNodeName_ = "layer";
		private static readonly string layerNameAttributeName_ = "name";
		private static readonly string layerWidthAttributeName_ = "width";
		private static readonly string layerHeightAttributeName_ = "height";
		private static readonly string layerDataNodeName_ = "data";
		private static readonly string layerDataEncodingAttributeName_ = "encoding";
		private static readonly string csvLayerDataEncodingName_ = "csv";

		private XDocument xml_;
		private XElement node_;
		private readonly List<XElement> layerNodeByLayerIndex_ = new List<XElement>();
		private readonly List<int[,]> layerTilesByLayerIndex_ = new List<int[,]>();
		private readonly List<string> layerNameOrNullByLayerIndex_ = new List<string>();

		public Vector2d Size { get; private set; }
		private Limit2d upperLayerSizeMinimumLimit_ { get { return Limit2d.NewUpper(Size); } }

		public IReadOnlyList<int[,]> LayerTilesByLayerIndex { get { return layerTilesByLayerIndex_; } }
		public IReadOnlyList<string> LayerNameOrNullByLayerIndex { get { return layerNameOrNullByLayerIndex_; } }

		private void registerLayer_(int layerIndex, XElement layerNode, int[,] layerTiles, string layerNameOrNull) {
			layerNodeByLayerIndex_.Insert(layerIndex, layerNode);
			layerTilesByLayerIndex_.Insert(layerIndex, layerTiles);
			layerNameOrNullByLayerIndex_.Insert(layerIndex, layerNameOrNull); }

		public void InsertNewLayer(int layerIndex, Vector2d sizeMinimum, string layerNameOrNull = null) {
			if(sizeMinimum.Exceeds(upperLayerSizeMinimumLimit_)) throw new Error("Trying to insert new layer ({0}) that is too big (size: {1}, map size is: {2}) at {3}.", layerNameOrNull ?? "unnamed", sizeMinimum, Size, Index.GetNice(layerIndex));
			XElement layerNode, layerDataNode;
			{	layerDataNode = new XElement(layerDataNodeName_);
				layerDataNode.SetAttributeValue(layerDataEncodingAttributeName_, csvLayerDataEncodingName_);
				layerNode = new XElement(layerNodeName_, layerDataNode);
				if(layerNameOrNull != null) layerNode.SetAttributeValue(layerNameAttributeName_, layerNameOrNull);
				layerNode.SetAttributeValue(layerWidthAttributeName_, Size.X);
				layerNode.SetAttributeValue(layerHeightAttributeName_, Size.Y); }
			registerLayer_(layerIndex, layerNode, Tmpa.Array.GetNew<int>(Size), layerNameOrNull);
			node_.Insert(layerNodeName_, layerIndex, layerNode); }
		public void RemoveLayer(int layerIndex) {
			layerNodeByLayerIndex_[layerIndex].Remove();
			layerNodeByLayerIndex_.RemoveAt(layerIndex);
			layerTilesByLayerIndex_.RemoveAt(layerIndex);
			layerNameOrNullByLayerIndex_.RemoveAt(layerIndex); }

		public void Grow(int growthLength, int growthTile) {
			var layerIndex = -1;
			foreach(var layerTiles in layerTilesByLayerIndex_.ToList()) layerTilesByLayerIndex_[++layerIndex] = layerTiles.Grow(growthLength, growthTile);
			var growthSize = new Vector2d(xAndY: growthLength);
			Size = growthSize + Size + growthSize; }

		public void Save(string path) {
			var layerIndex = -1;
			var newLine = Environment.NewLine;
			foreach(var layerNode in layerNodeByLayerIndex_) {
				layerIndex++;
				layerNode.Element(layerDataNodeName_).Value = newLine + layerTilesByLayerIndex_[layerIndex].GetRows().Select(row => row.Join(",")).Join("," + newLine);
				layerNode.Attribute(layerWidthAttributeName_).Value = Size.X.ToString();
				layerNode.Attribute(layerHeightAttributeName_).Value = Size.Y.ToString(); }
			xml_.Save(path); }
		public void Load(string path) {
			{	var xml = XDocument.Load(path);
				XElement node; if((node = xml.Element(nodeName_)) == null) throw new Error("File contains no <" + nodeName_ + "> node.");
				xml_ = xml;
				node_ = node; }
			layerNodeByLayerIndex_.Clear();
			layerTilesByLayerIndex_.Clear();
			layerNameOrNullByLayerIndex_.Clear();
			Size = Vector2d.Zero;
			foreach(var layerNode in node_.Elements(layerNodeName_)) {
				var layerIndex = Index.GetAfter(layerNodeByLayerIndex_.Count);
				var layerNameNodeOrNull = layerNode.Attribute(layerNameAttributeName_);
				var layerNameOrNull = layerNameNodeOrNull == null ? null : layerNameNodeOrNull.Value;
				Action<string> error = message => { throw new Error("Layer {0} ({1}): {2}", Index.GetNice(layerIndex), layerNameOrNull ?? "unnamed", message); };
				var layerWidth = -1;
				var layerHeight = -1;
				foreach(var length in new[] {
					new {
						AttributeName = layerWidthAttributeName_,
						Set = new Action<int>(value => {
							layerWidth = value;
							Size = Size.WithX(Math.Max(Size.X, layerWidth)); }) },
					new {
						AttributeName = layerHeightAttributeName_,
						Set = new Action<int>(value => {
							layerHeight = value;
							Size = Size.WithY(Math.Max(Size.Y, layerHeight)); }) } }) {
					var resultAttributeOrNull = layerNode.Attribute(length.AttributeName);
					var result = -1;
					if(resultAttributeOrNull == null || !int.TryParse(resultAttributeOrNull.Value, out result) || result < 0) error("Could not parse the <" + layerNameAttributeName_ + " " + length.AttributeName + "> attribute.");
					length.Set(result); }
				var layerTiles = new int[0, 0];
				try { layerTiles = new int[layerWidth, layerHeight]; }
				catch(ArgumentOutOfRangeException) { error("Could not allocate specified size (" + new Vector2d(layerWidth, layerHeight) +")."); }
				XElement layerDataNode;
				if((layerDataNode = layerNode.Element(layerDataNodeName_)) == null) error("Missing <" + layerDataNodeName_ + "> node.");
				XAttribute layerDataEncodingAttribute;
				if((layerDataEncodingAttribute = layerDataNode.Attribute(layerDataEncodingAttributeName_)) == null) error("Missing <" + layerDataNodeName_ + " " + layerDataEncodingAttributeName_ + "> attribute.");
				if(layerDataEncodingAttribute.Value != csvLayerDataEncodingName_) error("Only '" + csvLayerDataEncodingName_ + "' encoding supported (uses '" + layerDataEncodingAttribute.Value + "' encoding).");
				{	var y = -1;
					foreach(var row in layerDataNode.Value.GetLines()) {
						if(row == "") continue;
						y++;
						if(y > Index.GetMaximum(Size.Y)) break;
						var x = -1;
						foreach(var cell in row.Split(",")) {
							x++;
							if(x > Index.GetMaximum(Size.X)) break;
							int tile;
							if(!int.TryParse(cell, out tile)) error("Could not parse tile '" + cell + "' at " + new Vector2d(x, y) + ".");
							layerTiles[x, y] = tile; } } }
				registerLayer_(layerIndex, layerNode, layerTiles, layerNameOrNull); } } } }