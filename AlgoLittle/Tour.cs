using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    // Modélise une tournée dans le cadre du problème du voyageur de commerce
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

        // Coût total de la tournée
        public float Cost
        {
            get { return cost; }
        }

        // Nombre de trajets dans la tournée
        public int NbSegments
        {
            get { return segments.Count; }
        }

        // Renvoie vrai si la tournée contient le trajet `source`->`destination`
        public bool ContainsSegment((string source, string destination) segment)
        {
            return segments.Contains(segment);
        }

        // Affiche les informations sur la tournée : coût total et trajets
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
