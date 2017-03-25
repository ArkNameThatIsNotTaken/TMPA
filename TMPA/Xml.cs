using System.Linq;
using System.Xml.Linq;
namespace Tmpa {
	public static class Xml {
		public static void Insert(this XContainer container, string name, int index, object item) {
			var items = container.Elements(name);
			var itemAfter = index == 0 ? items.FirstOrDefault() : (index == items.Count() ? null : items.ElementAt(index));
			if(itemAfter == null) container.Add(item);
			else itemAfter.AddBeforeSelf(item); } } }