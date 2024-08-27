using System.Runtime.InteropServices;

namespace circular.buffer.tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        //arrange
        //act
        var circularBuffer = new CircularBuffer<UnmanagedClass>(6, UnmanagedClass.Create);

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
        var circularBuffer = new CircularBuffer<UnmanagedClass>(6, UnmanagedClass.Create);

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
        var circularBuffer = new CircularBuffer<UnmanagedClass>(6, UnmanagedClass.Create);

        //act
        UnmanagedClass unmanagedClass1 = circularBuffer.Acquire();
        var unmanagedClass2 = circularBuffer.Acquire();
        UnmanagedClass unmanagedClass3 = circularBuffer.Acquire();
        UnmanagedClass unmanagedClass4 = circularBuffer.Acquire();
        UnmanagedClass unmanagedClass5 = circularBuffer.Acquire();
        UnmanagedClass unmanagedClass6 = circularBuffer.Acquire();

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
        var circularBuffer = new CircularBuffer<UnmanagedClass>(6, UnmanagedClass.Create);
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