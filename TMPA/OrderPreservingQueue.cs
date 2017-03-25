using System.Collections.Generic;
namespace Tmpa {
	public static class OrderPreservingQueue {
		public static OrderPreservingQueue<Item> GetNew<Item>(/*immutable*/ IReadOnlyList<Item> items) {
			var queue = new OrderPreservingQueue<Item>();
			queue.InitializeOrReset(items);
			return queue; } }
	public class OrderPreservingQueue<Item> {
		private /*immutable*/ IReadOnlyList<Item> items_;
		private bool[] isItemDequeuedByIndex_;

		private int nextIndexPeek_;

		public bool IsEmpty { get { return nextIndexPeek_ > Index.GetMaximum(items_.Count); } }

		public Item NextPeek { get { return items_[nextIndexPeek_]; } }

		public void InitializeOrReset(IReadOnlyList<Item> items) {
			items_ = items;
			isItemDequeuedByIndex_ = new bool[items_.Count];
			nextIndexPeek_ = 0; }

		public void BeEnqueued(int index) {
			if(!isItemDequeuedByIndex_[index]) return;
			isItemDequeuedByIndex_[index] = false;
			if(nextIndexPeek_ > index) nextIndexPeek_ = index; }
		public void DequeueNext() {
			isItemDequeuedByIndex_[nextIndexPeek_] = true;
			do nextIndexPeek_++; while(!IsEmpty && isItemDequeuedByIndex_[nextIndexPeek_]); } } }