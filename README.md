# Objectif 2 — Algorithme de Little en C#

Projet réalisé dans le cadre du module **Problème Scientifique & Informatique (PSI)** (MESIIN240125) à l'ESILV.

L'objectif de ce projet est d'implémenter l'algorithme de Little pour résoudre le problème du voyageur de commerce (TSP) en C#.

## Objectifs du projet

- Modéliser une **tournée** (cycle hamiltonien) dans un graphe
- Implémenter l'**algorithme de Little** (branch & bound) pour trouver la tournée optimale
- Traiter les cas **symétriques** (graphe non orienté) et **asymétriques** (graphe orienté)
- Valider l'implémentation avec des **tests unitaires**

## Fonctionnalités

L'algorithme de Little permet de :

- **Réduire** une matrice de coûts (réduction par lignes et colonnes)
- Calculer le **regret maximal** pour choisir l'arc à brancher
- Détecter les **trajets parasites** (sous-tournées prématurées)
- Trouver le **cycle hamiltonien de plus faible coût** par exploration branch & bound

## Structure du projet

Le projet s'appuie sur les classes de l'Objectif 1 (`Graph`, `Matrix`) et ajoute :

- **Tour** : modélise une tournée (liste de trajets et coût total)
- **Little** : résout le TSP par l'algorithme de Little

## Tests

Le projet utilise **MSTest** pour vérifier le bon fonctionnement des classes.

Prérequis :

- SDK .NET 8.0

Deux parties de tests sont prévues :

- **Partie 1** — test des étapes de l'algorithme : réduction de matrice, calcul du regret maximal, détection des trajets parasites
- **Partie 2** — test de l'algorithme complet sur un problème symétrique (6 villes françaises, coût optimal = 2437 km) et un problème asymétrique (6 villes A–F, coût optimal = 20)

Pour lancer les tests :

```bash
dotnet test
```

## Crédits

[voir LICENSE]
