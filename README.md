# Objectif 1 — Graphe en C#

Projet réalisé dans le cadre du module **Problème Scientifique & Informatique (PSI)** (MESIIN240125) à l'ESILV.

L'objectif de ce projet est d'implémenter une structure de graphe en C# en utilisant une matrice d'adjacence.

## Objectifs du projet

- Implémenter un graphe **orienté ou non orienté**
- Représenter les arcs avec une **matrice d'adjacence**
- Manipuler les **sommets et arcs du graphe**
- Appliquer les principes de **programmation orientée objet**
- Valider l'implémentation avec des **tests unitaires**

## Fonctionnalités

Le graphe permet notamment :

- l'ajout de sommets
- l'ajout d'arcs pondérés
- l'accès à la valeur d'un sommet
- la gestion des graphes orientés ou non orientés

## Structure du projet

Le projet repose sur les deux classes suivantes :

- **Graph** : gestion des sommets et des arcs
- **Matrix** : gestion de la matrice d'adjacence

Les sommets sont identifiés par un nom et une valeur.

Les arcs sont stockés dans la matrice d'adjacence.

## Tests

Le projet utilise **MSTest** pour vérifier le bon fonctionnement des classes.

Prérequis :

- SDK .NET 8.0

Deux campagnes de tests sont prévues :

- 20 tests unitaires pour la classe `Matrix`
- 20 tests unitaires pour la classe `Graph`

Pour lancer les tests :

```bash
dotnet test
```

## Crédits

[voir LICENSE]
