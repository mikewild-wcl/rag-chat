namespace Rag.Chat.Core.UnitTests.Extensions;

public static class TestHelpers
{
    public static async IAsyncEnumerable<T> MockAsyncEnumerable<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.Yield();
        }
    }
}
