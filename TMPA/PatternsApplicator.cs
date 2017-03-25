using System.Collections.Generic;
using System.Linq;
namespace Tmpa {
	public class PatternsApplicator<Tile> {
		private struct CandidateInfo {
			public readonly Vector2d QueueOffset;
			public readonly int Index;
			public CandidateInfo(Vector2d queueOffset, int index) {
				QueueOffset = queueOffset;
				Index = index; } }

		//Caches for fast pattern recandidation after stamping.
		private readonly Dictionary<Tile, List<Pattern<Tile>>> candidatesByFirstTile_;
		private readonly Dictionary<Tile, List<CandidateInfo>> candidateInfosByTile_;

		private IReadOnlyList<Tile[,]> map_;
		private int layerIndex_;
		private Tile[,] layer_;
		private EachArray2dItem<Tile> layerTile_;
		private /*Sorted descendingly by precedence.*/OrderPreservingQueue<Pattern<Tile>>[,] layerCandidateQueues_;
		private bool hasLayerAltered_;

		public PatternsApplicator(IReadOnlyList<Pattern<Tile>> patterns /*Sorted descendingly by precedence.*/) {
			candidatesByFirstTile_ = patterns.GetNewListDictionary(getKey: pattern => pattern.FirstTile);
			candidateInfosByTile_ = patterns
				.SelectMany(pattern => pattern.Tiles.GetIndices().Select(index => new { Pattern = pattern, Index = index, Value = pattern.Tiles.GetValue(index) }))
				.Where(tile => tile.Value != null)
				.GetNewListDictionary(
					getKey: tile => tile.Value,
					getValue: tile => new CandidateInfo(
						queueOffset: -tile.Index + tile.Pattern.FirstTileIndex,
						index: candidatesByFirstTile_[tile.Pattern.FirstTile].IndexOf(tile.Pattern))); }
		
		public void ApplyTo(IReadOnlyList<Tile[,]> map) {
			map_ = map;
			layerIndex_ = -1;
			hasLayerAltered_ = false;
			foreach(var layer in map_) {
				layerIndex_++;
				layer_ = layer;
				layerTile_ = layer_.Each();
				if(layerTile_.IsNil) continue;
				layerCandidateQueues_ = layer_.GetCopy(tile => OrderPreservingQueue.GetNew(candidatesByFirstTile_.GetValueOrCreateValue(tile)));
				applyToLayer_(); } }

		private void setEarliestLayerTileIndex_(Vector2d xy) { layerTile_.Index = layerTile_.Index.GetFirst(xy); }

		private void applyToLayer_(bool isRecursing = false, uint minimalPrecedence = 0 /*should be 0 if not recursing*/) {
			repeat:
			var index = layerTile_.Index;
			var candidateQueue = layerCandidateQueues_.GetValue(index);

			repeatCandidatePeek:
			if(candidateQueue.IsEmpty) goto noMatchFound;
			var candidate = candidateQueue.NextPeek;
			if(candidate.Precedence < minimalPrecedence) goto noMatchFound;

			var candidateIndex = index - candidate.FirstTileIndex;
			foreach(var candidateTile in candidate.Tiles.GetItems(candidate.FirstTileIndex)) {
				if(candidateTile.Value == null) continue; //Some pattern tiles may be null to allow nonrectangular shapes. Thus null pattern tiles always "match".

				layerTile_.Index = candidateIndex + candidateTile.Index; //Dirty for recursive tile matching. Has to be cleaned up when this loop at this recursion level ends.

				if(layerTile_.IsNil) goto candidatePeekMismatches; //Part is out of range.

				//Potential higher precedence matches ahead may affect this tile matching, so search for them ahead first!
				applyToLayer_(true, candidate.Precedence + 1); //Stack overflow may occur when far too many patterns collide with each other in ascending distinct precedence order.
				if(hasLayerAltered_) goto layerAltered;
				
				if(Equals(candidateTile.Value, layerTile_.Value)) continue; //Tiles match.
				
				candidatePeekMismatches:
				candidateQueue.DequeueNext();
				layerTile_.Index = index; //Clean up from recursive tile matching.
				goto repeatCandidatePeek; }
			layerTile_.Index = index; //Clean up from recursive tile matching.
			
			//Candidate matches!
			candidateQueue.DequeueNext();
			stampOnLayer_(candidate, candidateIndex);
			if(hasLayerAltered_) goto layerAltered;
			goto repeatCandidatePeek;
			
			noMatchFound:
			if(isRecursing) return;
			layerTile_.BeNext();
			if(layerTile_.IsntNil) goto repeat;
		
			layerAltered: //A potentially affecting match has been found. layerTile_.Index is now the recandidation index if that occured at an earlier location.
			if(isRecursing) return; //Give control back to the caller which will flow into this hasLayerAltered_ if-block.
			hasLayerAltered_ = false; //Clean up from recandidation marking.
			setEarliestLayerTileIndex_(index); //Redo at recandidation or index, whichever comes first. //Clean up from recursive tile matching.
			goto repeat; }
		
		private void stampOnLayer_(Pattern<Tile> pattern, Vector2d index) {
			var relativeLayerIndex = 0;
			var layer = layer_;
			foreach(var stamp in pattern.Stamps) {
				if(stamp.RelativeLayerIndex != relativeLayerIndex) {
					relativeLayerIndex = (int)stamp.RelativeLayerIndex; //Assumes that layer counts practically never reach close to int.MaxValue.
					var layerIndex = layerIndex_ + relativeLayerIndex;
					if(layerIndex > Index.GetMaximum(map_.Count)) break;
					layer = map_[layerIndex]; }
				foreach(var tile in stamp.Tiles.GetItems()) {
					var value = tile.Value;
					if(value == null /*Can be null to allow nonrectangular shapes.*/) continue;
					var currentIndex = index + tile.Index;
					var formerValue = layer.GetValueOrGetDefault(currentIndex);
					if(formerValue == null /*Out of range.*/ || Equals(value, formerValue)) continue;
					layer.SetValue(currentIndex, value); //Actual stamping.
					if(relativeLayerIndex > 0) continue; //Recandidation is only needed in the current pattern application layer.
					hasLayerAltered_ = true; //Dirty for recandidation marking. //Tell pattern application about the existence of recandidated parts.
					setEarliestLayerTileIndex_(currentIndex); //Mark the earliest recandidated part for a re-search.
					layerCandidateQueues_.GetValue(currentIndex).InitializeOrReset(candidatesByFirstTile_[value]);
					foreach(var recandidateInfo in candidateInfosByTile_[value]) {
						var queueIndex = currentIndex + recandidateInfo.QueueOffset;
						var queue = layerCandidateQueues_.GetValueOrGetDefault(queueIndex);
						if(queue == null /*Out of range.*/) continue;
						setEarliestLayerTileIndex_(queueIndex); //Mark the earliest recandidated part for a re-search.
						queue.BeEnqueued(recandidateInfo.Index); } } } } } }