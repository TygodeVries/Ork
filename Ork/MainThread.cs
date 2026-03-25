using System.Collections.Concurrent;

namespace Ork;

public class MainThread
{
    private static ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
    public static void Run(Action action)
    {
        actions.Enqueue(action);
    }

    public static void Update()
    {
        while (actions.TryDequeue(out var action))
        {
            action();
        }
    }
}
