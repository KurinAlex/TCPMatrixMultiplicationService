namespace Client;

internal static class Generator
{
    private static readonly Random Rand = new();

    public static int GenerateInt32(int min, int max)
    {
        return Rand.Next(min, max);
    }

    public static double[][] GenerateMatrix(int rows, int cols, double min, double max)
    {
        var matrix = new double[rows][];
        for (var i = 0; i < rows; i++)
        {
            matrix[i] = new double[cols];
            for (var j = 0; j < cols; j++)
            {
                matrix[i][j] = GenerateDouble(min, max);
            }
        }

        return matrix;
    }

    private static double GenerateDouble(double min, double max)
    {
        return Rand.NextDouble() * (max - min) + min;
    }
}
