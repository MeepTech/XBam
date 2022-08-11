using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// A Keyed collection that uses a lambda for key fetching.
  /// </summary>
  public class KeyedSet<TKey, TItem> : KeyedCollection<TKey, TItem>, IReadOnlyKeyedSet<TKey, TItem> {
	 Func<TItem, TKey> _keyFetcher;

		/// <summary>
		/// Make a keyed set that uses the function to fetch it's items.
		/// </summary>
		public KeyedSet(Func<TItem, TKey> getKeyFunction) : base() {
			if(getKeyFunction is not null) {
				_keyFetcher = getKeyFunction;
			} else
				throw new ArgumentNullException(nameof(getKeyFunction));
		}

		/// <summary>
		/// Make a keyed set that uses the function to fetch it's items.
		/// </summary>
		public KeyedSet(Func<TItem, TKey> getKeyFunction, IEqualityComparer<TKey> comparer) : base(comparer) {
			if(getKeyFunction is not null) {
				_keyFetcher = getKeyFunction;
			} else
				throw new ArgumentNullException(nameof(getKeyFunction));
		}

    ///<summary><inheritdoc/></summary>
    protected override TKey GetKeyForItem(TItem item) 
			=> _keyFetcher(item);
  }
}