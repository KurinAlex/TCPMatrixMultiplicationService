using Utility;

namespace Server;

internal class MatrixMultiplier
{
    private const int ThreadsCount = 4;

    private readonly object _lockObject = new();

    private int _index = -1;

    public double[][] Multiply(MultiplicationData data, Stopper stopper)
    {
        data.Deconstruct(out var n, out var m, out var l, out var leftMatrix, out var rightMatrix);

        ThrowIfNotPositive(n, nameof(n));
        ThrowIfNotPositive(m, nameof(m));
        ThrowIfNotPositive(l, nameof(l));
        ThrowIfHasWrongDimensions(leftMatrix, n, m, "Left");
        ThrowIfHasWrongDimensions(rightMatrix, m, l, "Right");

        var resultMatrix = new double[n][];
        for (var i = 0; i < n; i++)
        {
            resultMatrix[i] = new double[l];
        }

        var countdownEvent = new CountdownEvent(ThreadsCount);

        for (var i = 0; i < ThreadsCount && !stopper.Stop; i++)
        {
            var container = new ThreadContainer(data, resultMatrix, countdownEvent, stopper);
            ThreadPool.QueueUserWorkItem(ThreadWork, container);
        }

        countdownEvent.Wait();

        return resultMatrix;
    }

    private void ThreadWork(object? container)
    {
        if (container is not ThreadContainer(var multiplicationData, var resultMatrix, var countdownEvent, var stopper))
        {
            return;
        }

        multiplicationData.Deconstruct(out var n, out var m, out var l, out var left, out var right);

        var maxIndexValue = n * l;

        while (!stopper.Stop)
        {
            var currentIndex = Interlocked.Increment(ref _index);
            if (currentIndex >= maxIndexValue)
            {
                break;
            }

            var i = currentIndex / l;
            var j = currentIndex % l;

            var sum = 0.0;
            for (var k = 0; k < m && !stopper.Stop; k++)
            {
                sum += left[i][k] * right[k][j];
            }

            if (stopper.Stop)
            {
                break;
            }

            lock (_lockObject)
            {
                resultMatrix[i][j] = sum;
            }
        }

        countdownEvent.Signal();
    }

    private static void ThrowIfNotPositive(int n, string paramName)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be positive integer.");
        }
    }

    private static void ThrowIfHasWrongDimensions(double[][] matrix, int rows, int cols, string matrixName)
    {
        if (matrix.Length != rows || Array.Exists(matrix, r => r.Length != cols))
        {
            throw new ArgumentException($"{matrixName} has wrong dimensions.");
        }
    }
}
