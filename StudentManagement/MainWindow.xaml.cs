using labmockups.MODELS;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media; // Adicionado para corrigir o erro relacionado ao SolidColorBrush

namespace trabalhoLAB
{
    public partial class MainWindow : Window
    {
        private readonly string _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "trabalhoLAB");
        private readonly string _tarefasFilePath;

        // Observable collection for tasks that will automatically update the UI when changed
        public ObservableCollection<Tarefa> ListaTarefas { get; private set; }

        // Adicione estas declarações de campo na sua classe MainWindow
        private TextBlock TotalAlunosCounter;
        private TextBlock TotalGruposCounter;
        private TextBlock TotalTarefasCounter;

        public MainWindow()
        {
            InitializeComponent();

            // Associa os controles do XAML aos campos
            TotalAlunosCounter = this.FindName("TotalAlunosCounter") as TextBlock;
            TotalGruposCounter = this.FindName("TotalGruposCounter") as TextBlock;
            TotalTarefasCounter = this.FindName("TotalTarefasCounter") as TextBlock;

            // Initialize the tasks collection
            ListaTarefas = new ObservableCollection<Tarefa>();

            // Set DataContext to this window to enable bindings
            this.DataContext = this;

            // Check authentication before proceeding
            CheckAuthentication();

            // Initialize file paths
            _tarefasFilePath = Path.Combine(_appDataPath, "tarefas.xml");

            // Load tasks into the observable collection
            CarregarTarefasParaExibicao();

            CarregarFotoPerfil();
            AtualizarContadores();
            UpdateThemeToggleButton();

            // Show welcome toast notification
            ShowToast("Bem-vindo ao Sistema de Gestão de Avaliações!", ToastType.Success);

            LoadDashboardView(); // Garante que o Dashboard aparece ao iniciar
        }

        private bool IsUserLoggedIn => App.ProfessorAtual != null &&
                                 !string.IsNullOrWhiteSpace(App.ProfessorAtual.Email);

        public void CheckAuthentication()
        {
            // Check if user data exists but is not marked as logged in
            if (!App.IsLoggedIn && IsUserLoggedIn)
            {
                App.IsLoggedIn = true;
            }

            // Do not force login on application start
            if (!App.IsLoggedIn)
            {
                // Show a welcome message with login option
                var result = MessageBox.Show(
                    "Bem-vindo ao Sistema de Gestão de Avaliações!\n\nDeseja fazer login para acessar todas as funcionalidades?",
                    "Bem-vindo",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    ShowLoginDialog();
                }
            }

            // Update UI regardless of login state
            CarregarFotoPerfil();
            AtualizarContadores();
        }

        private bool ShowLoginDialog()
        {
            // Instancie a janela de login diretamente
            Login loginDialog = new Login
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };

            bool? loginResult = loginDialog.ShowDialog();

            if (loginResult == true)
            {
                // Verify if the data is complete
                bool hasValidData = !string.IsNullOrWhiteSpace(App.ProfessorAtual.Nome) &&
                                  !string.IsNullOrWhiteSpace(App.ProfessorAtual.Email);

                // Update login state only if data is complete
                App.IsLoggedIn = hasValidData;

                // Reload user interface
                CarregarFotoPerfil();
                AtualizarContadores();

                // Redireciona automaticamente para o perfil após login bem-sucedido
                NavigateToProfile();

                return hasValidData;
            }

