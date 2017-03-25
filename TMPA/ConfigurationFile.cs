using System.Collections.Generic;
using System.IO;
namespace Tmpa {
	public static class ConfigurationFile {
		public static readonly char CommentStart = '#';
		public static readonly char ParameterArgumentSeparator = ':';
		public static Dictionary<string, string> GetArgumentByParameter(string path) {
			var result = new Dictionary<string, string>();
			var lines = File.ReadAllLines(path);
			var lineIndex = -1;
			foreach(var line in lines) {
				lineIndex++;
				if(line == "" || line[0] == CommentStart) continue;
				var separatorIndex = line.IndexOf(ParameterArgumentSeparator);
				if(separatorIndex == -1) throw new Error("{0}: Line #{1} has no parameter-argument separator ('{2}').", path, Index.GetNice(lineIndex), ParameterArgumentSeparator);
				var parameter = line.Substring(0, Count.Before(separatorIndex));
				if(result.ContainsKey(parameter)) throw new Error("{0}: Line #{1} defines the parameter '{2}' that a line before already defined.", path, Index.GetNice(lineIndex), parameter);
				var argument = line.Substring(separatorIndex + 1);
				result.Add(parameter, argument); }
			return result; } } }