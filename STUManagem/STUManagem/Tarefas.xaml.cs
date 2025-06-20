using labmockups.MODELS; // Possível erro: verifique se este namespace existe
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace trabalhoLAB
{
    public partial class Tarefas : UserControl
    {
        // Obtém a MainWindow para navegar entre views
        private MainWindow GetMainWindow()
        {
            return Window.GetWindow(this) as MainWindow;
        }
        private ObservableCollection<Tarefa> _listaTarefas;
        private Tarefa _tarefaAtual;
        private int _proximoId = 1;
        private bool _isEditing = false;

        private readonly string _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "trabalhoLAB");
        private readonly string _tarefasFilePath;

        public Tarefas()
        {
            InitializeComponent();

            // Initialize file paths
            _tarefasFilePath = Path.Combine(_appDataPath, "tarefas.xml");

            // Ensure the directory exists
            if (!Directory.Exists(_appDataPath))
            {
                Directory.CreateDirectory(_appDataPath);
            }

            // Initialize collections
            _listaTarefas = new ObservableCollection<Tarefa>();

            // Load saved data
            LoadTarefas();

            // Set DataContext for DataGrid
            DgTarefas.ItemsSource = _listaTarefas;

            // Initialize form for new task
            LimparFormulario();
            AtualizarProximoId();
            LoadProfilePhoto();
        }

        #region File Operations

        private void LoadTarefas()
        {
            try
            {
                if (File.Exists(_tarefasFilePath))
                {
                    var serializer = new XmlSerializer(typeof(List<Tarefa>));
                    using (var reader = new StreamReader(_tarefasFilePath))
                    {
                        var tarefas = (List<Tarefa>)serializer.Deserialize(reader);
                        _listaTarefas.Clear();
                        foreach (var tarefa in tarefas)
                        {
                            _listaTarefas.Add(tarefa);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar tarefas: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveTarefas()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(List<Tarefa>));
                using (var writer = new StreamWriter(_tarefasFilePath))
                {
                    var list = _listaTarefas.ToList();
                    serializer.Serialize(writer, list);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar tarefas: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region UI Operations

        private void LimparFormulario()
        {
            _tarefaAtual = new Tarefa
            {
                Id = _proximoId,
                DataInicio = DateTime.Today,
                DataEntrega = DateTime.Today.AddDays(7)
            };

            TxtId.Text = _tarefaAtual.Id.ToString();
            TxtTitulo.Text = string.Empty;
            TxtDescricao.Text = string.Empty;
            DpDataInicio.SelectedDate = _tarefaAtual.DataInicio;
            TxtHoraInicio.Text = "00";
            TxtMinutoInicio.Text = "00";
            DpDataTermino.SelectedDate = _tarefaAtual.DataEntrega;
            TxtHoraTermino.Text = "23";
            TxtMinutoTermino.Text = "59";
            TxtPeso.Text = "0";

            _isEditing = false;
            BtnExcluir.IsEnabled = false;
        }

        private void PreencherFormulario(Tarefa tarefa)
        {
            if (tarefa == null) return;

            _tarefaAtual = tarefa;

            TxtId.Text = tarefa.Id.ToString();
            TxtTitulo.Text = tarefa.Titulo;
            TxtDescricao.Text = tarefa.Descricao;
            DpDataInicio.SelectedDate = tarefa.DataInicio;
            TxtHoraInicio.Text = tarefa.DataInicio.Hour.ToString("00");
            TxtMinutoInicio.Text = tarefa.DataInicio.Minute.ToString("00");
            DpDataTermino.SelectedDate = tarefa.DataEntrega;
            TxtHoraTermino.Text = tarefa.DataEntrega.Hour.ToString("00");
            TxtMinutoTermino.Text = tarefa.DataEntrega.Minute.ToString("00");
            TxtPeso.Text = tarefa.Peso.ToString();

            _isEditing = true;
            BtnExcluir.IsEnabled = true;
        }

        private void AtualizarProximoId()
        {
            if (_listaTarefas.Count > 0)
            {
                _proximoId = _listaTarefas.Max(t => t.Id) + 1;
            }
            else
            {
                _proximoId = 1;
            }
        }

        #endregion

        #region Navigation Methods

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            // Navegação para o Dashboard
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Dashboard_Click(sender, e);
            }
        }

        private void Alunos_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Alunos_Click(sender, e);
            }
        }

        private void Grupos_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Grupos_Click(sender, e);
            }
        }

        private void Pauta_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Pauta_Click(sender, e);
            }
        }

        private void Histograma_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Histograma_Click(sender, e);
            }
        }

        private void Ajuda_Click(object sender, RoutedEventArgs e)
        {
            // Implementar ação de ajuda
            MessageBox.Show("Ajuda não implementada");
        }

        private void Sair_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Sair_Click(sender, e);
            }
        }

        private void LoadProfilePhoto()
        {
            try
            {
                if (!string.IsNullOrEmpty(App.ProfessorAtual.Foto) && System.IO.File.Exists(App.ProfessorAtual.Foto))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(App.ProfessorAtual.Foto);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    var profileImage = this.FindName("ProfileImage") as Image;
                    var profileInitials = this.FindName("ProfileInitials") as TextBlock;

                    if (profileImage != null)
                    {
                        profileImage.Source = bitmap;
                        profileImage.Visibility = Visibility.Visible;
                    }

                    if (profileInitials != null)
                    {
                        profileInitials.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    var profileImage = this.FindName("ProfileImage") as Image;
                    var profileInitials = this.FindName("ProfileInitials") as TextBlock;

                    if (profileImage != null)
                    {
                        profileImage.Visibility = Visibility.Collapsed;
                    }

                    if (profileInitials != null)
                    {
                        profileInitials.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar a foto do perfil: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                var profileImage = this.FindName("ProfileImage") as Image;
                var profileInitials = this.FindName("ProfileInitials") as TextBlock;

                if (profileImage != null)
                {
                    profileImage.Visibility = Visibility.Collapsed;
                }

                if (profileInitials != null)
                {
                    profileInitials.Visibility = Visibility.Visible;
                }
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Profile_Click(sender, e);
            }
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Profile_Click(sender, e);
            }
        }

        #endregion

        #region Event Handlers

        private void BtnNovo_Click(object sender, RoutedEventArgs e)
        {
            LimparFormulario();
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(TxtTitulo.Text))
            {
                MessageBox.Show("Por favor, preencha o título da tarefa.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTitulo.Focus();
                return;
            }

            // Verifica se o valor do peso é válido, usando CultureInfo para considerar formatos locais
            if (!float.TryParse(TxtPeso.Text, out float peso) || peso < 0 || peso > 100)
            {
                MessageBox.Show("O peso deve ser um número entre 0 e 100.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPeso.Focus();
                return;
            }

            if (DpDataInicio.SelectedDate == null || DpDataTermino.SelectedDate == null)
            {
                MessageBox.Show("Selecione as datas de início e término.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar horas e minutos
            if (!int.TryParse(TxtHoraInicio.Text, out int horaInicio) || horaInicio < 0 || horaInicio > 23)
            {
                MessageBox.Show("A hora de início deve ser um número entre 0 e 23.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtHoraInicio.Focus();
                return;
            }

            if (!int.TryParse(TxtMinutoInicio.Text, out int minutoInicio) || minutoInicio < 0 || minutoInicio > 59)
            {
                MessageBox.Show("O minuto de início deve ser um número entre 0 e 59.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtMinutoInicio.Focus();
                return;
            }

            if (!int.TryParse(TxtHoraTermino.Text, out int horaTermino) || horaTermino < 0 || horaTermino > 23)
            {
                MessageBox.Show("A hora de término deve ser um número entre 0 e 23.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtHoraTermino.Focus();
                return;
            }

            if (!int.TryParse(TxtMinutoTermino.Text, out int minutoTermino) || minutoTermino < 0 || minutoTermino > 59)
            {
                MessageBox.Show("O minuto de término deve ser um número entre 0 e 59.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtMinutoTermino.Focus();
                return;
            }

            var dataInicio = DpDataInicio.SelectedDate.Value.AddHours(horaInicio).AddMinutes(minutoInicio);
            var dataTermino = DpDataTermino.SelectedDate.Value.AddHours(horaTermino).AddMinutes(minutoTermino);

            if (dataTermino <= dataInicio)
            {
                MessageBox.Show("A data de término deve ser posterior à data de início.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Atualizar ou criar nova tarefa
            if (_isEditing)
            {
                // Atualizando tarefa existente
                var tarefaExistente = _listaTarefas.FirstOrDefault(t => t.Id == _tarefaAtual.Id);
                if (tarefaExistente != null)
                {
                    tarefaExistente.Titulo = TxtTitulo.Text;
                    tarefaExistente.Descricao = TxtDescricao.Text;
                    tarefaExistente.DataInicio = dataInicio;
                    tarefaExistente.DataEntrega = dataTermino;
                    tarefaExistente.Peso = peso;
                }
            }
            else
            {
                // Criando nova tarefa
                var novaTarefa = new Tarefa
                {
                    Id = _proximoId,
                    Titulo = TxtTitulo.Text,
                    Descricao = TxtDescricao.Text,
                    DataInicio = dataInicio,
                    DataEntrega = dataTermino,
                    Peso = peso
                };

                _listaTarefas.Add(novaTarefa);
                AtualizarProximoId();
            }

            // Salvar alterações
            SaveTarefas();

            // Atualizar UI
            DgTarefas.Items.Refresh();
            LimparFormulario();

            MessageBox.Show("Tarefa salva com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExcluir_Click(object sender, RoutedEventArgs e)
        {
            if (_tarefaAtual == null || !_isEditing) return;

            var result = MessageBox.Show($"Deseja realmente excluir a tarefa '{_tarefaAtual.Titulo}'?",
                "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var tarefaParaRemover = _listaTarefas.FirstOrDefault(t => t.Id == _tarefaAtual.Id);
                if (tarefaParaRemover != null)
                {
                    _listaTarefas.Remove(tarefaParaRemover);
                    SaveTarefas();
                    LimparFormulario();
                    MessageBox.Show("Tarefa excluída com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void DgTarefas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgTarefas.SelectedItem is Tarefa tarefaSelecionada)
            {
                PreencherFormulario(tarefaSelecionada);
            }
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            // Este método não parece estar vinculado a nenhum botão no XAML,
            // mas está sendo mantido para compatibilidade
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Dashboard_Click(sender, e);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        #endregion
    }
}