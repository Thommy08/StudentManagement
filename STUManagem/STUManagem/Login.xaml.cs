using labmockups.MODELS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace trabalhoLAB
{
    public partial class Login : Window
    {
        public bool IsAuthenticated { get; private set; }
        private List<SavedProfile> savedProfiles;
        private bool savedProfilesVisible = true;

        public Login()
        {
            InitializeComponent();
            IsAuthenticated = false;

            // Garantir que começamos com dados limpos
            App.LimparDadosSessao();

            // Carregar perfis guardados
            LoadSavedProfiles();
            UpdateUI();
        }

        private void LoadSavedProfiles()
        {
            try
            {
                string profilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrabalhosLAB", "saved_profiles.json");

                if (File.Exists(profilesPath))
                {
                    string json = File.ReadAllText(profilesPath);
                    savedProfiles = JsonConvert.DeserializeObject<List<SavedProfile>>(json) ?? new List<SavedProfile>();
                }
                else
                {
                    savedProfiles = new List<SavedProfile>();
                }
            }
            catch (Exception)
            {
                savedProfiles = new List<SavedProfile>();
            }
        }

        private void SaveProfiles()
        {
            try
            {
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrabalhosLAB");
                Directory.CreateDirectory(appDataPath);

                string profilesPath = Path.Combine(appDataPath, "saved_profiles.json");
                string json = JsonConvert.SerializeObject(savedProfiles, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(profilesPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar perfis: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateUI()
        {
            // Atualizar lista de perfis guardados
            SavedProfilesList.ItemsSource = null;
            SavedProfilesList.ItemsSource = savedProfiles;

            // Mostrar/ocultar seções baseado na existência de perfis salvos
            bool hasProfiles = savedProfiles.Count > 0;

            SavedProfilesTitle.Visibility = hasProfiles ? Visibility.Visible : Visibility.Collapsed;
            SavedProfilesPanel.Visibility = hasProfiles ? Visibility.Visible : Visibility.Collapsed;
            ToggleSavedProfilesBtn.Visibility = hasProfiles ? Visibility.Visible : Visibility.Collapsed;
            ClearAllProfilesBtn.Visibility = hasProfiles ? Visibility.Visible : Visibility.Collapsed;

            // Atualizar texto do subtítulo
            if (hasProfiles)
            {
                SubtitleText.Text = "Escolha uma conta ou faça login";
                NewLoginTitle.Text = "Ou faça login com nova conta";
            }
            else
            {
                SubtitleText.Text = "Faça login para continuar";
                NewLoginTitle.Text = "Dados de login";
            }

            // Atualizar botão de toggle
            if (hasProfiles)
            {
                ToggleSavedProfilesBtn.Content = savedProfilesVisible ? "▲ Ocultar contas guardadas" : "▼ Mostrar contas guardadas";
                SavedProfilesList.Visibility = savedProfilesVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SavedProfile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SavedProfile profile)
            {
                // Confirmar login com perfil selecionado
                var result = MessageBox.Show($"Entrar como {profile.Nome}?", "Confirmar Login",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    LoginWithProfile(profile);
                }
            }
        }

        private void LoginWithProfile(SavedProfile profile)
        {
            try
            {
                IsAuthenticated = true;

                // Atualizar dados do professor atual
                App.ProfessorAtual = new Professor
                {
                    Nome = profile.Nome,
                    Email = profile.Email,
                    Foto = profile.FotoPath
                };

                // Atualizar última utilização do perfil
                profile.LastUsed = DateTime.Now;

                // Reorganizar lista (perfil mais recente primeiro)
                savedProfiles.Remove(profile);
                savedProfiles.Insert(0, profile);
                SaveProfiles();

                App.SalvarDadosProfessor();

                MessageBox.Show($"Bem-vindo de volta, {profile.Nome}!", "Login Successful",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao fazer login: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // Validar campos
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Por favor, preencha todos os campos.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsAuthenticated = true;

                // Criar novo professor
                App.ProfessorAtual = new Professor
                {
                    Nome = NameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim()
                };

                // Salvar perfil se solicitado
                if (SaveProfileCheckBox.IsChecked == true)
                {
                    SaveCurrentProfile();
                }

                App.SalvarDadosProfessor();

                MessageBox.Show("Log-in efetuado com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao fazer login: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCurrentProfile()
        {
            try
            {
                string nome = NameTextBox.Text.Trim();
                string email = EmailTextBox.Text.Trim();

                // Verificar se já existe um perfil com o mesmo email
                var existingProfile = savedProfiles.FirstOrDefault(p =>
                    string.Equals(p.Email, email, StringComparison.OrdinalIgnoreCase));

                if (existingProfile != null)
                {
                    // Atualizar perfil existente
                    existingProfile.Nome = nome;
                    existingProfile.LastUsed = DateTime.Now;

                    // Mover para o topo da lista
                    savedProfiles.Remove(existingProfile);
                    savedProfiles.Insert(0, existingProfile);
                }
                else
                {
                    // Criar novo perfil
                    var newProfile = new SavedProfile
                    {
                        Nome = nome,
                        Email = email,
                        Initials = GetInitials(nome),
                        LastUsed = DateTime.Now,
                        FotoPath = null
                    };

                    savedProfiles.Insert(0, newProfile);
                }

                // Limitar número de perfis salvos (máximo 5)
                if (savedProfiles.Count > 5)
                {
                    savedProfiles = savedProfiles.Take(5).ToList();
                }

                SaveProfiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar perfil: {ex.Message}", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "?";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpper();

            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private void RemoveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SavedProfile profile)
            {
                var result = MessageBox.Show($"Remover a conta de {profile.Nome}?", "Confirmar Remoção",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    savedProfiles.Remove(profile);
                    SaveProfiles();
                    UpdateUI();
                }
            }
        }

        private void ToggleSavedProfiles_Click(object sender, RoutedEventArgs e)
        {
            savedProfilesVisible = !savedProfilesVisible;
            UpdateUI();
        }

        private void ClearAllProfiles_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Tem certeza que deseja remover todas as contas guardadas?",
                "Confirmar Limpeza", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                savedProfiles.Clear();
                SaveProfiles();
                UpdateUI();

                MessageBox.Show("Todas as contas foram removidas.", "Contas Removidas",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    // Classe para representar perfis salvos
    public class SavedProfile
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public DateTime LastUsed { get; set; } = DateTime.Now;
        public string? FotoPath { get; set; }
    }
}