
## Review Assignment
[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/0E1ES0GJ)

[![CI Build & Test](https://github.com/arcreane/asp-pokemonteam/actions/workflows/dotnet-build-and-test.yml/badge.svg)](https://github.com/arcreane/asp-pokemonteam/actions/workflows/dotnet-build-and-test.yml)

# 🎮 Arène des Légendes - ASP.NET MVC

## 📌 Description
**Arène des Légendes** est une application multijoueur développée en **ASP.NET MVC** permettant aux joueurs de s'affronter dans un système de combat stratégique.  
Ce projet exploite **Entity Framework** pour la gestion des données sous **SQL Server** et met en œuvre les meilleures pratiques de développement en ASP.NET.

---

## 🎯 Objectif
Le projet vise à :
- 🏗️ Développer un **backend unique** en ASP.NET MVC gérant les combats multijoueurs.
- 🛠️ Assurer une **modularité et une structuration** claire du code.
- 🗄️ Concevoir une **base de données relationnelle** pour stocker les joueurs, Pokémon et objets.
- 🚀 Déployer l'application sur un serveur pour démonstration.

---

## 🏗️ Architecture du Projet
📌 **Backend (commun)** : API REST en ASP.NET MVC  
📌 **Base de données** : SQL Server avec Entity Framework  
📌 **Frontend (individuel)** : Chaque étudiant développe sa propre interface utilisateur  
📌 **Gestion des versions** : Git avec workflow structuré (branches, commits clairs)  

---

## 🗂️ Modélisation des Données
### 📋 Tables Principales
- **`user_auth`** : Stocke les utilisateurs du jeu.
- **`player`** : Représente les joueurs et leurs statistiques.
- **`pokemon`** : Contient les Pokémon avec leurs attributs et évolutions.
- **`type`** : Définit les types de Pokémon avec leurs effets.
- **`skill`** : Gère les attaques des Pokémon.
- **`object`** : Représente les objets achetables en jeu.
- **`log`** : Historique des actions dans le jeu.

💡 **Relations** : Plusieurs relations en **clé étrangère** pour les interactions entre entités.

---

## ⚙️ Technologies Utilisées
- ✅ **ASP.NET MVC** (backend)
- ✅ **Entity Framework** (ORM)
- ✅ **SQL Server** (base de données)
- ✅ **C#** (langage de programmation)
- ✅ **Git/GitHub** (gestion de version)
- ✅ **HTML/CSS/JS** (pour les frontends)

---

## 🚀 Installation et Lancement

### 1️⃣ Prérequis
- [ ] **.NET SDK 6+**
- [ ] **SQL Server**
- [ ] **Git**

### 2️⃣ Cloner le dépôt
```bash
git clone https://github.com/votre-utilisateur/arena-legendes.git
cd arena-legendes
```

### 3️⃣ Configuration de la base de données
Modifier **`appsettings.json`** pour configurer la connexion SQL Server :
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=ArenaLegendesDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Appliquer les migrations :
```bash
dotnet ef database update
```

### 4️⃣ Lancer l’application
```bash
dotnet run
```
L’API sera disponible sur `http://localhost:5000`.

---

## 📜 Fonctionnalités Principales
✔️ Gestion des joueurs et de leurs Pokémon  
✔️ Système de combat en tour par tour  
✔️ Attribution des compétences et évolutions  
✔️ Économie en jeu avec achats d'objets  
✔️ Historique des combats  

---

## 🔥 Fonctionnalités Avancées (Bonus)
- 🎖️ **Classement** des meilleurs joueurs
- 🧠 **IA des adversaires**
- 🎨 **Animations et effets visuels**
- 📜 **Quêtes et événements aléatoires**

---

## 👥 Contributions
- **Backend commun** : Développé en équipe.
- **Frontends individuels** : Chaque étudiant crée son interface utilisateur.
- **Base de données** : Collaboration sur la modélisation et les migrations.

---

## 📌 Critères d'Évaluation

| Critère | Points |
|---------|--------|
| Respect du pattern MVC | 3 |
| Qualité du code | 3 |
| Expérience utilisateur (UX) | 2 |
| Utilisation d'ASP.NET | 5 |
| Fonctionnalités implémentées | 5 |
| Workflow Git | 2 |

---

## 📜 Licence
Ce projet est sous licence **MIT**. Vous êtes libre de l'utiliser et de le modifier.

---

