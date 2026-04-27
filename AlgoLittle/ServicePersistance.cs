using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TourneeFutee
{
    // Service de persistance permettant de sauvegarder et charger
    // des graphes et des tournées dans une base de données MySQL.
    public class ServicePersistance
    {
        private readonly string chaineConnexion;

        /// <summary>
        /// Instancie un service de persistance et se connecte automatiquement
        /// à la base de données <paramref name="dbname"/> sur le serveur
        /// à l'adresse IP <paramref name="serverIp"/>.
        /// Les identifiants sont définis par <paramref name="user"/> (utilisateur)
        /// et <paramref name="pwd"/> (mot de passe).
        /// </summary>
        /// <param name="serverIp">Adresse IP du serveur MySQL.</param>
        /// <param name="dbname">Nom de la base de données.</param>
        /// <param name="user">Nom d'utilisateur.</param>
        /// <param name="pwd">Mot de passe.</param>
        /// <exception cref="Exception">Levée si la connexion échoue.</exception>
        public ServicePersistance(string serverIp, string dbname, string user, string pwd)
        {
            chaineConnexion = $"server={serverIp};database={dbname};uid={user};pwd={pwd};";

            using (MySqlConnection conn = OuvrirConnexion())
            {
                // Si l'ouverture ne lève pas d'exception, la connexion est valide.
            }
        }

        /// <summary>
        /// Sauvegarde le graphe <paramref name="g"/> en base de données
        /// (sommets et arcs inclus) et renvoie son identifiant.
        /// </summary>
        /// <param name="g">Le graphe à sauvegarder.</param>
        /// <returns>Identifiant du graphe en base de données (AUTO_INCREMENT).</returns>
        public uint SaveGraph(Graph g)
        {
            using (MySqlConnection conn = OuvrirConnexion())
            {
                // 1. Insérer le graphe
                MySqlCommand cmdGraphe = conn.CreateCommand();
                cmdGraphe.CommandText = "INSERT INTO Graphe (est_oriente) VALUES (@oriente);";
                cmdGraphe.Parameters.AddWithValue("@oriente", g.Directed ? 1 : 0);
                cmdGraphe.ExecuteNonQuery();
                uint idGraphe = (uint)cmdGraphe.LastInsertedId;
                cmdGraphe.Dispose();

                // 2. Insérer les sommets et conserver la correspondance nom -> id base de données
                List<string> noms = g.VertexNames;
                Dictionary<string, uint> idsSommets = new Dictionary<string, uint>();

                foreach (string nom in noms)
                {
                    MySqlCommand cmdSommet = conn.CreateCommand();
                    cmdSommet.CommandText = "INSERT INTO Sommet (graphe_id, nom, valeur) VALUES (@gid, @nom, @valeur);";
                    cmdSommet.Parameters.AddWithValue("@gid", idGraphe);
                    cmdSommet.Parameters.AddWithValue("@nom", nom);
                    cmdSommet.Parameters.AddWithValue("@valeur", g.GetVertexValue(nom));
                    cmdSommet.ExecuteNonQuery();
                    idsSommets[nom] = (uint)cmdSommet.LastInsertedId;
                    cmdSommet.Dispose();
                }

                // 3. Insérer les arcs
                // Pour un graphe non orienté, on ne stocke qu'un seul sens (i < j)
                // pour éviter les doublons au rechargement.
                for (int i = 0; i < noms.Count; i++)
                {
                    for (int j = 0; j < noms.Count; j++)
                    {
                        if (i == j)
                            continue;
                        if (!g.Directed && j < i)
                            continue;

                        float poids;
                        try
                        {
                            poids = g.GetEdgeWeight(noms[i], noms[j]);
                        }
                        catch (ArgumentException)
                        {
                            continue;
                        }

                        MySqlCommand cmdArc = conn.CreateCommand();
                        cmdArc.CommandText = "INSERT INTO Arc (graphe_id, sommet_source, sommet_dest, poids) VALUES (@gid, @src, @dst, @poids);";
                        cmdArc.Parameters.AddWithValue("@gid", idGraphe);
                        cmdArc.Parameters.AddWithValue("@src", idsSommets[noms[i]]);
                        cmdArc.Parameters.AddWithValue("@dst", idsSommets[noms[j]]);
                        cmdArc.Parameters.AddWithValue("@poids", poids);
                        cmdArc.ExecuteNonQuery();
                        cmdArc.Dispose();
                    }
                }

                return idGraphe;
            }
        }

        /// <summary>
        /// Charge depuis la base de données le graphe identifié par <paramref name="id"/>
        /// et renvoie une instance de la classe <see cref="Graph"/>.
        /// </summary>
        /// <param name="id">Identifiant du graphe à charger.</param>
        /// <returns>Instance de <see cref="Graph"/> reconstituée.</returns>
        public Graph LoadGraph(uint id)
        {
            using (MySqlConnection conn = OuvrirConnexion())
            {
                // 1. Charger les métadonnées du graphe
                MySqlCommand cmdGraphe = conn.CreateCommand();
                cmdGraphe.CommandText = "SELECT est_oriente FROM Graphe WHERE id = @id;";
                cmdGraphe.Parameters.AddWithValue("@id", id);
                MySqlDataReader lecteurGraphe = cmdGraphe.ExecuteReader();

                if (!lecteurGraphe.Read())
                {
                    lecteurGraphe.Close();
                    cmdGraphe.Dispose();
                    throw new ArgumentException($"Graphe avec id={id} introuvable.");
                }

                bool oriente = lecteurGraphe.GetInt32("est_oriente") == 1;
                lecteurGraphe.Close();
                cmdGraphe.Dispose();

                Graph g = new Graph(directed: oriente);

                // 2. Charger les sommets dans l'ordre d'insertion (tri par id)
                MySqlCommand cmdSommets = conn.CreateCommand();
                cmdSommets.CommandText = "SELECT id, nom, valeur FROM Sommet WHERE graphe_id = @gid ORDER BY id;";
                cmdSommets.Parameters.AddWithValue("@gid", id);
                MySqlDataReader lecteurSommets = cmdSommets.ExecuteReader();

                Dictionary<uint, string> idVersNom = new Dictionary<uint, string>();
                while (lecteurSommets.Read())
                {
                    uint idSommet = lecteurSommets.GetUInt32("id");
                    string nom = lecteurSommets.GetString("nom");
                    float valeur = lecteurSommets.GetFloat("valeur");
                    g.AddVertex(nom, valeur);
                    idVersNom[idSommet] = nom;
                }
                lecteurSommets.Close();
                cmdSommets.Dispose();

                // 3. Charger les arcs
                MySqlCommand cmdArcs = conn.CreateCommand();
                cmdArcs.CommandText = "SELECT sommet_source, sommet_dest, poids FROM Arc WHERE graphe_id = @gid;";
                cmdArcs.Parameters.AddWithValue("@gid", id);
                MySqlDataReader lecteurArcs = cmdArcs.ExecuteReader();

                while (lecteurArcs.Read())
                {
                    uint idSource = lecteurArcs.GetUInt32("sommet_source");
                    uint idDest = lecteurArcs.GetUInt32("sommet_dest");
                    float poids = lecteurArcs.GetFloat("poids");
                    g.AddEdge(idVersNom[idSource], idVersNom[idDest], poids);
                }
                lecteurArcs.Close();
                cmdArcs.Dispose();

                return g;
            }
        }

        /// <summary>
        /// Sauvegarde la tournée <paramref name="t"/> (effectuée dans le graphe
        /// identifié par <paramref name="graphId"/>) en base de données
        /// et renvoie son identifiant.
        /// </summary>
        /// <param name="graphId">Identifiant BdD du graphe dans lequel la tournée a été calculée.</param>
        /// <param name="t">La tournée à sauvegarder.</param>
        /// <returns>Identifiant de la tournée en base de données (AUTO_INCREMENT).</returns>
        public uint SaveTour(uint graphId, Tour t)
        {
            using (MySqlConnection conn = OuvrirConnexion())
            {
                // 1. Insérer la tournée
                MySqlCommand cmdTournee = conn.CreateCommand();
                cmdTournee.CommandText = "INSERT INTO Tournee (graphe_id, cout_total) VALUES (@gid, @cout);";
                cmdTournee.Parameters.AddWithValue("@gid", graphId);
                cmdTournee.Parameters.AddWithValue("@cout", t.Cost);
                cmdTournee.ExecuteNonQuery();
                uint idTournee = (uint)cmdTournee.LastInsertedId;
                cmdTournee.Dispose();

                // 2. Insérer les étapes : récupérer l'id base de données de chaque sommet
                IList<string> sequence = t.Vertices;
                for (int ordre = 0; ordre < sequence.Count; ordre++)
                {
                    string nomSommet = sequence[ordre];

                    MySqlCommand cmdIdSommet = conn.CreateCommand();
                    cmdIdSommet.CommandText = "SELECT id FROM Sommet WHERE graphe_id = @gid AND nom = @nom LIMIT 1;";
                    cmdIdSommet.Parameters.AddWithValue("@gid", graphId);
                    cmdIdSommet.Parameters.AddWithValue("@nom", nomSommet);
                    uint idSommet = Convert.ToUInt32(cmdIdSommet.ExecuteScalar());
                    cmdIdSommet.Dispose();

                    MySqlCommand cmdEtape = conn.CreateCommand();
                    cmdEtape.CommandText = "INSERT INTO EtapeTournee (tournee_id, numero_ordre, sommet_id) VALUES (@tid, @ordre, @sid);";
                    cmdEtape.Parameters.AddWithValue("@tid", idTournee);
                    cmdEtape.Parameters.AddWithValue("@ordre", ordre);
                    cmdEtape.Parameters.AddWithValue("@sid", idSommet);
                    cmdEtape.ExecuteNonQuery();
                    cmdEtape.Dispose();
                }

                return idTournee;
            }
        }

        /// <summary>
        /// Charge depuis la base de données la tournée identifiée par <paramref name="id"/>
        /// et renvoie une instance de la classe <see cref="Tour"/>.
        /// </summary>
        /// <param name="id">Identifiant de la tournée à charger.</param>
        /// <returns>Instance de <see cref="Tour"/> reconstituée.</returns>
        public Tour LoadTour(uint id)
        {
            using (MySqlConnection conn = OuvrirConnexion())
            {
                // 1. Charger le coût total
                MySqlCommand cmdTournee = conn.CreateCommand();
                cmdTournee.CommandText = "SELECT cout_total FROM Tournee WHERE id = @id;";
                cmdTournee.Parameters.AddWithValue("@id", id);
                MySqlDataReader lecteurTournee = cmdTournee.ExecuteReader();

                if (!lecteurTournee.Read())
                {
                    lecteurTournee.Close();
                    cmdTournee.Dispose();
                    throw new ArgumentException($"Tournée avec id={id} introuvable.");
                }

                float cout = lecteurTournee.GetFloat("cout_total");
                lecteurTournee.Close();
                cmdTournee.Dispose();

                // 2. Charger les étapes dans l'ordre
                MySqlCommand cmdEtapes = conn.CreateCommand();
                cmdEtapes.CommandText =
                    "SELECT s.nom FROM EtapeTournee e " +
                    "JOIN Sommet s ON e.sommet_id = s.id " +
                    "WHERE e.tournee_id = @tid ORDER BY e.numero_ordre;";
                cmdEtapes.Parameters.AddWithValue("@tid", id);
                MySqlDataReader lecteurEtapes = cmdEtapes.ExecuteReader();

                List<string> sequence = new List<string>();
                while (lecteurEtapes.Read())
                {
                    sequence.Add(lecteurEtapes.GetString("nom"));
                }
                lecteurEtapes.Close();
                cmdEtapes.Dispose();

                return new Tour(sequence, cout);
            }
        }

        /// <summary>
        /// Crée et retourne une nouvelle connexion MySQL ouverte.
        /// Encadrez toujours l'appel dans un bloc using pour garantir la fermeture.
        /// </summary>
        private MySqlConnection OuvrirConnexion()
        {
            MySqlConnection conn = new MySqlConnection(chaineConnexion);
            conn.Open();
            return conn;
        }
    }
}
