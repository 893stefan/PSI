namespace TourneeFutee
{
    public class Tour
    {
        private readonly List<(string source, string destination)> segments;
        private readonly float cost;

        public Tour(List<(string source, string destination)> segments, float cost)
        {
            this.segments = segments;
            this.cost = cost;
        }

        public Tour() : this(new List<(string, string)>(), 0) { }

        public float Cost => cost;

        public int NbSegments => segments.Count;

        public bool ContainsSegment((string source, string destination) segment)
        {
            return segments.Contains(segment);
        }

        public void Print()
        {
            Console.WriteLine($"Coût total : {cost}");
            foreach ((string source, string destination) in segments)
            {
                Console.WriteLine($"  {source} -> {destination}");
            }
        }
    }
}
