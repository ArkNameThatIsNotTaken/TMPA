using System.IO;
namespace Tmpa {
	public static class Assembly {
		public static string GetEmbeddedString(this System.Reflection.Assembly assembly, string id) { return new StreamReader(assembly.GetManifestResourceStream(id)).ReadToEnd(); } } }