namespace Client;

internal static class Generator
{
    private static readonly Random Rand = new();

    public static int GenerateInt32(int min, int max)
    {
        return Rand.Next(min, max);
    }

    public static double[][] GenerateMatrix(int rows, int cols)
    {
        var matrix = new double[rows][];
        for (var i = 0; i < rows; i++)
        {
            matrix[i] = new double[cols];
            for (var j = 0; j < cols; j++)
            {
                matrix[i][j] = Rand.NextDouble();
            }
        }
        return matrix;
    }
}
