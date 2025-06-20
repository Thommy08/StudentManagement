using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using labmockups.MODELS;
using System.Windows.Media.Animation;

namespace trabalhoLAB
{
    /// <summary>
    /// Interaction logic for Profile.xaml
    /// </summary>
    public partial class Perfil : UserControl
    {
        private string? fotoPath;
        private bool _hasUnsavedChanges = false;
        private bool _modoEdicao = false;

        public Perfil()
        {
            InitializeComponent();

            NameTextBox.IsReadOnly = true;
            EmailTextBox.IsReadOnly = true;
            BtnSalvarAlteracoes.IsEnabled = false;

            NameTextBox.TextChanged += InputField_TextChanged;
            EmailTextBox.TextChanged += InputField_TextChanged;
            NameTextBox.TextChanged += TrackChanges;
            EmailTextBox.TextChanged += TrackChanges;

            AtualizarInterfaceComDadosProfessor();
            UpdateProfileInitials();
        }

        // Obtém a MainWindow para navegar entre views
        private MainWindow? GetMainWindow()
        {
            return Window.GetWindow(this) as MainWindow;
        }

        private void TrackChanges(object sender, TextChangedEventArgs e)
        {
            if (!_hasUnsavedChanges)
            {
                _hasUnsavedChanges = true;
            }
        }

        private bool ConfirmNavigateAway()
        {
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "Existem alterações não salvas. Deseja sair mesmo assim?",
                    "Alterações Pendentes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                return result == MessageBoxResult.Yes;
            }
            return true;
        }

        // Cleanup method to be called before navigation
        private void CleanupResources()
        {
            try
            {
                // Stop any running animations
                if (StatusText != null)
                {
                    StatusText.BeginAnimation(OpacityProperty, null);
                }

                if (StatusBorder != null && StatusBorder.Background != null)
                {
                    var storyboard = (Storyboard)FindResource("StatusColorAnimation");
                    storyboard.Stop();
                }

                // Remove event handlers
                if (NameTextBox != null)
                {
                    NameTextBox.TextChanged -= InputField_TextChanged;
                    NameTextBox.TextChanged -= TrackChanges;
                }
                if (EmailTextBox != null)
                {
                    EmailTextBox.TextChanged -= InputField_TextChanged;
                    EmailTextBox.TextChanged -= TrackChanges;
                }
            }
            catch (Exception)
            {
                // Ignore cleanup errors on resource cleanup
            }
        }

