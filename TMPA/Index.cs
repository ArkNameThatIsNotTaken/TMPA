using System.Collections.Generic;
namespace Tmpa {
	public static class Index {
		public static IEnumerable<Vector2d> RelativeNeighbourIndices2d {
			get {
				var selfIndex = Vector2d.One;
				var isSelfByIndex = Array.GetNew<bool>(new Vector2d(xAndY: 3));
				isSelfByIndex.SetValue(selfIndex, true);
				foreach(var item in isSelfByIndex.GetItems()) if(!item.Value) yield return item.Index - selfIndex; } }

		public static string GetNice(int index) { return "#" + (index >= 0 ? index + 1 : index); }

		public static int GetAfter(int count) { return count; }

		public static int GetMaximum(int count) { return count - 1; }
		public static Vector2d GetMaximum(Vector2d size) { return size - Vector2d.One; } } }