using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
namespace Tmpa {
	public static class ConsoleApplication {
		private delegate void registerParameter_(string name, string description, Action<string> setArgumentOrNull, string defaultArgumentOrNull = null, Action<string> executeOrNull = null);

		private static void write_(string message, params object[] messageArguments) { Console.WriteLine(message, messageArguments); }
		private static string read_(string message, params object[] messageArguments) {
			write_(message, messageArguments);
			return Console.ReadLine();  }

		public static void Process<Tile, Map>(string/*nowrite*/[] commandLineArguments) where Map : Map<Tile>, new() {
			var inputFilePath = "";
			var actionName = "";
			var bezelLayerName = "";
			var relativeConfigurationFilePath = "";
			var legendLayerName = "";
			var configurationLayerName = "";
			var yesIfEmbed = "";
			var relativeWorldFilePath = "";
			var unparsedPeekLength = "";
			var unformattedRelativeOutputFilePath = "";
			var yesIfOverwriteAllowedOptionallyAsk = "";
			var yesIfPause = "";

			var yes = "yes";

			yesIfPause = yes;

			var lineSeparator = Environment.NewLine;

			try {
				var executable = System.Reflection.Assembly.GetEntryAssembly();

				var filePath = AppDomain.CurrentDomain.FriendlyName;

				var version = 1;

				var help = executable.GetName().Name + " Version " + version + " help:" + lineSeparator;

				help += lineSeparator;

				var inputFilePathParameter = "in";

				var prepareActionName = "prepare";
				var applyPatternsActionName = "apply";
				var supportedActionNames = new[] { prepareActionName, applyPatternsActionName };

				var ask = "ask";

				help += "Command Line:" + lineSeparator + filePath + " (input file path) {<parameter name>(:<argument>)}" + lineSeparator;

				help += lineSeparator;

				var executeByParameter = new Dictionary<string, Action<string>>();

				help += "Parameters:" + lineSeparator;

				var defaultArgumentsFilePath = "Defaults.txt";
				var defaultArgumentsFileContent = "";
				var defaultArgumentByParameterName = new Dictionary<string, string>();
				if(File.Exists(defaultArgumentsFilePath)) {
					write_("Loading defaults file {0}...", defaultArgumentsFilePath);
					defaultArgumentByParameterName = ConfigurationFile.GetArgumentByParameter(defaultArgumentsFilePath);
					write_("Loaded file."); }
				registerParameter_ registerParameter = (string name, string description, Action<string> setArgumentOrNull, string defaultArgumentOrNull, Action<string> executeOrNull) => {
					executeByParameter.Add(name, executeOrNull ?? setArgumentOrNull); //Assumes that registration is correct (which results in a non-null here).
					if(defaultArgumentOrNull != null) {
						if(defaultArgumentByParameterName.ContainsKey(name)) defaultArgumentOrNull = defaultArgumentByParameterName[name];
						setArgumentOrNull(defaultArgumentOrNull); //Assumes that registration is correct (which results in a non-null here).
						defaultArgumentsFileContent += ConfigurationFile.CommentStart + description + lineSeparator;
						defaultArgumentsFileContent += name + ConfigurationFile.ParameterArgumentSeparator + defaultArgumentOrNull + lineSeparator;
						description += " Default: '" + defaultArgumentOrNull + "'"; }
					help += name + ": " + description + lineSeparator; };
				registerParameter(
					name: "help",
					description: "Print the help to the console.",
					setArgumentOrNull: null,
					defaultArgumentOrNull: null,
					executeOrNull: argument => write_(help));
				registerParameter(
					name: inputFilePathParameter,
					description: "Specify the input file path.",
					setArgumentOrNull: argument => inputFilePath = argument);
				registerParameter(
					name: "action",
					description: "Specify the action (available: " + supportedActionNames.Join(", ") + ").",
					setArgumentOrNull: argument => actionName = argument,
					defaultArgumentOrNull: applyPatternsActionName);
				defaultArgumentsFileContent += lineSeparator;
				registerParameter(
					name: "bezel",
					description: "Specify the name of the bezel layer that is added during preparation and removed during application. It is the layer that masks the peek into neighbouring maps.",
					setArgumentOrNull: argument => bezelLayerName = argument,
					defaultArgumentOrNull: "HIDE ENV");
				defaultArgumentsFileContent += lineSeparator;
				registerParameter(
					name: "config",
					description: "Specify the configuration file path relative to the input file (empty: use the one embedded in the input file).",
					setArgumentOrNull: argument => relativeConfigurationFilePath = argument,
					defaultArgumentOrNull: "");
				registerParameter(
					name: "legend",
					description: "Specify the name of the legend layer that is optionally embedded during preparation and removed during application.",
					setArgumentOrNull: argument => legendLayerName = argument,
					defaultArgumentOrNull: "LEGEND");
				registerParameter(
					name: "layer",
					description: "Specify the name of the configuration layer that is optionally embedded during preparation and removed during application.",
					setArgumentOrNull: argument => configurationLayerName = argument,
					defaultArgumentOrNull: "CONFIG");
				registerParameter(
					name: "embed",
					description: "'" + yes + "' to embed the configuration during preparation, otherwise anything else. Should be '" + yes + "' unless the configuration is too big for the map.",
					setArgumentOrNull: argument => yesIfEmbed = argument,
					defaultArgumentOrNull: "yes");
				defaultArgumentsFileContent += lineSeparator;
				registerParameter(
					name: "world",
					description: "Specify the path of the optional world file relative to the input file. All maps specified there must have the same size. Is used during preparation only.",
					setArgumentOrNull: argument => relativeWorldFilePath = argument,
					defaultArgumentOrNull: "world.txt");
				registerParameter(
					name: "peek",
					description: "Specify the length of the peek into neighbouring maps. Won't exceed map length regardless of specification and is used during preparation only.",
					setArgumentOrNull: argument => unparsedPeekLength = argument,
					defaultArgumentOrNull: "3");
				defaultArgumentsFileContent += lineSeparator;
				registerParameter(
					name: "out",
					description: "Specify the output file path relative to the input file ({0} = input file name, {1} = input file extension (dot included)).",
					setArgumentOrNull: argument => unformattedRelativeOutputFilePath = argument,
					defaultArgumentOrNull: "{0}.release{1}");
				registerParameter(
					name: "overwrite",
					description: "'" + yes + "' if existing files can be overwritten, otherwise '" + ask + "', otherwise anything else.",
					setArgumentOrNull: argument => yesIfOverwriteAllowedOptionallyAsk = argument,
					defaultArgumentOrNull: "ask");
				defaultArgumentsFileContent += lineSeparator;
				registerParameter(
					name: "pause",
					description: "'" + yes + "' to pause the console at execution end, otherwise anything else.",
					setArgumentOrNull: argument => yesIfPause = argument,
					defaultArgumentOrNull: "yes");
				File.WriteAllText(defaultArgumentsFilePath, defaultArgumentsFileContent);

				help += lineSeparator;

				help += typeof(ConsoleApplication).Assembly.GetEmbeddedString("Help") + lineSeparator;
				help += lineSeparator;
				help += executable.GetEmbeddedString("Help") + lineSeparator;

				help = help.Replace("{", "{{").Replace("}", "}}");

				if(commandLineArguments.Length == 0) {
					write_(help);
					goto exit; }
				else {
					var commandLineArgumentIndex = -1;
					var commandLineArgumentIndexByUsedParameter = new Dictionary<string, int>();
					Action<string> registerCommandLineArgument = (parameter) => {
						try { commandLineArgumentIndexByUsedParameter.Add(parameter, commandLineArgumentIndex); }
						catch(ArgumentException) { throw new Error("Parameter '{0}' at {1} was already used at {2}.", parameter, Index.GetNice(commandLineArgumentIndex), Index.GetNice(commandLineArgumentIndexByUsedParameter[parameter])); } };
					foreach(var commandLineArgument in commandLineArguments) {
						commandLineArgumentIndex++;
						var separatorIndex = commandLineArgument.IndexOf(':');
						var hasArgument = separatorIndex != -1;
						var parameter = hasArgument ? commandLineArgument.Substring(0, separatorIndex) : commandLineArgument;
						Action<string> execute;
						if(!executeByParameter.TryGetValue(parameter, out execute)) {
							if(commandLineArgumentIndex != 0) throw new Error("Unknown parameter '{0}' at {1}.", parameter, Index.GetNice(commandLineArgumentIndex));
							inputFilePath = commandLineArgument;
							registerCommandLineArgument(inputFilePathParameter);
							continue; }
						registerCommandLineArgument(parameter);
						var argument = hasArgument ? commandLineArgument.Substring(separatorIndex + 1) : "";
						execute(argument); } }

				if(inputFilePath == "") throw new Error("Please drag a file onto the .exe or specify the input argument.");
				write_("In: {0}", inputFilePath);

				var directory = Path.GetDirectoryName(inputFilePath);
				if(directory != "") directory += Path.DirectorySeparatorChar;

				string outputFilePath;
				{	var relativeOutputFilePath = string.Format(unformattedRelativeOutputFilePath, Path.GetFileNameWithoutExtension(inputFilePath), Path.GetExtension(inputFilePath));
					outputFilePath = directory + relativeOutputFilePath;
					write_("Out: {0}", relativeOutputFilePath); }

				if(File.Exists(outputFilePath)) {
					if(yesIfOverwriteAllowedOptionallyAsk == yes) goto overwriteAllowed;
					if(yesIfOverwriteAllowedOptionallyAsk == ask && read_(message: "File exists already. Type '" + yes + "' to overwrite.") == yes) goto overwriteAllowed;
					goto exit; }
				overwriteAllowed:

				write_(lineSeparator);

				write_("Loading map file {0}...", inputFilePath);
				var map = new Map();
				map.Load(inputFilePath);
				write_("Loaded file.");

				var configuration = map;

				var isEmbeddedConfiguration = relativeConfigurationFilePath == "";

				var configurationFilePath = inputFilePath;
				if(!isEmbeddedConfiguration) {
					configurationFilePath = directory + relativeConfigurationFilePath;
					write_("Loading configuration file {0}...", configurationFilePath);
					configuration = new Map();
					map.Load(configurationFilePath);
					write_("Loaded file."); }

				Tile[,] legendLayerTiles;
				LegendParser<Tile> legendParser;
				{	var legendLayerIndices = configuration.LayerNameOrNullByLayerIndex.IndexEachEqual(legendLayerName).GetCache();
					if(legendLayerIndices.Count != 1) throw new Error("{0} contains no or multiple '{1}' layers.", configurationFilePath, legendLayerName);
					legendLayerTiles = configuration.LayerTilesByLayerIndex[legendLayerIndices.Single()];
					legendParser = new LegendParser<Tile>(legendLayerTiles); }

				if(legendParser.Version != version) throw new Error("File version {0} is not supported.", legendParser.Version);

				var emptyTile = legendParser.EmptyTile;

				var bezelTile = legendParser.GetNextTile("bezel");

				var configurationParser = new ConfigurationParser<Tile>(legendParser);

				var legendTiles = new[] { emptyTile, bezelTile }.Concat(configurationParser.OwnLegendTiles).GetCache();
				if(legendTiles.ContainsDuplicates()) throw new Error("Legend contains conflicting definitions.");

				{	var nameByIndex = new List<string>();
					var tilesByIndex = new List<Tile[,]>();
					foreach(var layerIndex in configuration.LayerNameOrNullByLayerIndex.IndexEachEqual(configurationLayerName)) {
						var index = Index.GetAfter(tilesByIndex.Count);
						nameByIndex.Add(configurationLayerName + " " + Index.GetNice(index));
						tilesByIndex.Add(configuration.LayerTilesByLayerIndex[layerIndex]); }
					configurationParser.NameByIndex = nameByIndex;
					configurationParser.TilesByIndex = tilesByIndex; }

				int peekLength;
				if(!int.TryParse(unparsedPeekLength, out peekLength) || peekLength < 0) throw new Error("Couldn't parse peek length ('{0}').", unparsedPeekLength);
				peekLength = Math.Min(peekLength, Math.Min(map.Size.X, map.Size.Y));
				write_("Using peek length of {0}.", peekLength);

				var hasPeek = peekLength > 0;

				if(actionName == prepareActionName) {
					write_("Preparing file for pattern application...");

					var ungrownMapSize = map.Size;
					var growthSize = new Vector2d(peekLength, peekLength);

					if(!hasPeek) write_("Peek length is zero. Skipping map growth.");
					else {
						write_("Growing map for neighbours peek...");
						map.Grow(peekLength, emptyTile);
						write_("Map grown from {0} to {1}.", ungrownMapSize, map.Size); }

					Action<string, Tile[,]> insertMapLayer;
					{	var nextMapLayerInsertionIndex = map.LayerTilesByLayerIndex.Count();
						insertMapLayer = (name, tiles) => {
							map.InsertNewLayer(nextMapLayerInsertionIndex, tiles.GetSize(), name);
							tiles.WriteTo(map.LayerTilesByLayerIndex[nextMapLayerInsertionIndex]);
							write_("Inserted layer '{0}' at {1}.", name, Index.GetNice(nextMapLayerInsertionIndex));
							nextMapLayerInsertionIndex++; }; }

					var doEmbed = yesIfEmbed == yes;

					if(isEmbeddedConfiguration) write_("Configuration is already embedded. Skipping embedding.");
					else if(!doEmbed) write_("Embedding configuration not permitted. Skipping.");
					else {
						write_("Embedding configuration...");
						insertMapLayer(legendLayerName, legendLayerTiles);
						foreach(var configurationLayerTiles in configurationParser.TilesByIndex) insertMapLayer(configurationLayerName, configurationLayerTiles);
						write_("Configuration embedded."); }

					if(!hasPeek) write_("Peek length is zero. Skipping bezel layer.");
					else {
						write_("Adding bezel layer...");
						var bezelLayerTiles = Array.GetNew<Tile>(ungrownMapSize);
						bezelLayerTiles.FillWith(emptyTile);
						bezelLayerTiles = bezelLayerTiles.Grow(peekLength, bezelTile);
						insertMapLayer(bezelLayerName, bezelLayerTiles);
						write_("Bezel layer added."); }

					if(!hasPeek) write_("Peek length is zero. Skipping peek.");
					else {
						var worldFilePath = directory + relativeWorldFilePath;
						if(!File.Exists(worldFilePath)) write_("'{0}' does not exist. Assuming no neighbours.", worldFilePath);
						else {
							var worldFileRelativeInputFilePath = new Uri(worldFilePath).MakeRelativeUri(new Uri(inputFilePath)).ToString();
							worldFileRelativeInputFilePath = worldFileRelativeInputFilePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
							write_("Determined '{0}' to be '{1}' in the world file.", inputFilePath, worldFileRelativeInputFilePath);

							write_("Loading world file {0}...", worldFilePath);
							var coordinatesByWorldFileRelativeMapFilePath = ConfigurationFile.GetArgumentByParameter(worldFilePath);
							write_("Loaded file.");

							write_("Adding neighbours peek...");
							if(!coordinatesByWorldFileRelativeMapFilePath.ContainsKey(worldFileRelativeInputFilePath)) write_("'{0}' is not in the world. Assuming no neighbours.", inputFilePath);
							else {
								write_("Parsing '{0}' world coordinates.", worldFileRelativeInputFilePath);
								var coordinates = Vector2d.Import(coordinatesByWorldFileRelativeMapFilePath[worldFileRelativeInputFilePath]);
								write_("Parsed map world coordinates ({0}).", coordinates);

								var worldFileDirectory = Path.GetDirectoryName(worldFilePath);
								if(worldFileDirectory != "") worldFileDirectory += Path.DirectorySeparatorChar;

								foreach(var relativeNeighbourCoordinates in Index.RelativeNeighbourIndices2d) {
									var neighbourCoordinates = (coordinates + relativeNeighbourCoordinates).ToString();
									if(!coordinatesByWorldFileRelativeMapFilePath.ContainsKey(neighbourCoordinates)) write_("No neighbour at {0}. Skipping neighbour copy.", neighbourCoordinates);
									else {
										var worldFileRelativeNeighbourFilePath = coordinatesByWorldFileRelativeMapFilePath[neighbourCoordinates];

										var neighbourOffset = new Vector2d(relativeNeighbourCoordinates.X * ungrownMapSize.X, relativeNeighbourCoordinates.Y * ungrownMapSize.Y);
										neighbourOffset += growthSize;

										write_("Copying neighbour {0} to {1} ({2})...", worldFileRelativeNeighbourFilePath, relativeNeighbourCoordinates, neighbourOffset);

										var neighbourFilePath = worldFileDirectory + worldFileRelativeNeighbourFilePath;

										write_("Loading neighbour map {0}...", neighbourFilePath);
										var neighbourMap = new Map();
										neighbourMap.Load(neighbourFilePath);
										write_("Neighbour map loaded.");

										{	var layerIndexMinimumByLayerName = new NullKeyDictionary<string, int>();
											var neighbourLayerIndex = -1;
											foreach(var layerName in neighbourMap.LayerNameOrNullByLayerIndex) {
												neighbourLayerIndex++;

												var layerIndexMinimum = layerIndexMinimumByLayerName.GetValueOrDefault(layerName);
												var layerIndex = map.LayerNameOrNullByLayerIndex.IndexOf(layerName, layerIndexMinimum);
												if(layerIndex == -1) {
													write_("{0}: Could not find a post-{1} occurrence of a layer called {2}.", inputFilePath, Index.GetNice(layerIndexMinimum), String.GetNice(layerName));
													write_("Skipping neighbour layer copy.");
													continue; }
												layerIndexMinimumByLayerName[layerName] = layerIndex + 1;

												write_("Copying neighbour layer {0}.", layerName);
												neighbourMap.LayerTilesByLayerIndex[neighbourLayerIndex].WriteTo(map.LayerTilesByLayerIndex[layerIndex], neighbourOffset);
												write_("Neighbour layer copied."); } }
										write_("Neighbour copied."); } } }
							write_("Neighbours peek added."); } }

					write_("File prepared."); }
				else if(actionName == applyPatternsActionName) {
					write_("Removing configuration and bezel layers...");
					var layerIndex = -1;
					foreach(var layerName in map.LayerNameOrNullByLayerIndex.ToList()) {
						layerIndex++;
						if(layerName == configurationLayerName || layerName == bezelLayerName) map.RemoveLayer(layerIndex); }
					write_("Configuration and bezel layers removed.");

					write_("Applying patterns...");
					new PatternsApplicator<Tile>(configurationParser.ParseAndGetPatterns()).ApplyTo(map.LayerTilesByLayerIndex);
					write_("Patterns applied.");

					if(!hasPeek) { write_("No neighbours peek. Skipping neighbours peek removal."); }
					else {
						write_("Removing neighbours peek...");
						map.Grow(-peekLength, emptyTile);
						write_("Neighbours peek removed."); } }
				else throw new Error("Unknown action '{0}'.", actionName);
			
				write_("Writing map file {0}...", outputFilePath);
				map.Save(outputFilePath);
				write_("Wrote file."); }

			#if DEBUG //Do not convert exceptions to console errors in debug mode.
			finally { }
			try { }
			#endif

			catch(Exception exception) { write_("Error: {0}", exception.Message); }

			exit:
			if(yesIfPause != yes) return;
			write_(lineSeparator);
			write_("Paused. Press a key to continue.");
			Console.ReadKey();
			write_(lineSeparator);
			write_("Continued."); } } }