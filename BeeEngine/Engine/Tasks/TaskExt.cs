using System.Collections.Concurrent;

namespace BeeEngine.Tasks
{
    public static class TaskExt
    {
        public static async Task<IEnumerable<T>> WhenAll<T>(params Task<T>[] tasks)
        {
            var allTasks = Task.WhenAll(tasks);
            try
            {
                return await allTasks;
            }
            catch (Exception)
            {
                
            }

            throw allTasks.Exception ?? throw new UnreachableException("AllTasks.Exception was null");
        }

        public static Task ParallelForEachAsync<T>(IEnumerable<T> source, int degreeOfParallelization,
            Func<T, Task> body)
        {
            async Task AwaitPartition(IEnumerator<T> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    {
                        await body(partition.Current);
                    }
                }
            }

            return Task.WhenAll(Partitioner
                .Create(source)
                .GetPartitions(degreeOfParallelization)
                .AsParallel()
                .Select(AwaitPartition));
        }
    }
}