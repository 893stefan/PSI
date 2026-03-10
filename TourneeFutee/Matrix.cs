using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Matrix
    {
        private readonly float valeurParDefaut;
        private readonly List<List<float>> matrice;

        /* Crée une matrice de dimensions `nbRows` x `nbColums`.
         * Toutes les cases de cette matrice sont remplies avec `defaultValue`.
         * Lève une ArgumentOutOfRangeException si une des dimensions est négative
         */
        public Matrix(int nbRows = 0, int nbColumns = 0, float defaultValue = 0)
        {
            if (nbRows < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nbRows));
            }

            if (nbColumns < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nbColumns));
            }

            valeurParDefaut = defaultValue;
            matrice = new List<List<float>>();

            for (int i = 0; i < nbRows; i++)
            {
                List<float> ligne = new List<float>();
                for (int j = 0; j < nbColumns; j++)
                {
                    ligne.Add(defaultValue);
                }

                matrice.Add(ligne);
            }
        }

        // Valeur par défaut des nouvelles cases
        public float DefaultValue
        {
            get { return valeurParDefaut; }
        }

        // Nombre de lignes
        public int NbRows
        {
            get { return matrice.Count; }
        }

        // Nombre de colonnes
        public int NbColumns
        {
            get
            {
                if (NbRows == 0)
                {
                    return 0;
                }

                return matrice[0].Count;
            }
        }

        /* Insère une ligne à l'indice `i`. Décale les lignes suivantes vers le bas.
         * Toutes les cases de la nouvelle ligne contiennent DefaultValue.
         * Si `i` = NbRows, insère une ligne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `i` est en dehors des indices valides
         */
        public void AddRow(int i)
        {
            if (i < 0 || i > NbRows)
            {
                throw new ArgumentOutOfRangeException(nameof(i));
            }

            List<float> ligne = new List<float>();
            for (int j = 0; j < NbColumns; j++)
            {
                ligne.Add(DefaultValue);
            }

            matrice.Insert(i, ligne);
        }

        /* Insère une colonne à l'indice `j`. Décale les colonnes suivantes vers la droite.
         * Toutes les cases de la nouvelle ligne contiennent DefaultValue.
         * Si `j` = NbColums, insère une colonne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `j` est en dehors des indices valides
         */
        public void AddColumn(int j)
        {
            if (j < 0 || j > NbColumns)
            {
                throw new ArgumentOutOfRangeException(nameof(j));
            }

            for (int i = 0; i < NbRows; i++)
            {
                matrice[i].Insert(j, DefaultValue);
            }
        }

        // Supprime une ligne
        public void RemoveRow(int i)
        {
            VerifierIndiceLigne(i);
            matrice.RemoveAt(i);
        }

        // Supprime une colonne
        public void RemoveColumn(int j)
        {
            VerifierIndiceColonne(j);

            for (int i = 0; i < NbRows; i++)
            {
                matrice[i].RemoveAt(j);
            }
        }

        // Renvoie la valeur d'une case
        public float GetValue(int i, int j)
        {
            VerifierIndiceLigne(i);
            VerifierIndiceColonne(j);
            return matrice[i][j];
        }

        // Change la valeur d'une case
        public void SetValue(int i, int j, float v)
        {
            VerifierIndiceLigne(i);
            VerifierIndiceColonne(j);
            matrice[i][j] = v;
        }

        // Affiche la matrice dans la console
        public void Print()
        {
            for (int i = 0; i < NbRows; i++)
            {
                for (int j = 0; j < NbColumns; j++)
                {
                    Console.Write(GetValue(i, j) + " ");
                }

                Console.WriteLine();
            }
        }

        private void VerifierIndiceLigne(int i)
        {
            if (i < 0 || i >= NbRows)
            {
                throw new ArgumentOutOfRangeException(nameof(i));
            }
        }

        private void VerifierIndiceColonne(int j)
        {
            if (j < 0 || j >= NbColumns)
            {
                throw new ArgumentOutOfRangeException(nameof(j));
            }
        }
    }
}
