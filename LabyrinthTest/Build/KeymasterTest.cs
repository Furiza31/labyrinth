using Labyrinth.Build;
using Labyrinth.Items;

namespace LabyrinthTest.Build;

[TestFixture]
public class KeymasterTest
{
    [Test]
    public void HandlesMultipleDoorsBeforeKeys()
    {
        using var km = new Keymaster();

        var firstDoor = km.NewDoor();
        var secondDoor = km.NewDoor();

        var firstRoom = km.NewKeyRoom();
        var secondRoom = km.NewKeyRoom();

        using var scope = Assert.EnterMultipleScope();
        Assert.That(firstRoom.Pass().HasItems, Is.True);
        Assert.That(secondRoom.Pass().HasItems, Is.True);
        Assert.That(firstDoor.IsLocked, Is.True);
        Assert.That(secondDoor.IsLocked, Is.True);

        var firstBag = new MyInventory();
        firstBag.MoveItemFrom(firstRoom.Pass());
        Assert.That(firstDoor.Open(firstBag), Is.True);

        var secondBag = new MyInventory();
        secondBag.MoveItemFrom(secondRoom.Pass());
        Assert.That(secondDoor.Open(secondBag), Is.True);
    }

    [Test]
    public void HandlesMultipleKeysBeforeDoors()
    {
        using var km = new Keymaster();

        var firstRoom = km.NewKeyRoom();
        var secondRoom = km.NewKeyRoom();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(firstRoom.Pass().HasItems, Is.False);
            Assert.That(secondRoom.Pass().HasItems, Is.False);
        }

        var firstDoor = km.NewDoor();
        Assert.That(firstRoom.Pass().HasItems, Is.True);
        Assert.That(secondRoom.Pass().HasItems, Is.False);

        var secondDoor = km.NewDoor();
        Assert.That(secondRoom.Pass().HasItems, Is.True);

        var firstBag = new MyInventory();
        firstBag.MoveItemFrom(firstRoom.Pass());
        Assert.That(firstDoor.Open(firstBag), Is.True);

        var secondBag = new MyInventory();
        secondBag.MoveItemFrom(secondRoom.Pass());
        Assert.That(secondDoor.Open(secondBag), Is.True);
    }

    [Test]
    public void DisposeThrowsWhenDoorsLackRooms()
    {
        var km = new Keymaster();
        km.NewDoor();

        Assert.That(() => km.Dispose(), Throws.InvalidOperationException);
    }

    [Test]
    public void DisposeThrowsWhenRoomsLackDoors()
    {
        var km = new Keymaster();
        km.NewKeyRoom();

        Assert.That(() => km.Dispose(), Throws.InvalidOperationException);
    }

    [Test]
    public void DisposeDoesNotThrowWhenDistributionMatches()
    {
        Assert.DoesNotThrow(() =>
        {
            using var km = new Keymaster();
            km.NewDoor();
            km.NewKeyRoom();
        });
    }
}
