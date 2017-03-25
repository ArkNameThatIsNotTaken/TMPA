using System;
namespace Tmpa {
	public static class CSharp {
		public static Type GetOrError<Type>(Func<Type> get, Action error) { try { return get(); } catch(Exception) { error(); throw new Error("No exception was thrown."); } } } }