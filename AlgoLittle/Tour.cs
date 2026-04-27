using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    // Modélise une tournée dans le cadre du problème du voyageur de commerce
    public class Tour
    {
        private readonly List<(string source, string destination)> segments;
        private readonly List<string> sommets;
        private readonly float cout;

        public Tour(List<(string source, string destination)> segments, float cout)
        {
            this.segments = segments;
            this.cout = cout;
            this.sommets = ReconstruireSommets(segments);
        }

        // Construit une tournée à partir d'une séquence ordonnée de noms de sommets
        public Tour(List<string> sommets, float cout)
        {
            this.sommets = new List<string>(sommets);
            this.cout = cout;
            this.segments = ReconstruireSegments(sommets);
        }

        public Tour() : this(new List<(string, string)>(), 0) { }

        // Coût total de la tournée
        public float Cost
        {
            get { return cout; }
        }

        // Séquence ordonnée des noms de sommets visités
        public IList<string> Vertices
        {
            get { return new List<string>(sommets); }
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
            Console.WriteLine($"Coût total : {cout}");
            foreach ((string source, string destination) in segments)
            {
                Console.WriteLine($"  {source} -> {destination}");
            }
        }

        // Reconstruit la liste ordonnée de sommets à partir d'une liste de segments non ordonnés
        private static List<string> ReconstruireSommets(List<(string source, string destination)> segs)
        {
            if (segs.Count == 0)
                return new List<string>();

            string depart = segs[0].source;
            List<string> resultat = new List<string> { depart };
            string courant = depart;

            for (int etape = 0; etape < segs.Count; etape++)
            {
                int idx = segs.FindIndex(s => s.source == courant);
                if (idx < 0)
                    break;
                courant = segs[idx].destination;
                resultat.Add(courant);
            }

            return resultat;
        }

        // Reconstruit la liste de segments à partir d'une séquence ordonnée de sommets
        private static List<(string source, string destination)> ReconstruireSegments(List<string> listeSommets)
        {
            List<(string, string)> segs = new List<(string, string)>();
            for (int i = 0; i + 1 < listeSommets.Count; i++)
                segs.Add((listeSommets[i], listeSommets[i + 1]));
            return segs;
        }
    }
}
