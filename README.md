# Optimizer

Optimizer est une application WPF conçue pour le jeu Dofus Unity dont l'objectif est d'améliorer et rendre plus confortable le multicompte.
Elle permet de compenser/remplacer temporairement les fonctionnalités existantes en jeu qui provoquent régulièrement des bugs/crashs.
L'application comporte 4 fonctionnalités : Mouse Clone, Hotkey Clone, Window Switcher et Easy Team.

## Présentation des fonctionnalités

Mouse Clone :
Permet de répliquer les clics dans toutes les fenêtres spécifiées, qu'elles soient au premier plan ou non, grâce à un script AutoHotkey ainsi qu'un raccourci défini par l'utilisateur.

Hotkey Clone :
Permet de répliquer les touches dans toutes les fenêtres spécifiées, qu'elles soient au premier plan ou non, grâce à un script AutoHotkey ainsi qu'un raccourci défini par l'utilisateur.

Window Switcher :
Permet de basculer d'une fenêtre du jeu à une autre fenêtre du jeu, en suivant l'ordre défini des fenêtres (équivalent au raccourci Alt+Echap) grâce à un raccourci défini par l'utilisateur.

Easy Team :
Permet d'inviter en groupe les personnages correspondants aux fenêtres spécifiées depuis la fenêtre du personnage défini en temps que chef (équivalent à /invite NomDuPersonnage dans le chat du jeu).

## Fonctionnalités existantes

- Fonctionnalité Mouse Clone : Réplication de clics sur plusieurs fenêtres de Dofus.
- Configuration facile d'un raccourci clavier pour la fonctionnalité Mouse Clone, avec prise en charge des touches/combinaisons de touches (Ctrl, Shift, etc.)/boutons de la souris.
- Possibilité de définir jusqu'à 3 fenêtres cibles du jeu.
- Interface utilisateur simple et intuitive.

## Fonctionnalités et modifications à venir

- Fonctionnalité Hotkey Clone
- Fonctionnalité Window Switcher
- Fonctionnalité Easy Team
- Ajout d'un bouton dans l'interface pour chaque personnage pour définir le chef d'équipe (pour invitation via Easy Team)
- Ajout de la réduction dans la barre des taches et d'un menu contextuel lors du clic sur l'icone
- Ajout d'un délai minimum et maximum pour Mouse Clone et Hotkey Clone
- Ajout d'un fichier settings.ini pour sauvegarder l'état de l'application à la fermeture
- Liaison avec l'UI moderne pour plus de confort

## Prérequis

- [Visual Studio Community 2022](https://visualstudio.microsoft.com/fr/vs/community/) (ou une version ultérieure).
- [AutoHotkey v2](https://www.autohotkey.com/) (inclus dans le projet).
- Le jeu Dofus Unity (DirectX 11) installé.