            return false;
        }

        private void ShowLoginIfNeeded()
        {
            if (!IsUserLoggedIn)
            {
                var result = MessageBox.Show(
                    "Esta funcionalidade requer autenticação. Deseja fazer login?",
                    "Login Necessário",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ShowLoginDialog();
                }
            }
        }

        private void AtualizarContadores()
        {
            if (TotalAlunosCounter != null)
            {
                TotalAlunosCounter.Text = App.ListaAlunos.Count.ToString();
            }

            if (TotalGruposCounter != null)
            {
                int totalGrupos = App.ListaGrupos.Count;
                TotalGruposCounter.Text = totalGrupos.ToString();
            }

            if (TotalTarefasCounter != null)
            {
                int totalTarefas = CarregarTarefas().Count;
                TotalTarefasCounter.Text = totalTarefas.ToString();
            }
        }

        // Corrija o método CarregarTarefas para evitar warnings de nulidade:
        private List<Tarefa> CarregarTarefas()
        {
            var tarefas = new List<Tarefa>();
            try
            {
                if (File.Exists(_tarefasFilePath))
                {
                    var serializer = new XmlSerializer(typeof(List<Tarefa>));
                    using (var reader = new StreamReader(_tarefasFilePath))
                    {
                        var result = serializer.Deserialize(reader);
                        if (result is List<Tarefa> lista)
                            tarefas = lista;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar tarefas: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return tarefas;
        }

        private void CarregarTarefasParaExibicao()
        {
            // Clear existing items
            ListaTarefas.Clear();

            // Load the tasks from storage
            var tarefas = CarregarTarefas();

            // Sort tasks by start date (most recent first)
            tarefas.Sort((a, b) => b.DataInicio.CompareTo(a.DataInicio));

            // Add each task to the observable collection
            foreach (var tarefa in tarefas)
            {
                ListaTarefas.Add(tarefa);
            }
        }

        private void CarregarFotoPerfil()
        {
            Professor professor = App.ProfessorAtual;

            if (!string.IsNullOrEmpty(professor.Foto))
            {
                try
                {
                    BitmapImage image = new BitmapImage(new Uri(professor.Foto));
                    ProfileImage.Source = image;
                    ProfileImage.Visibility = Visibility.Visible;
                    ProfileInitials.Visibility = Visibility.Collapsed;
                    ProfileBorder.Background = System.Windows.Media.Brushes.Transparent;
                }
                catch (Exception)
                {
                    MostrarIniciaisUsuario(professor.Nome);
                }
            }
            else
            {
                MostrarIniciaisUsuario(professor.Nome);
            }
        }

        private void MostrarIniciaisUsuario(string nome)
        {
            ProfileImage.Visibility = Visibility.Collapsed;
            ProfileInitials.Visibility = Visibility.Visible;
            ProfileBorder.Background = System.Windows.Media.Brushes.LightGray;

            if (!string.IsNullOrWhiteSpace(nome))
            {
                string[] nameParts = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
            else
            {
                ProfileInitials.Text = "👤";
            }
        }

        // Adicione este método para atualizar a miniatura do perfil
        public void AtualizarMiniaturaPerfil(BitmapImage imagemPerfil, string iniciais)
        {
            if (imagemPerfil != null)
            {
                ProfileImage.Source = imagemPerfil;
                ProfileImage.Visibility = Visibility.Visible;
                ProfileInitials.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProfileImage.Source = null;
                ProfileImage.Visibility = Visibility.Collapsed;
                ProfileInitials.Text = iniciais;
                ProfileInitials.Visibility = Visibility.Visible;
            }
        }

        // Método para carregar UserControls no ContentControl
        public
            void LoadView(UserControl view, string title = "", string subtitle = "")
        {
            // Define o conteúdo do ContentControl
            MainContent.Content = view;

            // Atualiza o título e subtítulo da página, se fornecidos
            if (!string.IsNullOrEmpty(title))
                PageTitle.Text = title;

            if (!string.IsNullOrEmpty(subtitle))
                PageSubtitle.Text = subtitle;

            // Atualiza contadores e outras informações da UI conforme necessário
            AtualizarContadores();
        }

        public void LoadDashboardView()
        {
            // Carrega o UserControl Dashboard no ContentControl
            MainContent.Content = new Dashboard();

            // Define o título e subtítulo da página
            PageTitle.Text = "Dashboard";
            PageSubtitle.Text = "Bem-vindo ao Sistema de Gestão de Avaliações";

            // Atualiza contadores e outras informações da UI
            CarregarTarefasParaExibicao();
            AtualizarContadores();
        }

        private void NavigateToProfile()
        {
            // Check if login is required for this feature
            if (!IsUserLoggedIn)
            {
                ShowLoginIfNeeded();
                if (!IsUserLoggedIn) return; // Don't proceed if not logged in
            }

            // Carrega o UserControl de perfil
            LoadView(new Perfil(), "Perfil", "Gerencie suas informações pessoais");
        }

        // Event Handlers
        public void Perfil_Click(object sender, RoutedEventArgs e)
        {
            NavigateToProfile();
        }

        public void Profile_Click(object sender, RoutedEventArgs e)
        {
            NavigateToProfile();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToProfile();
        }

        public void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboardView();
        }

        public void Alunos_Click(object sender, RoutedEventArgs e)
        {
            // Check if login is required for this feature
            if (!IsUserLoggedIn)
            {
                ShowLoginIfNeeded();
                if (!IsUserLoggedIn) return; // Don't proceed if not logged in
            }

            // Carrega o UserControl de alunos
            LoadView(new Alunos(), "Gestão de Alunos", "Gerencie os alunos do sistema");
        }

        public void Tarefas_Click(object sender, RoutedEventArgs e)
        {
            // Check if login is required for this feature
            if (!IsUserLoggedIn)
            {
                ShowLoginIfNeeded();
                if (!IsUserLoggedIn) return; // Don't proceed if not logged in
            }

            // Carrega o UserControl de tarefas
            LoadView(new Tarefas(), "Gestão de Tarefas", "Gerencie as tarefas de avaliação");

            // Atualizar os dados ao carregar a visão
            CarregarTarefasParaExibicao();
            AtualizarContadores();
        }

        public void Grupos_Click(object sender, RoutedEventArgs e)
        {
            // Check if login is required for this feature
            if (!IsUserLoggedIn)
            {
                ShowLoginIfNeeded();
                if (!IsUserLoggedIn) return; // Don't proceed if not logged in
            }

            // Carrega o UserControl de grupos
            LoadView(new Grupos(), "Gestão de Grupos", "Gerencie os grupos de trabalho");
        }

        public void Pauta_Click(object sender, RoutedEventArgs e)
        {
            // Check if login is required for this feature
            if (!IsUserLoggedIn)
            {
                ShowLoginIfNeeded();
                if (!IsUserLoggedIn) return; // Don't proceed if not logged in
            }

            // Carrega o UserControl de pauta
            LoadView(new Pauta(), "Pauta de Avaliação", "Visualize e gerencie as notas dos alunos");
        }

        public void Histograma_Click(object sender, RoutedEventArgs e)
        {
            // Check if login is required for this feature
            if (!IsUserLoggedIn)
            {
                ShowLoginIfNeeded();
                if (!IsUserLoggedIn) return; // Don't proceed if not logged in
            }

            // Carrega o UserControl de histograma
            LoadView(new Histograma(), "Histograma de Notas", "Análise estatística das notas");
        }

        public void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between light and dark theme
            App.ToggleTheme();

            // Update toggle button appearance
            UpdateThemeToggleButton();

            // Show toast notification about theme change
            string themeMessage = App.IsDarkTheme ? "Tema escuro ativado" : "Tema claro ativado";
            ShowToast(themeMessage, ToastType.Info);
        }

        private void UpdateThemeToggleButton()
        {
            // Update toggle button appearance based on current theme
            if (App.IsDarkTheme)
            {
                LightModeIcon.Visibility = Visibility.Collapsed;
                DarkModeIcon.Visibility = Visibility.Visible;
                ThemeToggleButton.ToolTip = "Mudar para tema claro";
            }
            else
            {
                LightModeIcon.Visibility = Visibility.Visible;
                DarkModeIcon.Visibility = Visibility.Collapsed;
                ThemeToggleButton.ToolTip = "Mudar para tema escuro";
            }
        }

        // Toast notification system
        public enum ToastType { Success, Warning, Error, Info }

        // Corrija a declaração do _toastTimer para ser anulável:
        private System.Windows.Threading.DispatcherTimer? _toastTimer;

        public void ShowToast(string message, ToastType type = ToastType.Success, int durationMs = 3000)
        {
            // Ensure ToastIndicator and ToastContainer are defined in the XAML file
            if (ToastIndicator == null || ToastContainer == null)
            {
                MessageBox.Show("ToastIndicator ou ToastContainer não estão definidos no XAML.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Set toast message
            ToastMessage.Text = message;

            // Apply appropriate styling based on toast type
            switch (type)
            {
                case ToastType.Success:
                    ToastIndicator.Style = FindResource("SuccessStatusIndicator") as Style;
                    break;
                case ToastType.Warning:
                    ToastIndicator.Style = FindResource("WarningStatusIndicator") as Style;
                    break;
                case ToastType.Error:
                    ToastIndicator.Style = FindResource("ErrorStatusIndicator") as Style;
                    break;
                case ToastType.Info:
                    ToastIndicator.Style = FindResource("SuccessStatusIndicator") as Style;
                    ToastIndicator.Background = (SolidColorBrush)FindResource("AppAccent");
                    break;
            }

            // Show toast container
            ToastContainer.Visibility = Visibility.Visible;

            // Set up timer to hide toast after duration
            if (_toastTimer == null)
            {
                _toastTimer = new System.Windows.Threading.DispatcherTimer();
                _toastTimer.Tick += (s, e) => {
                    ToastContainer.Visibility = Visibility.Collapsed;
                    _toastTimer.Stop();
                };
            }
            else
            {
                _toastTimer.Stop();
            }

            _toastTimer.Interval = TimeSpan.FromMilliseconds(durationMs);
            _toastTimer.Start();
        }

        private void CloseToast_Click(object sender, RoutedEventArgs e)
        {
            ToastContainer.Visibility = Visibility.Collapsed;
            if (_toastTimer != null && _toastTimer.IsEnabled)
            {
                _toastTimer.Stop();
            }
        }

        public void Sair_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Deseja realmente sair?", "Confirmação",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Limpar dados da sessão antes de fechar
                App.LimparDadosSessao();
                Application.Current.Shutdown();
            }
        }

        internal void LoadView(Histograma histograma, string v)
        {
            throw new NotImplementedException();
        }

        private void GetParentWindow()
        {
            Window parentWindow = Window.GetWindow(this);
        }
    }
}



