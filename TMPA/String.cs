using System;
using System.Collections.Generic;
using System.IO;
namespace Tmpa {
	public static class String {
		public static string GetNice(string text) { return text == null ? "<null>" : "'" + text + "'"; }

		public static string[] Split(this string text, string separator) { return text.Split(new[] { separator }, StringSplitOptions.None); }
	
		public static IEnumerable<string> GetLines(this string text) {
			using(var reader = new StringReader(text)) {
				string line;
				while((line = reader.ReadLine()) != null) yield return line; } } } }