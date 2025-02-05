using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace WpfAppWithAutoHotkey
{
    public partial class MainWindow : Window
    {
        private Process _ahkProcess; // Pour garder une référence au processus AHK
        private bool _isScriptActive = false; // Pour suivre l'état du script
        private bool _isWaitingForMouseCloneShortcut = false; // Pour savoir si on attend un raccourci pour Mouse Clone
        private bool _isWaitingForHotkeyCloneShortcut = false; // Pour savoir si on attend un raccourci pour Hotkey Clone
        private bool _isHotkeyCloneActive = false; // Pour suivre l'état de Hotkey Clone
        private NamedPipeServerStream _pipeServer;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;

            // Démarrer le serveur de pipe
            StartPipeServer();

            // Ajouter des gestionnaires d'événements pour les checkboxes
            Window1MouseCloneCheckBox.Checked += CheckBox_CheckedChanged;
            Window1MouseCloneCheckBox.Unchecked += CheckBox_CheckedChanged;
            Window1HotkeyCloneCheckBox.Checked += CheckBox_CheckedChanged;
            Window1HotkeyCloneCheckBox.Unchecked += CheckBox_CheckedChanged;

            Window2MouseCloneCheckBox.Checked += CheckBox_CheckedChanged;
            Window2MouseCloneCheckBox.Unchecked += CheckBox_CheckedChanged;
            Window2HotkeyCloneCheckBox.Checked += CheckBox_CheckedChanged;
            Window2HotkeyCloneCheckBox.Unchecked += CheckBox_CheckedChanged;

            Window3MouseCloneCheckBox.Checked += CheckBox_CheckedChanged;
            Window3MouseCloneCheckBox.Unchecked += CheckBox_CheckedChanged;
            Window3HotkeyCloneCheckBox.Checked += CheckBox_CheckedChanged;
            Window3HotkeyCloneCheckBox.Unchecked += CheckBox_CheckedChanged;

            Window4MouseCloneCheckBox.Checked += CheckBox_CheckedChanged;
            Window4MouseCloneCheckBox.Unchecked += CheckBox_CheckedChanged;
            Window4HotkeyCloneCheckBox.Checked += CheckBox_CheckedChanged;
            Window4HotkeyCloneCheckBox.Unchecked += CheckBox_CheckedChanged;

            // Ajouter des gestionnaires d'événements pour les boutons de raccourci
            MouseCloneShortcutButton.LostFocus += ShortcutButton_LostFocus;
            HotkeyCloneShortcutButton.LostFocus += ShortcutButton_LostFocus;
        }

        private void StartPipeServer()
        {
            _pipeServer = new NamedPipeServerStream("OptimizerPipe", PipeDirection.In);
            Task.Run(() =>
            {
                while (true)
                {
                    _pipeServer.WaitForConnection();
                    using (StreamReader reader = new StreamReader(_pipeServer))
                    {
                        string message = reader.ReadLine();
                        Dispatcher.Invoke(() =>
                        {
                            if (message == "HotkeyCloneEnabled")
                            {
                                HotkeyCloneStatusIndicator.Fill = Brushes.Green;
                                _isHotkeyCloneActive = true;
                            }
                            else if (message == "HotkeyCloneDisabled")
                            {
                                HotkeyCloneStatusIndicator.Fill = Brushes.Gray;
                                _isHotkeyCloneActive = false;
                            }
                        });
                    }
                    _pipeServer.Disconnect();
                }
            });
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

        private void MouseCloneShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            // Afficher "Entrer un raccourci" et attendre la saisie
            MouseCloneShortcutButton.Content = "Entrer un raccourci";
            _isWaitingForMouseCloneShortcut = true;

            // Capturer les événements clavier et souris
            this.PreviewKeyUp += MainWindow_PreviewKeyUp;
            this.PreviewMouseUp += MainWindow_PreviewMouseUp;
        }

        private void HotkeyCloneShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            // Afficher "Entrer un raccourci" et attendre la saisie
            HotkeyCloneShortcutButton.Content = "Entrer un raccourci";
            _isWaitingForHotkeyCloneShortcut = true;

            // Capturer les événements clavier et souris
            this.PreviewKeyUp += MainWindow_PreviewKeyUp;
            this.PreviewMouseUp += MainWindow_PreviewMouseUp;
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!_isWaitingForMouseCloneShortcut && !_isWaitingForHotkeyCloneShortcut)
            {
                return; // Ignorer si on n'attend pas de raccourci
            }

            // Ignorer les touches modificatrices seules (Ctrl, Shift, Alt)
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
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

            string newShortcut = shortcutBuilder.ToString();

            // Vérifier si le raccourci est déjà utilisé
            if (IsShortcutAlreadyUsed(newShortcut))
            {
                // Changer la couleur du contour du bouton en rouge
                if (_isWaitingForMouseCloneShortcut)
                {
                    MouseCloneShortcutButton.BorderBrush = Brushes.Red;
                }
                else if (_isWaitingForHotkeyCloneShortcut)
                {
                    HotkeyCloneShortcutButton.BorderBrush = Brushes.Red;
                }

                // Ne pas mettre à jour le texte du bouton
                return;
            }

            // Mettre à jour le texte du bouton
            if (_isWaitingForMouseCloneShortcut)
            {
                MouseCloneShortcutButton.Content = newShortcut;
                MouseCloneShortcutButton.BorderBrush = SystemColors.ControlDarkBrush; // Rétablir la couleur par défaut
                _isWaitingForMouseCloneShortcut = false;
            }
            else if (_isWaitingForHotkeyCloneShortcut)
            {
                HotkeyCloneShortcutButton.Content = newShortcut;
                HotkeyCloneShortcutButton.BorderBrush = SystemColors.ControlDarkBrush; // Rétablir la couleur par défaut
                _isWaitingForHotkeyCloneShortcut = false;
            }

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
            if (!_isWaitingForMouseCloneShortcut && !_isWaitingForHotkeyCloneShortcut)
            {
                return; // Ignorer si on n'attend pas de raccourci
            }

            // Capturer les clics de souris
            string mouseButton = TranslateMouseButton(e.ChangedButton);

            if (_isWaitingForMouseCloneShortcut)
            {
                MouseCloneShortcutButton.Content = mouseButton;
                _isWaitingForMouseCloneShortcut = false;
            }
            else if (_isWaitingForHotkeyCloneShortcut)
            {
                HotkeyCloneShortcutButton.Content = mouseButton;
                _isWaitingForHotkeyCloneShortcut = false;
            }

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
                string mouseCloneShortcut = MouseCloneShortcutButton.Content.ToString().Trim();
                string hotkeyCloneShortcut = HotkeyCloneShortcutButton.Content.ToString().Trim();

                if (string.IsNullOrEmpty(mouseCloneShortcut) || mouseCloneShortcut == "Entrer un raccourci" ||
                    string.IsNullOrEmpty(hotkeyCloneShortcut) || hotkeyCloneShortcut == "Entrer un raccourci")
                {
                    MessageBox.Show("Veuillez entrer un raccourci pour Mouse Clone et Hotkey Clone.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                StartScript(mouseCloneShortcut, hotkeyCloneShortcut);
                ToggleScriptButton.Content = "Désactiver le script";
                _isScriptActive = true;
            }

            // Mettre à jour les voyants
            UpdateStatusIndicators();
        }

        private void StartScript(string mouseCloneShortcut, string hotkeyCloneShortcut)
        {
            // Arrêter le script actuel s'il est déjà en cours d'exécution
            StopScript();

            // Générer le script AHK dynamiquement
            string ahkScriptContent = GenerateAhkScript(mouseCloneShortcut, hotkeyCloneShortcut);
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

        private string GenerateAhkScript(string mouseCloneShortcut, string hotkeyCloneShortcut)
        {
            // Récupérer les noms des fenêtres et l'état des checkboxes
            string window1 = Window1TextBox.Text.Trim();
            string window2 = Window2TextBox.Text.Trim();
            string window3 = Window3TextBox.Text.Trim();
            string window4 = Window4TextBox.Text.Trim();

            bool isWindow1MouseCloneActive = Window1MouseCloneCheckBox.IsChecked == true;
            bool isWindow1HotkeyCloneActive = Window1HotkeyCloneCheckBox.IsChecked == true;

            bool isWindow2MouseCloneActive = Window2MouseCloneCheckBox.IsChecked == true;
            bool isWindow2HotkeyCloneActive = Window2HotkeyCloneCheckBox.IsChecked == true;

            bool isWindow3MouseCloneActive = Window3MouseCloneCheckBox.IsChecked == true;
            bool isWindow3HotkeyCloneActive = Window3HotkeyCloneCheckBox.IsChecked == true;

            bool isWindow4MouseCloneActive = Window4MouseCloneCheckBox.IsChecked == true;
            bool isWindow4HotkeyCloneActive = Window4HotkeyCloneCheckBox.IsChecked == true;

            // Convertir les raccourcis en noms AHK
            string ahkMouseCloneShortcut = TranslateShortcutToAhk(mouseCloneShortcut);
            string ahkHotkeyCloneShortcut = TranslateShortcutToAhk(hotkeyCloneShortcut);

            // Générer le script AHK dynamiquement
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine("; Masquer l'icône AutoHotkey dans la barre des tâches");
            scriptBuilder.AppendLine("#NoTrayIcon");
            scriptBuilder.AppendLine();

            // Raccourci pour Mouse Clone
            scriptBuilder.AppendLine($"; Raccourci pour Mouse Clone");
            scriptBuilder.AppendLine($"{ahkMouseCloneShortcut}::");
            scriptBuilder.AppendLine("{");

            if (isWindow1MouseCloneActive)
            {
                scriptBuilder.AppendLine($"    if WinExist(\"{window1}\")");
                scriptBuilder.AppendLine("    {");
                scriptBuilder.AppendLine($"        ControlClick(, \"{window1}\",, \"left\")");
                scriptBuilder.AppendLine("    }");
            }

            if (isWindow2MouseCloneActive)
            {
                scriptBuilder.AppendLine($"    if WinExist(\"{window2}\")");
                scriptBuilder.AppendLine("    {");
                scriptBuilder.AppendLine($"        ControlClick(, \"{window2}\",, \"left\")");
                scriptBuilder.AppendLine("    }");
            }

            if (isWindow3MouseCloneActive)
            {
                scriptBuilder.AppendLine($"    if WinExist(\"{window3}\")");
                scriptBuilder.AppendLine("    {");
                scriptBuilder.AppendLine($"        ControlClick(, \"{window3}\",, \"left\")");
                scriptBuilder.AppendLine("    }");
            }

            if (isWindow4MouseCloneActive)
            {
                scriptBuilder.AppendLine($"    if WinExist(\"{window4}\")");
                scriptBuilder.AppendLine("    {");
                scriptBuilder.AppendLine($"        ControlClick(, \"{window4}\",, \"left\")");
                scriptBuilder.AppendLine("    }");
            }

            scriptBuilder.AppendLine("}");
            scriptBuilder.AppendLine();

            // Raccourci pour Hotkey Clone
            scriptBuilder.AppendLine($"; Raccourci pour Hotkey Clone");
            scriptBuilder.AppendLine($"{ahkHotkeyCloneShortcut}::");
            scriptBuilder.AppendLine("{");
            scriptBuilder.AppendLine("    ; Activer/désactiver Hotkey Clone");
            scriptBuilder.AppendLine("    HotkeyCloneEnabled := !HotkeyCloneEnabled");
            scriptBuilder.AppendLine("    if (HotkeyCloneEnabled)");
            scriptBuilder.AppendLine("    {");
            scriptBuilder.AppendLine("        ; Envoyer un message à l'application pour allumer le voyant");
            scriptBuilder.AppendLine("        Run, \"\"\"C:\\Windows\\System32\\cmd.exe\" /c echo HotkeyCloneEnabled > \\\\.\\pipe\\OptimizerPipe\",, Hide");
            scriptBuilder.AppendLine("    }");
            scriptBuilder.AppendLine("    else");
            scriptBuilder.AppendLine("    {");
            scriptBuilder.AppendLine("        ; Envoyer un message à l'application pour éteindre le voyant");
            scriptBuilder.AppendLine("        Run, \"\"\"C:\\Windows\\System32\\cmd.exe\" /c echo HotkeyCloneDisabled > \\\\.\\pipe\\OptimizerPipe\",, Hide");
            scriptBuilder.AppendLine("    }");
            scriptBuilder.AppendLine("}");

            // Fonctionnalité Hotkey Clone
            scriptBuilder.AppendLine($"; Fonctionnalité Hotkey Clone");
            scriptBuilder.AppendLine("HotkeyCloneEnabled := false");
            scriptBuilder.AppendLine("Loop");
            scriptBuilder.AppendLine("{");
            scriptBuilder.AppendLine("    if (HotkeyCloneEnabled)");
            scriptBuilder.AppendLine("    {");

            if (isWindow1HotkeyCloneActive)
            {
                scriptBuilder.AppendLine($"        if WinExist(\"{window1}\")");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine($"            ControlSend(\"\", \"\", \"{window1}\", \"{hotkeyCloneShortcut}\")");
                scriptBuilder.AppendLine("        }");
            }

            if (isWindow2HotkeyCloneActive)
            {
                scriptBuilder.AppendLine($"        if WinExist(\"{window2}\")");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine($"            ControlSend(\"\", \"\", \"{window2}\", \"{hotkeyCloneShortcut}\")");
                scriptBuilder.AppendLine("        }");
            }

            if (isWindow3HotkeyCloneActive)
            {
                scriptBuilder.AppendLine($"        if WinExist(\"{window3}\")");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine($"            ControlSend(\"\", \"\", \"{window3}\", \"{hotkeyCloneShortcut}\")");
                scriptBuilder.AppendLine("        }");
            }

            if (isWindow4HotkeyCloneActive)
            {
                scriptBuilder.AppendLine($"        if WinExist(\"{window4}\")");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine($"            ControlSend(\"\", \"\", \"{window4}\", \"{hotkeyCloneShortcut}\")");
                scriptBuilder.AppendLine("        }");
            }

            scriptBuilder.AppendLine("    }");
            scriptBuilder.AppendLine("    Sleep, 100");
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
                    return "!";
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

        private bool IsShortcutAlreadyUsed(string shortcut)
        {
            string mouseCloneShortcut = MouseCloneShortcutButton.Content.ToString().Trim();
            string hotkeyCloneShortcut = HotkeyCloneShortcutButton.Content.ToString().Trim();

            return shortcut == mouseCloneShortcut || shortcut == hotkeyCloneShortcut;
        }

        private void ShortcutButton_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.BorderBrush = SystemColors.ControlDarkBrush; // Rétablir la couleur par défaut
            }
        }

        private void UpdateStatusIndicators()
        {
            // Mettre à jour le voyant de Mouse Clone
            if (_isScriptActive && MouseCloneShortcutButton.Content.ToString() != "Entrer un raccourci")
            {
                MouseCloneStatusIndicator.Fill = Brushes.Green;
            }
            else
            {
                MouseCloneStatusIndicator.Fill = Brushes.Gray;
            }

            // Mettre à jour le voyant de Hotkey Clone
            if (_isScriptActive && HotkeyCloneShortcutButton.Content.ToString() != "Entrer un raccourci")
            {
                HotkeyCloneStatusIndicator.Fill = Brushes.Green;
            }
            else
            {
                HotkeyCloneStatusIndicator.Fill = Brushes.Gray;
            }
        }



        private void UpdateScript()
        {
            // Arrêter le script actuel
            StopScript();

            // Redémarrer le script avec les nouvelles configurations
            string mouseCloneShortcut = MouseCloneShortcutButton.Content.ToString().Trim();
            string hotkeyCloneShortcut = HotkeyCloneShortcutButton.Content.ToString().Trim();

            if (string.IsNullOrEmpty(mouseCloneShortcut) || mouseCloneShortcut == "Entrer un raccourci" ||
                string.IsNullOrEmpty(hotkeyCloneShortcut) || hotkeyCloneShortcut == "Entrer un raccourci")
            {
                MessageBox.Show("Veuillez entrer un raccourci pour Mouse Clone et Hotkey Clone.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            StartScript(mouseCloneShortcut, hotkeyCloneShortcut);

            // Mettre à jour les voyants
            UpdateStatusIndicators();
        }
    }
}