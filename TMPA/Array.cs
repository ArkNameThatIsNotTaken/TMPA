using System;
using System.Collections.Generic;
using System.Linq;
namespace Tmpa {
	public static class Array {
		public static Value[,] GetNew<Value>(Vector2d size) { return new Value[size.X, size.Y]; }

		public static Value2[,] GetCopy<Value, Value2>(this Value[,] array, Func<Value, Value2> getValue) { return array.GetCopy(index => index, getValue); }
		public static Value2[,] GetCopy<Value, Value2>(this Value[,] array, Func<Vector2d, Vector2d> getIndex, Func<Value, Value2> getValue) {
			var result = GetNew<Value2>(array.GetSize());
			foreach(var item in array.GetItems()) result.SetValue(getIndex(item.Index), getValue(item.Value));
			return result; }

		public static EachArray2dItem<Value> Each<Value>(this Value[,] array, Vector2d firstIndex = default(Vector2d) /*zero*/) { return new EachArray2dItem<Value>(array, firstIndex); }

		public static IEnumerable<EachArray2dItem<Value>> GetItems<Value>(this Value[,] array, Vector2d firstIndex = default(Vector2d) /*zero*/) { for(var item = array.Each(firstIndex); item.IsntNil; item.BeNext()) yield return item; }

		public static IEnumerable<Value> GetValues<Value>(this Value[,] array) { return array.GetItems().Select(item => item.Value); }

		public static IEnumerable<IEnumerable<Value>> GetRows<Value>(this Value[,] array) { for(var item = array.Each(); item.IsntNil;) yield return item.RowRemainder; }

		public static Value[,] GetCopy<Value>(this Value[,] array, Vector2d index, Vector2d size) { return array.GetCopy(new Limit2d(index, index + size)); }
		public static Value[,] GetCopy<Value>(this Value[,] array, Limit2d indexLimit) {
			var result = GetNew<Value>(indexLimit.Lower.GetFromTo(indexLimit.Upper).GetAtLeast(Vector2d.Zero));
			foreach(var resultItem in result.GetItems()) {
				var index = indexLimit.Lower + resultItem.Index;
				if(array.ContainsIndex(index)) resultItem.Value = array.GetValue(index); }
			return result; }

		public static Vector2d GetSize<Value>(this Value[,] array) { return new Vector2d(array.GetLength(0), array.GetLength(1)); }

		public static IEnumerable<Vector2d> GetIndices<Value>(this Value[,] array) { return array.GetItems().Select(item => item.Index); }

		public static bool ContainsIndex<Value>(this Value[,] array, Vector2d index) {
			var size = array.GetSize();
			return index.X >= 0 && index.Y >= 0 && index.X < size.X && index.Y < size.Y; }

		public static Value GetValue<Value>(this Value[,] array, Vector2d index) { return array[index.X, index.Y]; }
		public static Value GetValueOrGetDefault<Value>(this Value[,] array, Vector2d index) { return array.ContainsIndex(index) ? array.GetValue(index) : default(Value); }

		public static void WriteTo<Value>(this Value[,] array, Value[,] writee) { array.WriteTo(writee, Vector2d.Zero); }
		public static void WriteTo<Value>(this Value[,] array, Value[,] writee, Vector2d offset) {
			foreach(var item in array.GetItems()) {
				var index = item.Index + offset;
				if(writee.ContainsIndex(index)) writee.SetValue(index, item.Value); } }

		public static void FillWith<Value>(this Value[,] array, Value value) { foreach(var item in array.GetItems()) item.Value = value; }
		public static void FillWith<Value>(this Value[,] array, Value value, Vector2d offset, Vector2d size) { for(var y = 0; y < size.Y; y++) for(var x = 0; x < size.X; x++) array[offset.X + x, offset.Y + y] = value; }

		public static Value[,] GetXFlip<Value>(this Value[,] array) {
			var size = array.GetSize();
			var xMaximum = Index.GetMaximum(size.X);
			var result = GetNew<Value>(size);
			foreach(var resultItem in result.GetItems()) resultItem.Value = array[xMaximum - resultItem.X, resultItem.Y];
			return result; }
			
		public static Value[,] Grow<Value>(this Value[,] array, int growthLength, Value growthValue) {
			if(growthLength == 0) return array;
			if(growthLength < 0) return array.Shrink(growthLength);
			var growthSize = new Vector2d(growthLength);
			var size = array.GetSize();
			var nonGrowthResultIndexLimit = new Limit2d(lower: Vector2d.Zero + growthSize, upper: Index.GetMaximum(growthSize + size));
			var result = GetNew<Value>(growthSize + size + growthSize);
			foreach(var resultItem in result.GetItems()) resultItem.Value = nonGrowthResultIndexLimit.Contains(resultItem.Index) ? array.GetValue(resultItem.Index - growthSize) : growthValue;
			return result; }
		public static Value[,] Shrink<Value>(this Value[,] array, int shrinkLength) {
			if(shrinkLength == 0) return array;
			var shrinkSize = new Vector2d(shrinkLength);
			var result = GetNew<Value>(-shrinkSize + array.GetSize() - shrinkSize);
			foreach(var resultItem in result.GetItems()) resultItem.Value = array.GetValue(resultItem.Index + shrinkSize);
			return result; }

		public static Value[,] GetQuarterClockwiseTurn<Value>(this Value[,] array) {
			var size = array.GetSize();
			var yMaximum = Index.GetMaximum(size.Y);
			var result = new Value[size.Y, size.X];
			foreach(var item in array.GetItems()) result[yMaximum - item.Y, item.X] = item.Value;
			return result; }

		public static void SetValue<Value>(this Value[,] array, Vector2d index, Value value) { array[index.X, index.Y] = value; } } }