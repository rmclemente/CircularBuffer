using System.Runtime.InteropServices;

namespace circular.buffer.tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        //arrange
        //act
        var circularBuffer = CircularBuffer<ManagedClass>.Create(6, ManagedClass.Create);

        //assert
        Assert.Equal(6, circularBuffer.Capacity);
        Assert.Equal(6, circularBuffer.Available);
        Assert.True(circularBuffer.IsFull);
        Assert.False(circularBuffer.IsEmpty);
    }

    [Fact]
    public void Test2()
    {
        //arrange
        var circularBuffer = CircularBuffer<ManagedClass>.Create(6, ManagedClass.Create);

        //act
        var unmanagedClass = circularBuffer.Acquire();

        //assert
        Assert.Equal(6, circularBuffer.Capacity);
        Assert.Equal(5, circularBuffer.Available);
        Assert.False(circularBuffer.IsFull);
        Assert.False(circularBuffer.IsEmpty);
    }

    [Fact]
    public void Test3()
    {
        //arrange
        var circularBuffer = CircularBuffer<ManagedClass>.Create(6, ManagedClass.Create);

        //act
        ManagedClass unmanagedClass1 = circularBuffer.Acquire();
        var unmanagedClass2 = circularBuffer.Acquire();
        ManagedClass unmanagedClass3 = circularBuffer.Acquire();
        ManagedClass unmanagedClass4 = circularBuffer.Acquire();
        ManagedClass unmanagedClass5 = circularBuffer.Acquire();
        ManagedClass unmanagedClass6 = circularBuffer.Acquire();

        //assert
        Assert.Equal(6, circularBuffer.Capacity);
        Assert.Equal(0, circularBuffer.Available);
        Assert.False(circularBuffer.IsFull);
        Assert.True(circularBuffer.IsEmpty);
    }

    [Fact]
    public void Test4()
    {
        //arrange
        var circularBuffer = CircularBuffer<ManagedClass>.Create(6, ManagedClass.Create);
        int capacityBefore;
        int availableBefore;
        bool isFullBefore;
        bool isEmptyBefore;

        //act
        using (var item = circularBuffer.Acquire())
        {
            capacityBefore = circularBuffer.Capacity;
            availableBefore = circularBuffer.Available;
            isFullBefore = circularBuffer.IsFull;
            isEmptyBefore = circularBuffer.IsEmpty;
        }

        //assert
        Assert.Equal(6, circularBuffer.Capacity);
        Assert.Equal(6, circularBuffer.Available);
        Assert.True(circularBuffer.IsFull);
        Assert.False(circularBuffer.IsEmpty);

        Assert.Equal(6, capacityBefore);
        Assert.Equal(5, availableBefore);
        Assert.False(isFullBefore);
    }

    [Fact]
    public void Test5()
    {
        //arrange
        var circularBuffer = UnmanagedCircularBuffer<UnmanagedClass>.Create(1, UnmanagedClass.Create, p => p.Id != Guid.Empty);
        UnmanagedClass unmanagedClass;
        Guid unmanagedClassId;

        //act
        using (var bufferItem = circularBuffer.Acquire())
        {
            unmanagedClass = bufferItem;
            unmanagedClassId = bufferItem.Item.Id;
        }

        //assert
        using (var bufferItem = circularBuffer.Acquire())
        {
            Assert.NotEqual(unmanagedClassId, bufferItem.Item.Id);
        }
        Assert.Equal(Guid.Empty, unmanagedClass.Id);
    }
}

public class UnmanagedClass : IDisposable
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public static UnmanagedClass Create() => new();
    public void Dispose()
    {
        Id = Guid.Empty;
    }
}

public class ManagedClass
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public static ManagedClass Create() => new();
}