using System;
namespace Tmpa {
	public struct Vector2d {
		public static readonly Vector2d Zero = new Vector2d(xAndY: 0);
		public static readonly Vector2d One = new Vector2d(xAndY: 1);
		public static readonly Vector2d Minimum = new Vector2d(xAndY: int.MinValue);
		public static readonly Vector2d Maximum = new Vector2d(xAndY: int.MaxValue);

		public readonly int X, Y;
		public Vector2d(int x, int y) {
			X = x;
			Y = y; }
		public Vector2d(int xAndY) : this(x: xAndY, y: xAndY) { }

		public static Vector2d Import(string exported) {
			Action<string> error = msg => { throw new Error("Importing '" + exported + "' failed: " + msg); };
			if(exported[0] != 'X') error("Expected 'X' at the beginning.");
			var exportedStubs = exported.Split('Y');
			if(exportedStubs.Length != 2) error("Expected one 'Y'.");
			return new Vector2d(
				x: CSharp.GetOrError(() => int.Parse(exportedStubs[0].Substring(1)), () => error("Could not parse X.")),
				y: CSharp.GetOrError(() => int.Parse(exportedStubs[1]), () => error("Could not parse Y."))); }
		
		public static Vector2d operator+(Vector2d xy, Vector2d summand) { return new Vector2d(xy.X + summand.X, xy.Y + summand.Y); }
		public static Vector2d operator-(Vector2d xy, Vector2d subtrahend) { return new Vector2d(xy.X - subtrahend.X, xy.Y - subtrahend.Y); }
		public static Vector2d operator-(Vector2d xy) { return new Vector2d(-xy.X, -xy.Y); }

		public Vector2d WithX(int x) { return new Vector2d(x, Y); }
		public Vector2d WithY(int y) { return new Vector2d(X, y); }

		public Vector2d GetFromTo(Vector2d other) { return other - this; }

		public Vector2d GetAtLeast(Vector2d other) { return new Vector2d(Math.Max(X, other.X), Math.Max(Y, other.Y)); }

		public Vector2d GetFirst(Vector2d other) { return (Y == other.Y && other.X < X) || other.Y < Y ? other : this; }

		public bool Exceeds(Limit2d limit) { return !limit.Contains(this); }

		public override string ToString() { return "X" + X + "Y" + Y; } } }