using System.Collections.Generic;
namespace Tmpa {
	public class EachArray2dItem<Value2> {
		public readonly Value2[,] Array;
		public Vector2d Index;
		public readonly Vector2d IndexMaximum;

		public EachArray2dItem(Value2[,] array, Vector2d firstIndex = default(Vector2d) /*zero*/) {
			Array = array;
			Index = firstIndex;
			IndexMaximum = Tmpa.Index.GetMaximum(array.GetSize()); }

		public int X { get { return Index.X; } set { Index = Index.WithX(value); } }
		public int Y { get { return Index.Y; } set { Index = Index.WithY(value); } }

		public int XMaximum { get { return IndexMaximum.X; } }
		public int YMaximum { get { return IndexMaximum.Y; } }

		public bool IsNil { get { return !IsntNil; } }
		public bool IsntNil { get { return Array.ContainsIndex(Index); } }

		public Value2 Value { get { return Array.GetValue(Index); } set { Array.SetValue(Index, value); } }

		public IEnumerable<Value2> Remainder { get { for(; IsntNil; BeNext()) yield return Value; } }
		public IEnumerable<Value2> RowRemainder { get { for(var y = Y; Y == y && IsntNil; BeNext()) yield return Value; } }

		public void BeNext() { if(X < XMaximum) X++; else BeFirstOfNextRow(); }
		public void BeFirstOfNextRow() {
			X = 0;
			Y++; } } }