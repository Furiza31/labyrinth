using Labyrinth.Items;
using Labyrinth.Tiles;

namespace Labyrinth.Build
{
    /// <summary>
    /// Manage the creation of doors and key rooms ensuring each door has a corresponding key room.
    /// </summary>
    public sealed class Keymaster : IDisposable
    {
        /// <summary>
        /// Ensure all created doors have a corresponding key room and vice versa.
        /// </summary>
        /// <exception cref="InvalidOperationException">Some keys are missing or are not placed.</exception>
        public void Dispose()
        {
            if (_unplacedKeys.HasItems || _pendingKeyRooms.Count > 0)
            {
                throw new InvalidOperationException("Unmatched key/door creation");
            }
        }

        /// <summary>
        /// Create a new door and place its key in a previously created empty key room (if any).
        /// </summary>
        /// <returns>Created door</returns>
        public Door NewDoor()
        {
            var door = new Door();

            door.LockAndTakeKey(_unplacedKeys);
            PlacePendingKeys();
            return door;
        }

        /// <summary>
        /// Create a new room with key and place the key if a door was previously created.
        /// </summary>
        /// <returns>Created key room</returns>
        public Room NewKeyRoom()
        {
            var room = new Room();
            _pendingKeyRooms.Enqueue(room);
            PlacePendingKeys();
            return room;
        }

        private void PlacePendingKeys()
        {
            while (_unplacedKeys.HasItems && _pendingKeyRooms.Count > 0)
            {
                var pendingRoom = _pendingKeyRooms.Dequeue();
                pendingRoom.Pass().MoveItemFrom(_unplacedKeys);
            }
        }

        private readonly MyInventory _unplacedKeys = new();
        private readonly Queue<Room> _pendingKeyRooms = new();
    }
}
