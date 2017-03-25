namespace Tmpa {
	public class Limit2d {
		public readonly Vector2d Lower, Upper;
		public static Limit2d NewLower(Vector2d lower) { return new Limit2d(lower, Vector2d.Maximum); }
		public static Limit2d NewUpper(Vector2d upper) { return new Limit2d(Vector2d.Minimum, upper); }
		public Limit2d(Vector2d lower, Vector2d upper) {
			Lower = lower;
			Upper = upper; }

		public bool Contains(Vector2d vector) {
			return
				vector.X >= Lower.X &&
				vector.Y >= Lower.Y &&
				vector.X <= Upper.X &&
				vector.Y <= Upper.Y; } } }