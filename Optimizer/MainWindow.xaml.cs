using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Optimizer
{
    public partial class MainWindow : Window
    {
        private Process _ahkProcess; // Pour garder une référence au processus AHK
        private bool _isScriptActive = false; // Pour suivre l'état du script
        private bool _isWaitingForShortcut = false; // Pour savoir si on attend un raccourci

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;

            // Ajouter des gestionnaires d'événements pour les checkboxes
            Window1CheckBox.Checked += CheckBox_CheckedChanged;
            Window1CheckBox.Unchecked += CheckBox_CheckedChanged;

            Window2CheckBox.Checked += CheckBox_CheckedChanged;
            Window2CheckBox.Unchecked += CheckBox_CheckedChanged;

            Window3CheckBox.Checked += CheckBox_CheckedChanged;
            Window3CheckBox.Unchecked += CheckBox_CheckedChanged;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Arrêter le script AHK lorsque la fenêtre est fermée
            StopScript();
        }

        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Mettre à jour le script AHK si le script est actif
            if (_isScriptActive)
            {
                UpdateScript();
            }
        }

        private void ToggleScriptButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isScriptActive)
            {
                // Désactiver le script
                StopScript();
                ToggleScriptButton.Content = "Activer le script";
                _isScriptActive = false;
            }
            else
            {
                // Activer le script
                string shortcut = ShortcutButton.Content.ToString().Trim();
                if (string.IsNullOrEmpty(shortcut) || shortcut == "Entrer un raccourci")
                {
                    MessageBox.Show("Veuillez entrer un raccourci clavier.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                StartScript(shortcut);
                ToggleScriptButton.Content = "Désactiver le script";
                _isScriptActive = true;
            }
        }

        private void StartScript(string shortcut)
        {
            // Arrêter le script actuel s'il est déjà en cours d'exécution
            StopScript();

            // Générer le script AHK dynamiquement
            string ahkScriptContent = GenerateAhkScript(shortcut);
            string ahkScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClickReplicated.ahk");

            // Écrire le script dans un fichier
            File.WriteAllText(ahkScriptPath, ahkScriptContent);

            // Vérifier si AutoHotkey.exe existe
            string ahkExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoHotkey64.exe");
            if (!File.Exists(ahkExePath))
            {
                MessageBox.Show($"AutoHotkey64.exe n'a pas été trouvé à l'emplacement : {ahkExePath}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Exécuter le script AutoHotkey
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ahkExePath,
                    Arguments = $"\"{ahkScriptPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                _ahkProcess = Process.Start(startInfo);
                Console.WriteLine("Script activé !");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exécution d'AutoHotkey : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopScript()
        {
            // Arrêter le processus AHK
            if (_ahkProcess != null && !_ahkProcess.HasExited)
            {
                _ahkProcess.Kill();
                _ahkProcess = null;
                Console.WriteLine("Script désactivé !");
            }
        }

        private string GenerateAhkScript(string shortcut)
        {
            // Récupérer les noms des fenêtres et l'état des checkboxes
            string window1 = Window1TextBox.Text.Trim();
            string window2 = Window2TextBox.Text.Trim();
            string window3 = Window3TextBox.Text.Trim();

            bool isWindow1Active = Window1CheckBox.IsChecked == true;
            bool isWindow2Active = Window2CheckBox.IsChecked == true;
            bool isWindow3Active = Window3CheckBox.IsChecked == true;

            // Convertir le raccourci en nom AHK
            string ahkShortcut = TranslateShortcutToAhk(shortcut);

            // Générer le script AHK dynamiquement
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine("; Masquer l'icône AutoHotkey dans la barre des tâches");
            scriptBuilder.AppendLine("#NoTrayIcon");
            scriptBuilder.AppendLine();

            // Ajouter ~ pour ne pas bloquer le clic gauche
            if (ahkShortcut == "LButton")
            {
                ahkShortcut = "~LButton";
            }

            // Limiter l'action du raccourci aux fenêtres spécifiques
            scriptBuilder.AppendLine($"; Raccourci clavier défini par l'utilisateur");
            scriptBuilder.AppendLine($"{ahkShortcut}::");
            scriptBuilder.AppendLine("{");

            if (isWindow1Active)
            {
                scriptBuilder.AppendLine($"    if WinExist(\"{window1}\")");
                scriptBuilder.AppendLine("    {");
                scriptBuilder.AppendLine($"        ControlClick(, \"{window1}\",, \"left\")");
                scriptBuilder.AppendLine("    }");
            }

            if (isWindow2Active)
            {
                scriptBuilder.AppendLine($"    if WinExist(\"{window2}\")");
                scriptBuilder.AppendLine("    {");
                scriptBuilder.AppendLine($"        ControlClick(, \"{window2}\",, \"left\")");
                scriptBuilder.AppendLine("    }");
            }

            if (isWindow3Active)
            {
                scriptBuilder.AppendLine($"    if WinExist(\"{window3}\")");
                scriptBuilder.AppendLine("    {");
                scriptBuilder.AppendLine($"        ControlClick(, \"{window3}\",, \"left\")");
                scriptBuilder.AppendLine("    }");
            }

            scriptBuilder.AppendLine("}");

            return scriptBuilder.ToString();
        }

        private string TranslateShortcutToAhk(string shortcut)
        {
            // Si le raccourci est un bouton de souris, le traduire en nom AHK
            if (shortcut.StartsWith("Bouton"))
            {
                return TranslateToAhkMouseButton(shortcut);
            }

            // Sinon, traiter comme une combinaison de touches
            string[] keys = shortcut.Split('+');
            StringBuilder ahkShortcutBuilder = new StringBuilder();

            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                string ahkKey = TranslateToAhkKey(key);

                if (i < keys.Length - 1)
                {
                    ahkShortcutBuilder.Append(ahkKey); // Ajouter le modificateur (Ctrl, Shift, Alt)
                }
                else
                {
                    ahkShortcutBuilder.Append(ahkKey); // Ajouter la touche principale
                }
            }

            return ahkShortcutBuilder.ToString();
        }

        private string TranslateToAhkKey(string key)
        {
            switch (key)
            {
                case "Ctrl":
                    return "^";
                case "Shift":
                    return "+";
                case "Alt":
                    return "!"; // Traduire "Alt" en "!" (équivalent de Alt en AHK)
                case "F1":
                case "F2":
                case "F3":
                case "F4":
                case "F5":
                case "F6":
                case "F7":
                case "F8":
                case "F9":
                case "F10":
                case "F11":
                case "F12":
                    return key; // Les touches F1 à F12 sont les mêmes
                case "Space":
                    return "Space";
                case "Enter":
                    return "Enter";
                case "Escape":
                    return "Escape";
                case "Back":
                    return "Backspace";
                case "Tab":
                    return "Tab";
                case "Capital":
                    return "CapsLock";
                case "Left":
                    return "Left";
                case "Right":
                    return "Right";
                case "Up":
                    return "Up";
                case "Down":
                    return "Down";
                case "PageUp":
                    return "PgUp";
                case "PageDown":
                    return "PgDn";
                case "Home":
                    return "Home";
                case "End":
                    return "End";
                case "Insert":
                    return "Insert";
                case "Delete":
                    return "Delete";
                case "NumPad0":
                case "NumPad1":
                case "NumPad2":
                case "NumPad3":
                case "NumPad4":
                case "NumPad5":
                case "NumPad6":
                case "NumPad7":
                case "NumPad8":
                case "NumPad9":
                    return key.Replace("NumPad", "Numpad"); // Correction de la casse
                case "Multiply":
                    return "NumpadMult";
                case "Add":
                    return "NumpadAdd";
                case "Subtract":
                    return "NumpadSub";
                case "Divide":
                    return "NumpadDiv";
                case "Decimal":
                    return "NumpadDot";
                default:
                    return key.ToLower(); // Par défaut, convertir en minuscules
            }
        }

        private string TranslateToAhkMouseButton(string frenchName)
        {
            switch (frenchName)
            {
                case "Bouton gauche":
                    return "LButton";
                case "Bouton droit":
                    return "RButton";
                case "Bouton du milieu":
                    return "MButton";
                case "Bouton latéral 1":
                    return "XButton1";
                case "Bouton latéral 2":
                    return "XButton2";
                default:
                    return "";
            }
        }

        private void ShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            // Afficher "Entrer un raccourci" et attendre la saisie
            ShortcutButton.Content = "Entrer un raccourci";
            _isWaitingForShortcut = true;

            // Capturer les événements clavier et souris
            this.PreviewKeyUp += MainWindow_PreviewKeyUp;
            this.PreviewMouseUp += MainWindow_PreviewMouseUp;
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!_isWaitingForShortcut)
            {
                return; // Ignorer si on n'attend pas de raccourci
            }

            // Ignorer la touche Alt (seule ou en combinaison) et System
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.System ||
                Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                // Ne rien faire et continuer d'attendre une touche valide
                return;
            }

            // Ignorer les touches modificatrices seules (Ctrl, Shift)
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                return;
            }

            // Capturer la combinaison de touches
            StringBuilder shortcutBuilder = new StringBuilder();

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                shortcutBuilder.Append("Ctrl+");
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                shortcutBuilder.Append("Shift+");
            }

            // Ajouter la touche principale
            string mainKey = e.Key.ToString();
            shortcutBuilder.Append(mainKey);

            // Mettre à jour le texte du bouton
            ShortcutButton.Content = shortcutBuilder.ToString();

            // Réinitialiser l'état d'attente
            _isWaitingForShortcut = false;

            // Désactiver les gestionnaires d'événements
            this.PreviewKeyUp -= MainWindow_PreviewKeyUp;
            this.PreviewMouseUp -= MainWindow_PreviewMouseUp;

            // Mettre à jour le script si actif
            if (_isScriptActive)
            {
                UpdateScript();
            }

            // Empêcher la saisie manuelle
            e.Handled = true;
        }

        private void MainWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isWaitingForShortcut)
            {
                return; // Ignorer si on n'attend pas de raccourci
            }

            // Capturer les clics de souris
            string mouseButton = TranslateMouseButton(e.ChangedButton);
            ShortcutButton.Content = mouseButton;

            // Réinitialiser l'état d'attente
            _isWaitingForShortcut = false;

            // Désactiver les gestionnaires d'événements
            this.PreviewKeyUp -= MainWindow_PreviewKeyUp;
            this.PreviewMouseUp -= MainWindow_PreviewMouseUp;

            // Mettre à jour le script si actif
            if (_isScriptActive)
            {
                UpdateScript();
            }

            // Empêcher la saisie manuelle
            e.Handled = true;
        }

        private string TranslateMouseButton(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return "Bouton gauche";
                case MouseButton.Right:
                    return "Bouton droit";
                case MouseButton.Middle:
                    return "Bouton du milieu";
                case MouseButton.XButton1:
                    return "Bouton latéral 1";
                case MouseButton.XButton2:
                    return "Bouton latéral 2";
                default:
                    return "Bouton inconnu";
            }
        }

        private void UpdateScript()
        {
            // Arrêter le script actuel
            StopScript();

            // Redémarrer le script avec les nouvelles configurations
            string shortcut = ShortcutButton.Content.ToString().Trim();
            if (string.IsNullOrEmpty(shortcut) || shortcut == "Entrer un raccourci")
            {
                MessageBox.Show("Veuillez entrer un raccourci clavier.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            StartScript(shortcut);
        }
    }
}