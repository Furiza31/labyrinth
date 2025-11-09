namespace Labyrinth.Items
{
    /// <summary>
    /// Inventory of collectable items for rooms and players.
    /// </summary>
    /// <param name="item">Optional initial item in the inventory.</param>
    public abstract class Inventory(ICollectable? item = null)
    {
        /// <summary>
        /// Collection storing the items owned by the inventory.
        /// </summary>
        protected IReadOnlyList<ICollectable> StoredItems => _items;

        /// <summary>
        /// True if the inventory contains at least one item, false otherwise.
        /// </summary>
        public bool HasItems => _items.Count > 0;

        /// <summary>
        /// Gets the types of the items in the inventory.
        /// </summary>
        public IEnumerable<Type> ItemTypes => _items.Select(i => i.GetType());

        /// <summary>
        /// Places the n-th item from another inventory into this one.
        /// </summary>
        /// <param name="from">The inventory from which the item is taken. The item is removed from this inventory.</param>
        /// <param name="nth">Zero-based index of the item to move.</param>
        /// <exception cref="InvalidOperationException">Thrown if the source has no items.</exception>
        public void MoveItemFrom(Inventory from, int nth = 0)
        {
            ArgumentNullException.ThrowIfNull(from);

            if (!from.HasItems)
            {
                throw new InvalidOperationException("No item to take from the source inventory");
            }

            ArgumentOutOfRangeException.ThrowIfNegative(nth);
            if (nth >= from._items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(nth), nth, "Requested item index is out of range.");
            }

            var item = from._items[nth];
            from._items.RemoveAt(nth);
            _items.Add(item);
        }

        private List<ICollectable> _items = item is null
            ? []
            : [item];
    }
}
