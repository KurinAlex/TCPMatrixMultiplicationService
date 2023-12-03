using Utility;

namespace Server;

internal class MatrixMultiplier
{
    private const int ThreadsCount = 4;

    private int _index = -1;

    public double[][] Multiply(MultiplicationData data)
    {
        _index = -1;

        var threads = new List<Thread>(ThreadsCount);
        var containers = new List<ThreadContainer>(ThreadsCount);

        for (var i = 0; i < ThreadsCount; i++)
        {
            var thread = new Thread(ThreadWork);
            threads.Add(thread);

            var container = new ThreadContainer(data);
            containers.Add(container);

            thread.Start(container);
        }

        var n = data.N;
        var l = data.L;

        var resultMatrix = new double[n][];
        for (var i = 0; i < n; i++)
        {
            resultMatrix[i] = new double[l];
        }

        for (var i = 0; i < ThreadsCount; i++)
        {
            threads[i].Join();
            foreach (var (index, result) in containers[i].Results)
            {
                resultMatrix[index / l][index % l] = result;
            }
        }

        return resultMatrix;
    }

    private void ThreadWork(object? container)
    {
        if (container is not ThreadContainer threadContainer)
        {
            return;
        }

        threadContainer.Data.Deconstruct(out var n, out var m, out var l, out var left, out var right);

        var maxIndexValue = n * l;

        while (true)
        {
            var currentIndex = Interlocked.Increment(ref _index);
            if (currentIndex >= maxIndexValue)
            {
                break;
            }

            var i = currentIndex / l;
            var j = currentIndex % l;

            var sum = 0.0;
            for (var k = 0; k < m; k++)
            {
                sum += left[i][k] * right[k][j];
            }

            threadContainer.Results.Add((currentIndex, sum));
        }
    }
}
