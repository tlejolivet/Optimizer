# Optimizer

Optimizer est une application WPF con�ue pour le jeu Dofus Unity dont l'objectif est d'am�liorer et rendre plus confortable le multicompte.
Elle permet de compenser/remplacer temporairement les fonctionnalit�s existantes en jeu qui provoquent r�guli�rement des bugs/crashs.
L'application comporte 4 fonctionnalit�s : Mouse Clone, Hotkey Clone, Window Switcher et Easy Team.

## Pr�sentation des fonctionnalit�s

Mouse Clone :
Permet de r�pliquer les clics dans toutes les fen�tres sp�cifi�es, qu'elles soient au premier plan ou non, gr�ce � un script AutoHotkey ainsi qu'un raccourci d�fini par l'utilisateur.

Hotkey Clone :
Permet de r�pliquer les touches dans toutes les fen�tres sp�cifi�es, qu'elles soient au premier plan ou non, gr�ce � un script AutoHotkey ainsi qu'un raccourci d�fini par l'utilisateur.

Window Switcher :
Permet de basculer d'une fen�tre du jeu � une autre fen�tre du jeu, en suivant l'ordre d�fini des fen�tres (�quivalent au raccourci Alt+Echap) gr�ce � un raccourci d�fini par l'utilisateur.

Easy Team :
Permet d'inviter en groupe les personnages correspondants aux fen�tres sp�cifi�es depuis la fen�tre du personnage d�fini en temps que chef (�quivalent � /invite NomDuPersonnage dans le chat du jeu).

## Fonctionnalit�s existantes

- Fonctionnalit� Mouse Clone : R�plication de clics sur plusieurs fen�tres de Dofus.
- Configuration facile d'un raccourci clavier pour la fonctionnalit� Mouse Clone, avec prise en charge des touches/combinaisons de touches (Ctrl, Shift, etc.)/boutons de la souris.
- Possibilit� de d�finir jusqu'� 3 fen�tres cibles du jeu.
- Interface utilisateur simple et intuitive.

## Fonctionnalit�s et modifications � venir

- Fonctionnalit� Hotkey Clone
- Fonctionnalit� Window Switcher
- Fonctionnalit� Easy Team
- Ajout d'un bouton dans l'interface pour chaque personnage pour d�finir le chef d'�quipe (pour invitation via Easy Team)
- Ajout de la r�duction dans la barre des taches et d'un menu contextuel lors du clic sur l'icone
- Ajout d'un d�lai minimum et maximum pour Mouse Clone et Hotkey Clone
- Ajout d'un fichier settings.ini pour sauvegarder l'�tat de l'application � la fermeture
- Liaison avec l'UI moderne pour plus de confort

## Pr�requis

- [Visual Studio Community 2022](https://visualstudio.microsoft.com/fr/vs/community/) (ou une version ult�rieure).
- [AutoHotkey v2](https://www.autohotkey.com/) (inclus dans le projet).
- Le jeu Dofus Unity (DirectX 11) install�.