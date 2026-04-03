// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Internal;

namespace CodeGenerator.Core.UnitTests;

public class ObservableTests
{
    private class TestObserver<T> : IObserver<T>
    {
        public List<T> ReceivedItems { get; } = new();
        public List<Exception> ReceivedErrors { get; } = new();
        public int CompletedCount { get; private set; }

        public void OnNext(T value) => ReceivedItems.Add(value);
        public void OnError(Exception error) => ReceivedErrors.Add(error);
        public void OnCompleted() => CompletedCount++;
    }

    // ── Subscribe ──

    [Fact]
    public void Subscribe_ReturnsDisposable()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();

        var subscription = observable.Subscribe(observer);

        Assert.NotNull(subscription);
    }

    [Fact]
    public void Subscribe_MultipleObservers_AllReceiveMessages()
    {
        var observable = new Observable<int>();
        var observer1 = new TestObserver<int>();
        var observer2 = new TestObserver<int>();

        observable.Subscribe(observer1);
        observable.Subscribe(observer2);

        observable.Broadcast(42);

        Assert.Single(observer1.ReceivedItems);
        Assert.Single(observer2.ReceivedItems);
        Assert.Equal(42, observer1.ReceivedItems[0]);
        Assert.Equal(42, observer2.ReceivedItems[0]);
    }

    // ── Broadcast ──

    [Fact]
    public void Broadcast_SingleSubscriber_ReceivesItem()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();
        observable.Subscribe(observer);

        observable.Broadcast("hello");

        Assert.Single(observer.ReceivedItems);
        Assert.Equal("hello", observer.ReceivedItems[0]);
    }

    [Fact]
    public void Broadcast_MultipleItems_AllReceived()
    {
        var observable = new Observable<int>();
        var observer = new TestObserver<int>();
        observable.Subscribe(observer);

        observable.Broadcast(1);
        observable.Broadcast(2);
        observable.Broadcast(3);

        Assert.Equal(3, observer.ReceivedItems.Count);
        Assert.Equal(new[] { 1, 2, 3 }, observer.ReceivedItems);
    }

    [Fact]
    public void Broadcast_NoSubscribers_DoesNotThrow()
    {
        var observable = new Observable<string>();
        observable.Broadcast("test");
    }

    // ── Unsubscribe ──

    [Fact]
    public void Unsubscribe_StopsReceivingMessages()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();
        observable.Subscribe(observer);

        observable.Broadcast("before");
        observable.Unsubscribe(observer);
        observable.Broadcast("after");

        Assert.Single(observer.ReceivedItems);
        Assert.Equal("before", observer.ReceivedItems[0]);
    }

    [Fact]
    public void Unsubscribe_NonExistentObserver_DoesNotThrow()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();

        observable.Unsubscribe(observer);
    }

    // ── Dispose subscription ──

    [Fact]
    public void DisposeSubscription_StopsReceivingMessages()
    {
        var observable = new Observable<string>();
        var observer = new TestObserver<string>();
        var subscription = observable.Subscribe(observer);

        observable.Broadcast("before");
        subscription.Dispose();
        observable.Broadcast("after");

        Assert.Single(observer.ReceivedItems);
        Assert.Equal("before", observer.ReceivedItems[0]);
    }

    // ── BroadcastError (protected, tested via derived class) ──

    private class TestObservable<T> : Observable<T>
    {
        public new void BroadcastError(Exception error) => base.BroadcastError(error);
        public new void BroadcastOnCompleted() => base.BroadcastOnCompleted();
    }

    [Fact]
    public void BroadcastError_NotifiesObservers()
    {
        var observable = new TestObservable<string>();
        var observer = new TestObserver<string>();
        observable.Subscribe(observer);

        var error = new InvalidOperationException("test error");
        observable.BroadcastError(error);

        Assert.Single(observer.ReceivedErrors);
        Assert.Same(error, observer.ReceivedErrors[0]);
    }

    [Fact]
    public void BroadcastError_MultipleObservers_AllNotified()
    {
        var observable = new TestObservable<string>();
        var observer1 = new TestObserver<string>();
        var observer2 = new TestObserver<string>();
        observable.Subscribe(observer1);
        observable.Subscribe(observer2);

        observable.BroadcastError(new Exception("err"));

        Assert.Single(observer1.ReceivedErrors);
        Assert.Single(observer2.ReceivedErrors);
    }

    // ── BroadcastOnCompleted ──

    [Fact]
    public void BroadcastOnCompleted_NotifiesObservers()
    {
        var observable = new TestObservable<string>();
        var observer = new TestObserver<string>();
        observable.Subscribe(observer);

        observable.BroadcastOnCompleted();

        Assert.Equal(1, observer.CompletedCount);
    }

    [Fact]
    public void BroadcastOnCompleted_MultipleObservers_AllNotified()
    {
        var observable = new TestObservable<string>();
        var observer1 = new TestObserver<string>();
        var observer2 = new TestObserver<string>();
        observable.Subscribe(observer1);
        observable.Subscribe(observer2);

        observable.BroadcastOnCompleted();

        Assert.Equal(1, observer1.CompletedCount);
        Assert.Equal(1, observer2.CompletedCount);
    }

    // ── IObservable<T> ──

    [Fact]
    public void ImplementsIObservable()
    {
        var observable = new Observable<string>();
        Assert.IsAssignableFrom<IObservable<string>>(observable);
    }
}