        private void InputField_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Check if both fields have content and update status accordingly
            // Only update status, not timestamp, when typing
            bool hasValidData = !string.IsNullOrWhiteSpace(NameTextBox.Text) &&
                               !string.IsNullOrWhiteSpace(EmailTextBox.Text);
            UpdateAccountStatus(hasValidData, false);
        }

        private void UpdateAccountStatus(bool isActive, bool updateTimestamp = false)
        {
            if (StatusBorder != null && StatusText != null)
            {
                // Find and prepare animation resources
                var storyboard = (Storyboard)FindResource("StatusColorAnimation");
                var animation = (ColorAnimation)storyboard.Children[0];

                // Get target color from resources
                Color targetColor = isActive ?
                    ((SolidColorBrush)FindResource("ActiveStatusColor")).Color :
                    ((SolidColorBrush)FindResource("InactiveStatusColor")).Color;

                // Initialize or animate the background
                if (StatusBorder.Background == null)
                {
                    StatusBorder.Background = new SolidColorBrush(targetColor);
                }
                else
                {
                    // Configure and start color animation
                    animation.To = targetColor;
                    Storyboard.SetTarget(animation, StatusBorder);
                    storyboard.Begin();
                }

                // Configure text animation
                StatusText.Opacity = 0;
                StatusText.Text = isActive ? "Ativo" : "Inativo";

                // Create and start fade-in animation
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                StatusText.BeginAnimation(OpacityProperty, fadeIn);

                // Handle timestamp and change tracking
                if (updateTimestamp && LastUpdateText != null)
                {
                    LastUpdateText.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    _hasUnsavedChanges = false; // Reset changes flag on save
                }
                else if (!updateTimestamp && !_hasUnsavedChanges)
                {
                    _hasUnsavedChanges = true; // Mark as having changes on real-time updates
                }
            }
        }

        private void AtualizarInterfaceComDadosProfessor()
        {
            // Preenche os campos do formulário com os dados do professor global
            Professor professorAtual = App.ProfessorAtual;

            NameTextBox.Text = professorAtual.Nome;
            EmailTextBox.Text = professorAtual.Email;

            // Initialize timestamp if it's still showing the default value
            if (LastUpdateText != null && LastUpdateText.Text == "14/05/2025")
            {
                LastUpdateText.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            }

            // Atualiza o status baseado nos dados do professor
            bool isActive = !string.IsNullOrWhiteSpace(professorAtual.Nome) &&
                           !string.IsNullOrWhiteSpace(professorAtual.Email);
            // Update both status and timestamp when loading profile
            UpdateAccountStatus(isActive, true);

            // Se houver um caminho de foto, tenta carregar a imagem
            if (!string.IsNullOrEmpty(professorAtual.Foto))
            {
                try
                {
                    BitmapImage image = new BitmapImage(new Uri(professorAtual.Foto));
                    ProfileImage.Source = image;
                    ProfileImage.Visibility = Visibility.Visible;
                    ProfileInitials.Visibility = Visibility.Collapsed;
                    fotoPath = professorAtual.Foto;
                }
                catch (Exception)
                {
                    // Se não conseguir carregar a imagem, mostra as iniciais
                    ProfileImage.Visibility = Visibility.Collapsed;
                    ProfileInitials.Visibility = Visibility.Visible;
                }
            }
        }

        private void UpdateProfileInitials()
        {
            string name = NameTextBox.Text;
            if (!string.IsNullOrWhiteSpace(name))
            {
                // Pegar as iniciais do nome
                string[] nameParts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string initials = "";

                if (nameParts.Length > 0)
                {
                    initials += nameParts[0][0];

                    if (nameParts.Length > 1)
                    {
                        initials += nameParts[nameParts.Length - 1][0];
                    }
                }

                ProfileInitials.Text = initials.ToUpper();
            }
        }

        private void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Selecionar Foto de Perfil",
                Filter = "Arquivos de Imagem|*.jpg;*.jpeg;*.png;*.gif|Todos os Arquivos|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage image = new BitmapImage(new Uri(openFileDialog.FileName));
                    ProfileImage.Source = image;
                    ProfileImage.Visibility = Visibility.Visible;
                    ProfileInitials.Visibility = Visibility.Collapsed;

                    // Salva o caminho da imagem para uso posterior
                    fotoPath = openFileDialog.FileName;

                    // Após o usuário carregar uma nova foto, chame o método da MainWindow
                    ((MainWindow)Application.Current.MainWindow).AtualizarMiniaturaPerfil(image, "QH");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar a imagem: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Deseja realmente fazer logout?", "Confirmação",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                // Limpar dados da sessão
                App.ProfessorAtual = new Professor();
                if (App.ListaAlunos != null) App.ListaAlunos.Clear();
                if (App.ListaGrupos != null) App.ListaGrupos.Clear();
                App.LimparDadosSessao();
                App.SalvarDadosProfessor();

                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    CleanupResources();
                    mainWindow.Dashboard_Click(sender, e);

                    // FORÇA O LOGIN NOVAMENTE APÓS LOGOUT
                    mainWindow.CheckAuthentication();
                }

                MessageBox.Show("Logout realizado com sucesso!", "Logout",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao realizar logout: {ex.Message}",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            // Disable the button during save
            var saveButton = sender as Button;
            if (saveButton != null)
            {
                saveButton.IsEnabled = false;
                saveButton.Content = "Salvando...";
            }

            try
            {
                // Verifica se os campos obrigatórios estão preenchidos
                bool hasValidData = !string.IsNullOrWhiteSpace(NameTextBox.Text) &&
                                   !string.IsNullOrWhiteSpace(EmailTextBox.Text);

                if (!hasValidData)
                {
                    MessageBox.Show("Por favor, preencha todos os campos obrigatórios.",
                        "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Atualiza os dados do professor no objeto global
                App.ProfessorAtual.Nome = NameTextBox.Text;
                App.ProfessorAtual.Email = EmailTextBox.Text;

                // Atualiza o caminho da foto se foi alterado
                if (!string.IsNullOrEmpty(fotoPath))
                {
                    App.ProfessorAtual.Foto = fotoPath;
                }

                // Simula um breve delay para mostrar o feedback visual
                await Task.Delay(500);

                // Salva os dados do professor (em um cenário real, salvaria no banco de dados)
                App.SalvarDadosProfessor();

                // Update status and timestamp after successful save
                UpdateAccountStatus(true, true);

                // Atualiza as iniciais do perfil com base no novo nome
                UpdateProfileInitials();

                // Reset change tracking after successful save
                _hasUnsavedChanges = false;

                MessageBox.Show("Alterações salvas com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar alterações: {ex.Message}",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable the button and restore original text
                if (saveButton != null)
                {
                    saveButton.IsEnabled = true;
                    saveButton.Content = "Salvar Alterações";
                }
                NameTextBox.IsReadOnly = true;
                EmailTextBox.IsReadOnly = true;
                BtnSalvarAlteracoes.IsEnabled = false;
                _modoEdicao = false;
            }
        }

        private void ResetForm_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Deseja resetar o formulário para os valores originais?",
                "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Restore original values
                if (App.ProfessorAtual != null)
                {
                    NameTextBox.Text = App.ProfessorAtual.Nome;
                    EmailTextBox.Text = App.ProfessorAtual.Email;

                    // Reset photo if changed
                    if (!string.IsNullOrEmpty(App.ProfessorAtual.Foto))
                    {
                        try
                        {
                            BitmapImage image = new BitmapImage(new Uri(App.ProfessorAtual.Foto));
                            ProfileImage.Source = image;
                            ProfileImage.Visibility = Visibility.Visible;
                            ProfileInitials.Visibility = Visibility.Collapsed;
                            fotoPath = App.ProfessorAtual.Foto;
                        }
                        catch
                        {
                            ProfileImage.Source = null;
                            ProfileImage.Visibility = Visibility.Collapsed;
                            ProfileInitials.Visibility = Visibility.Visible;
                            fotoPath = null;
                        }
                    }

                    // Update status based on original data
                    bool isActive = !string.IsNullOrWhiteSpace(App.ProfessorAtual.Nome) &&
                                  !string.IsNullOrWhiteSpace(App.ProfessorAtual.Email);
                    UpdateAccountStatus(isActive, false);

                    // Reset unsaved changes flag
                    _hasUnsavedChanges = false;

                    MessageBox.Show("Formulário resetado com sucesso!", "Sucesso",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Deseja descartar as alterações?", "Confirmação",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Revert to original data
                if (App.ProfessorAtual != null)
                {
                    // Restore the original values
                    NameTextBox.Text = App.ProfessorAtual.Nome;
                    EmailTextBox.Text = App.ProfessorAtual.Email;

                    // Update status based on original data (don't update timestamp)
                    bool isActive = !string.IsNullOrWhiteSpace(App.ProfessorAtual.Nome) &&
                                  !string.IsNullOrWhiteSpace(App.ProfessorAtual.Email);
                    UpdateAccountStatus(isActive, false);
                }

                // Reset unsaved changes flag since we're discarding changes
                _hasUnsavedChanges = false;

                // Navigate back to dashboard
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    // Clean up resources before navigation
                    CleanupResources();

                    // Navigate to Dashboard
                    mainWindow.LoadDashboardView();
                }
            }
        }

        // Helper method for navigation with confirmation
        private bool NavigateWithConfirmation(Action navigationAction)
        {
            if (ConfirmNavigateAway())
            {
                // Clean up resources
                CleanupResources();

                // Execute the navigation action
                navigationAction?.Invoke();
                return true;
            }
            return false;
        }

        private void Ajuda_Click(object sender, RoutedEventArgs e)
        {
            string helpMessage =
                "Ajuda - Perfil do Usuário\n\n" +
                "• Nome e Email: Preencha ambos os campos para ativar sua conta\n" +
                "• Status da Conta:\n" +
                "  - Verde (Ativo): Todos os campos obrigatórios estão preenchidos\n" +
                "  - Vermelho (Inativo): Um ou mais campos obrigatórios estão vazios\n" +
                "• Foto de Perfil: Clique em 'Alterar Foto' para adicionar ou alterar sua foto\n" +
                "• Última Atualização: Mostra quando suas informações foram alteradas pela última vez\n\n" +
                "Suas alterações só serão salvas após clicar em 'Salvar Alterações'.";

            MessageBox.Show(helpMessage, "Ajuda do Perfil",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Sair_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Deseja realmente sair?", "Confirmação",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                CleanupResources();
                mainWindow.LoadDashboardView();
            }
        }

        private void Alunos_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                CleanupResources();
                mainWindow.LoadView(new Alunos(), "Gestão de Alunos", "Gerencie os alunos do sistema");
            }
        }

        private void Tarefas_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                CleanupResources();
                mainWindow.LoadView(new Tarefas(), "Gestão de Tarefas", "Gerencie as tarefas de avaliação");
            }
        }

        private void Grupos_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                CleanupResources();
                mainWindow.LoadView(new Grupos(), "Gestão de Grupos", "Gerencie os grupos de trabalho");
            }
        }

        private void Pauta_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                CleanupResources();
                mainWindow.LoadView(new Pauta(), "Pauta de Avaliação", "Visualize e gerencie as notas dos alunos");
            }
        }

        private void Histograma_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                CleanupResources();
                mainWindow.LoadView(new Histograma(), "Histograma");
            }
        }

        private void BtnEditarPerfil_Click(object sender, RoutedEventArgs e)
        {
            _modoEdicao = true;
            NameTextBox.IsReadOnly = false;
            EmailTextBox.IsReadOnly = false;
            BtnSalvarAlteracoes.IsEnabled = true;
            NameTextBox.Focus();
        }
    }
}
