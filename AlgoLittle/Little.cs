namespace TourneeFutee
{
    public class Little
    {
        private readonly Graph graph;
        private readonly int nbCities;

        public Little(Graph graph)
        {
            this.graph = graph;
            this.nbCities = graph.Order;
        }

        public Tour ComputeOptimalTour()
        {
            List<string> vertices = graph.VertexNames;
            Matrix m = BuildCostMatrix(vertices);
            float reduction = ReduceMatrix(m);
            return BranchAndBound(m, new List<string>(vertices), new List<string>(vertices),
                reduction, new List<(string, string)>());
        }

        private Tour BranchAndBound(Matrix matrix, List<string> rowLabels, List<string> colLabels,
            float lowerBound, List<(string, string)> includedSegments)
        {
            if (lowerBound == float.PositiveInfinity)
                return new Tour(new List<(string, string)>(), float.PositiveInfinity);

            if (matrix.NbRows == 1)
            {
                float lastCost = matrix.GetValue(0, 0);
                if (lastCost == float.PositiveInfinity)
                    return new Tour(new List<(string, string)>(), float.PositiveInfinity);
                var finalSegments = new List<(string, string)>(includedSegments) { (rowLabels[0], colLabels[0]) };
                return new Tour(finalSegments, lowerBound + lastCost);
            }

            var (i, j, _) = GetMaxRegret(matrix);
            string ri = rowLabels[i];
            string cj = colLabels[j];

            // Branche droite : inclure l'arc (ri → cj)
            var rightIncluded = new List<(string, string)>(includedSegments) { (ri, cj) };

            Matrix rightMatrix = CopyMatrix(matrix);
            rightMatrix.RemoveRow(i);
            rightMatrix.RemoveColumn(j);

            var rightRows = new List<string>(rowLabels);
            rightRows.RemoveAt(i);
            var rightCols = new List<string>(colLabels);
            rightCols.RemoveAt(j);

            for (int r = 0; r < rightMatrix.NbRows; r++)
                for (int c = 0; c < rightMatrix.NbColumns; c++)
                    if (rightMatrix.GetValue(r, c) != float.PositiveInfinity &&
                        IsForbiddenSegment((rightRows[r], rightCols[c]), rightIncluded, nbCities))
                        rightMatrix.SetValue(r, c, float.PositiveInfinity);

            float rightReduction = ReduceMatrix(rightMatrix);
            Tour rightTour = BranchAndBound(rightMatrix, rightRows, rightCols,
                lowerBound + rightReduction, rightIncluded);

            // Branche gauche : exclure l'arc (ri → cj)
            Matrix leftMatrix = CopyMatrix(matrix);
            leftMatrix.SetValue(i, j, float.PositiveInfinity);

            float leftReduction = ReduceMatrix(leftMatrix);
            Tour leftTour = BranchAndBound(leftMatrix, new List<string>(rowLabels), new List<string>(colLabels),
                lowerBound + leftReduction, new List<(string, string)>(includedSegments));

            return rightTour.Cost <= leftTour.Cost ? rightTour : leftTour;
        }

        public static float ReduceMatrix(Matrix m)
        {
            float total = 0;

            for (int i = 0; i < m.NbRows; i++)
            {
                float rowMin = float.PositiveInfinity;
                for (int j = 0; j < m.NbColumns; j++)
                    rowMin = Math.Min(rowMin, m.GetValue(i, j));

                if (rowMin == float.PositiveInfinity) return float.PositiveInfinity;
                if (rowMin == 0) continue;

                total += rowMin;
                for (int j = 0; j < m.NbColumns; j++)
                    if (m.GetValue(i, j) != float.PositiveInfinity)
                        m.SetValue(i, j, m.GetValue(i, j) - rowMin);
            }

            for (int j = 0; j < m.NbColumns; j++)
            {
                float colMin = float.PositiveInfinity;
                for (int i = 0; i < m.NbRows; i++)
                    colMin = Math.Min(colMin, m.GetValue(i, j));

                if (colMin == float.PositiveInfinity) return float.PositiveInfinity;
                if (colMin == 0) continue;

                total += colMin;
                for (int i = 0; i < m.NbRows; i++)
                    if (m.GetValue(i, j) != float.PositiveInfinity)
                        m.SetValue(i, j, m.GetValue(i, j) - colMin);
            }

            return total;
        }

        public static (int i, int j, float value) GetMaxRegret(Matrix m)
        {
            int bestI = 0, bestJ = 0;
            float bestRegret = float.NegativeInfinity;

            for (int i = 0; i < m.NbRows; i++)
            {
                for (int j = 0; j < m.NbColumns; j++)
                {
                    if (m.GetValue(i, j) != 0) continue;

                    float rowMin = float.PositiveInfinity;
                    for (int k = 0; k < m.NbColumns; k++)
                        if (k != j) rowMin = Math.Min(rowMin, m.GetValue(i, k));

                    float colMin = float.PositiveInfinity;
                    for (int k = 0; k < m.NbRows; k++)
                        if (k != i) colMin = Math.Min(colMin, m.GetValue(k, j));

                    float regret = (rowMin == float.PositiveInfinity ? 0 : rowMin)
                                 + (colMin == float.PositiveInfinity ? 0 : colMin);

                    if (regret > bestRegret)
                    {
                        bestRegret = regret;
                        bestI = i;
                        bestJ = j;
                    }
                }
            }

            return (bestI, bestJ, bestRegret);
        }

        public static bool IsForbiddenSegment((string source, string destination) segment,
            List<(string source, string destination)> includedSegments, int nbCities)
        {
            string current = segment.destination;
            int chainLength = 1;

            while (true)
            {
                int idx = includedSegments.FindIndex(s => s.source == current);
                if (idx < 0) return false;
                current = includedSegments[idx].destination;
                chainLength++;
                if (current == segment.source)
                    return chainLength < nbCities;
            }
        }

        private Matrix BuildCostMatrix(List<string> vertices)
        {
            int n = vertices.Count;
            Matrix m = new Matrix(n, n, defaultValue: float.PositiveInfinity);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    try
                    {
                        m.SetValue(i, j, graph.GetEdgeWeight(vertices[i], vertices[j]));
                    }
                    catch (ArgumentException)
                    {
                        // arc inexistant : coût infini (déjà valeur par défaut)
                    }
                }
            }
            return m;
        }

        private static Matrix CopyMatrix(Matrix m)
        {
            Matrix copy = new Matrix(m.NbRows, m.NbColumns);
            for (int i = 0; i < m.NbRows; i++)
                for (int j = 0; j < m.NbColumns; j++)
                    copy.SetValue(i, j, m.GetValue(i, j));
            return copy;
        }
    }
}
