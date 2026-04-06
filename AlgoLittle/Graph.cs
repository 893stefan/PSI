using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Graph
    {
        private readonly bool oriente;
        private readonly float valeurSansArc;
        private readonly List<string> nomsSommets;
        private readonly List<float> valeursSommets;
        private readonly Matrix matriceAdjacence;

        // Construit un graphe orienté ou non
        public Graph(bool directed, float noEdgeValue = 0)
        {
            oriente = directed;
            valeurSansArc = noEdgeValue;
            nomsSommets = new List<string>();
            valeursSommets = new List<float>();
            matriceAdjacence = new Matrix(defaultValue: noEdgeValue);
        }

        // Nombre de sommets
        public int Order
        {
            get { return nomsSommets.Count; }
        }

        // Graphe orienté ou non
        public bool Directed
        {
            get { return oriente; }
        }

        // Noms de tous les sommets dans l'ordre d'insertion
        public List<string> VertexNames
        {
            get { return new List<string>(nomsSommets); }
        }

        // Ajoute un sommet
        public void AddVertex(string name, float value = 0)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (nomsSommets.Contains(name))
            {
                throw new ArgumentException("Un sommet avec ce nom existe déjà.", nameof(name));
            }

            nomsSommets.Add(name);
            valeursSommets.Add(value);
            matriceAdjacence.AddRow(matriceAdjacence.NbRows);
            matriceAdjacence.AddColumn(matriceAdjacence.NbColumns);
        }

        // Supprime un sommet
        public void RemoveVertex(string name)
        {
            int indice = GetVertexIndex(name);

            nomsSommets.RemoveAt(indice);
            valeursSommets.RemoveAt(indice);
            matriceAdjacence.RemoveRow(indice);
            matriceAdjacence.RemoveColumn(indice);
        }

        // Renvoie la valeur d'un sommet
        public float GetVertexValue(string name)
        {
            int indice = GetVertexIndex(name);
            return valeursSommets[indice];
        }

        // Modifie la valeur d'un sommet
        public void SetVertexValue(string name, float value)
        {
            int indice = GetVertexIndex(name);
            valeursSommets[indice] = value;
        }

        // Renvoie les voisins d'un sommet
        public List<string> GetNeighbors(string vertexName)
        {
            int indiceSommet = GetVertexIndex(vertexName);
            List<string> voisins = new List<string>();

            for (int j = 0; j < Order; j++)
            {
                if (ArcExiste(indiceSommet, j))
                {
                    voisins.Add(nomsSommets[j]);
                }
            }

            return voisins;
        }

        /* Ajoute un arc allant du sommet nommé "sourceName" au sommet nommé "destinationName", avec le poids "weight" (1 par défaut)
         * Si le graphe n'est pas orienté, ajoute aussi l'arc inverse, avec le même poids
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - il existe déjà un arc avec ces extrémités
         */
        public void AddEdge(string sourceName, string destinationName, float weight = 1)
        {
            int indiceSource = GetVertexIndex(sourceName);
            int indiceDestination = GetVertexIndex(destinationName);

            if (ArcExiste(indiceSource, indiceDestination))
            {
                throw new ArgumentException("Un arc avec ces sommets existe déjà.");
            }

            matriceAdjacence.SetValue(indiceSource, indiceDestination, weight);
            if (!oriente)
            {
                matriceAdjacence.SetValue(indiceDestination, indiceSource, weight);
            }
        }

        /* Supprime l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName` du graphe
         * Si le graphe n'est pas orienté, supprime aussi l'arc inverse
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - l'arc n'existe pas
         */
        public void RemoveEdge(string sourceName, string destinationName)
        {
            int indiceSource = GetVertexIndex(sourceName);
            int indiceDestination = GetVertexIndex(destinationName);

            if (!ArcExiste(indiceSource, indiceDestination))
            {
                throw new ArgumentException("L'arc n'existe pas.");
            }

            matriceAdjacence.SetValue(indiceSource, indiceDestination, valeurSansArc);
            if (!oriente)
            {
                matriceAdjacence.SetValue(indiceDestination, indiceSource, valeurSansArc);
            }
        }

        /* Renvoie le poids de l'arc allant du sommet nommé "sourceName" au sommet nommé "destinationName"
         * Si le graphe n'est pas orienté, GetEdgeWeight(A, B) = GetEdgeWeight(B, A)
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - l'arc n'existe pas
         */
        public float GetEdgeWeight(string sourceName, string destinationName)
        {
            int indiceSource = GetVertexIndex(sourceName);
            int indiceDestination = GetVertexIndex(destinationName);

            if (!ArcExiste(indiceSource, indiceDestination))
            {
                throw new ArgumentException("L'arc n'existe pas.");
            }

            return matriceAdjacence.GetValue(indiceSource, indiceDestination);
        }

        /* Affecte le poids l'arc allant du sommet nommé "sourceName" au sommet nommé "destinationName" à "weight"
         * Si le graphe n'est pas orienté, affecte le même poids à l'arc inverse
         * Lève une ArgumentException si un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         */
        public void SetEdgeWeight(string sourceName, string destinationName, float weight)
        {
            int indiceSource = GetVertexIndex(sourceName);
            int indiceDestination = GetVertexIndex(destinationName);

            matriceAdjacence.SetValue(indiceSource, indiceDestination, weight);
            if (!oriente)
            {
                matriceAdjacence.SetValue(indiceDestination, indiceSource, weight);
            }
        }

        private int GetVertexIndex(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            int indice = nomsSommets.IndexOf(name);
            if (indice < 0)
            {
                throw new ArgumentException("Sommet introuvable.", nameof(name));
            }

            return indice;
        }

        private bool ArcExiste(int indiceSource, int indiceDestination)
        {
            return matriceAdjacence.GetValue(indiceSource, indiceDestination) != valeurSansArc;
        }
    }
}
