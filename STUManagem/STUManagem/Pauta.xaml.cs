using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Serialization;
using labmockups.MODELS;

namespace trabalhoLAB
{
    /// <summary>
    /// Interaction logic for Pauta.xaml
    /// </summary>
    public partial class Pauta : UserControl
    {
        // Obtém a MainWindow para navegar entre views
        private MainWindow GetMainWindow()
        {
            return Window.GetWindow(this) as MainWindow;
        }

        // Classes para gerenciar os dados exibidos na pauta
        public class Aluno
        {
            public int Id { get; set; }
            public string Numero { get; set; }
            public string Nome { get; set; }
            public string Email { get; set; }
            public int? GrupoId { get; set; }
            public string NomeGrupo { get; set; }
            public Dictionary<int, Nota> Notas { get; set; } = new Dictionary<int, Nota>();
            public double NotaFinal { get; set; }
            public string Situacao { get; set; }
        }

        // Caminho para o arquivo de avaliações
        private static readonly string _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "trabalhoLAB");
        private static readonly string _avaliacoesFilePath = Path.Combine(_appDataPath, "avaliacoes.xml");
        private List<labmockups.MODELS.Avaliacao> _avaliacoes = new List<labmockups.MODELS.Avaliacao>();

        public class Nota : INotifyPropertyChanged
        {
            private double _valor;
            public int TarefaId { get; set; }
            public double Valor
            {
                get { return _valor; }
                set
                {
                    if (_valor != value)
                    {
                        _valor = value;
                        OnPropertyChanged("Valor");
                    }
                }
            }
            public bool Atribuida { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public class Tarefa
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public string Descricao { get; set; }
            public DateTime DataEntrega { get; set; }
            public double Peso { get; set; }
        }

        public class Grupo
        {
            public int Id { get; set; }
            public string Nome { get; set; }
        }

        // Listas de dados
        private ObservableCollection<Aluno> _alunos = new ObservableCollection<Aluno>();
        private List<Tarefa> _tarefas = new List<Tarefa>();
        private List<Grupo> _grupos = new List<Grupo>();

        // Filtros ativos
        private int? _filtroGrupoId = null;
        private int? _filtroTarefaId = null;

        // Flag para verificar se o DataGrid já foi inicializado
        private bool _dataGridInitialized = false;

        public Pauta()
        {
            InitializeComponent();

            // Registrar o evento Loaded para configurar a UI após o controle ser carregado
            this.Loaded += Pauta_Loaded;

            // Adicionar evento específico para o DataGrid
            PautaDataGrid.Loaded += PautaDataGrid_Loaded;
        }

        private void PautaDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Marcar o DataGrid como inicializado
            _dataGridInitialized = true;

            // Agora que o DataGrid está carregado, podemos atualizar a UI
            Dispatcher.InvokeAsync(() => {
                ConfigurarPauta();
                AtualizarEstatisticas();
            });
        }

        private void Pauta_Loaded(object sender, RoutedEventArgs e)
        {
            // Carregar dados (em produção, será substituído por acesso a banco de dados)
            CarregarDados();

            // Configurar os combos
            ConfigurarCombos();

            // A configuração da pauta será feita quando o DataGrid estiver carregado
            // no evento PautaDataGrid_Loaded
            
            // Verificar se o DataGrid já está inicializado
            if (_dataGridInitialized)
            {
                // Forçar uma atualização dos dados da pauta
                ConfigurarPauta();
                AtualizarEstatisticas();
            }
        }

        private void CarregarDados()
        {
            try
            {
                // Limpar as listas antes de carregar
                _grupos.Clear();
                _tarefas.Clear();
                _alunos.Clear();
                _avaliacoes.Clear();

                // Carregar alunos da aplicação
                if (App.ListaAlunos != null)
                {
                    foreach (var alunoModel in App.ListaAlunos)
                    {
                        var grupo = App.ListaGrupos?.FirstOrDefault(g => g.Id == alunoModel.GrupoId);
                        
                        var aluno = new Aluno
                        {
                            Id = alunoModel.Numero,
                            Numero = alunoModel.Numero.ToString(),
                            Nome = alunoModel.Nome,
                            Email = alunoModel.Email,
                            GrupoId = alunoModel.GrupoId,
                            NomeGrupo = grupo?.Nome ?? "Sem Grupo",
                            Notas = new Dictionary<int, Nota>(),
                            NotaFinal = 0,
                            Situacao = "Não Avaliado"
                        };
                        
                        _alunos.Add(aluno);
                    }
                }

                // Carregar tarefas da aplicação
                if (App.ListaTarefas != null)
                {
                    foreach (var tarefaModel in App.ListaTarefas)
                    {
                        var tarefa = new Tarefa
                        {
                            Id = tarefaModel.Id,
                            Nome = tarefaModel.Titulo,
                            Descricao = tarefaModel.Descricao,
                            DataEntrega = tarefaModel.DataEntrega,
                            Peso = tarefaModel.Peso
                        };
                        
                        _tarefas.Add(tarefa);
                    }
                }

                // Carregar grupos da aplicação
                if (App.ListaGrupos != null)
                {
                    foreach (var grupoModel in App.ListaGrupos)
                    {
                        var grupo = new Grupo
                        {
                            Id = grupoModel.Id,
                            Nome = grupoModel.Nome
                        };
                        
                        _grupos.Add(grupo);
                    }
                }

                // Carregar avaliações do arquivo XML
                CarregarAvaliacoes();

                // Calcular notas finais para todos os alunos
                foreach (var aluno in _alunos)
                {
                    CalcularNotaFinalDoAluno(aluno);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CarregarAvaliacoes()
        {
            try
            {
                if (File.Exists(_avaliacoesFilePath))
                {
                    // Deserializar avaliações do arquivo XML
                    var serializer = new XmlSerializer(typeof(List<labmockups.MODELS.Avaliacao>));
                    using var reader = new StreamReader(_avaliacoesFilePath, Encoding.UTF8);
                    _avaliacoes = (List<labmockups.MODELS.Avaliacao>)serializer.Deserialize(reader);

                    // Aplicar as avaliações aos alunos
                    foreach (var avaliacao in _avaliacoes)
                    {
                        var aluno = _alunos.FirstOrDefault(a => int.Parse(a.Numero) == avaliacao.AlunoId);
                        if (aluno != null)
                        {
                            if (!aluno.Notas.ContainsKey(avaliacao.TarefaId))
                            {
                                aluno.Notas[avaliacao.TarefaId] = new Nota
                                {
                                    TarefaId = avaliacao.TarefaId,
                                    Valor = avaliacao.Valor,
                                    Atribuida = true
                                };
                            }
                            else
                            {
                                aluno.Notas[avaliacao.TarefaId].Valor = avaliacao.Valor;
                                aluno.Notas[avaliacao.TarefaId].Atribuida = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar avaliações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

private void SalvarAvaliacoes()
{
    try
    {
        // Criar diretório se não existir
        if (!Directory.Exists(_appDataPath))
        {
            Directory.CreateDirectory(_appDataPath);
        }

        // Atualizar a lista de avaliações com base nos dados atuais
        _avaliacoes.Clear();
        
        // Use a counter for unique IDs
        int idCounter = 1;
        
        foreach (var aluno in _alunos)
        {
            foreach (var nota in aluno.Notas)
            {
                if (nota.Value.Atribuida)
                {
                    // Create the complete Avaliacao object with all required properties
                    var avaliacao = new labmockups.MODELS.Avaliacao
                    {
                        Id = idCounter++,
                        AlunoId = int.Parse(aluno.Numero),
                        TarefaId = nota.Key,
                        Valor = nota.Value.Valor
                    };
                    
                    // If we can find the matching Aluno and Tarefa objects, add them to make the XML complete
                    var alunoObj = App.ListaAlunos.FirstOrDefault(a => a.Numero == int.Parse(aluno.Numero));
                    if (alunoObj != null)
                    {
                        avaliacao.Aluno = alunoObj;
                    }
                    
                    var tarefaObj = App.ListaTarefas.FirstOrDefault(t => t.Id == nota.Key);
                    if (tarefaObj != null)
                    {
                        avaliacao.Tarefa = tarefaObj;
                    }
                    
                    _avaliacoes.Add(avaliacao);
                }
            }
        }

        // Serializar avaliações para o arquivo XML
        var settings = new System.Xml.XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            OmitXmlDeclaration = false
        };

        var serializer = new XmlSerializer(typeof(List<labmockups.MODELS.Avaliacao>));
        using var writer = System.Xml.XmlWriter.Create(_avaliacoesFilePath, settings);
        serializer.Serialize(writer, _avaliacoes);
    }
    catch (Exception ex)
    {
        throw new Exception($"Erro ao salvar avaliações: {ex.Message}", ex);
    }
}

        private void ConfigurarCombos()
        {
            try
            {
                // Clear existing items
                ComboGrupos.Items.Clear();
                ComboTarefas.Items.Clear();

                // Configure ComboBox for Groups
                ComboGrupos.Items.Add(new ComboBoxItem { Content = "Todos os Grupos", Tag = null });
                
                // Add groups without duplicates
                var gruposUnicos = _grupos
                    .GroupBy(g => g.Id)
                    .Select(g => g.First())
                    .OrderBy(g => g.Nome);
                    
                foreach (var grupo in gruposUnicos)
                {
                    ComboGrupos.Items.Add(new ComboBoxItem { Content = grupo.Nome, Tag = grupo.Id });
                }
                ComboGrupos.SelectedIndex = 0;

                // Configure ComboBox for Tasks
                ComboTarefas.Items.Add(new ComboBoxItem { Content = "Todas as Tarefas", Tag = null });
                
                // Add tasks without duplicates
                var tarefasUnicas = _tarefas
                    .GroupBy(t => t.Id)
                    .Select(t => t.First())
                    .OrderBy(t => t.Nome);
                    
                foreach (var tarefa in tarefasUnicas)
                {
                    ComboTarefas.Items.Add(new ComboBoxItem { Content = tarefa.Nome, Tag = tarefa.Id });
                }
                ComboTarefas.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao configurar filtros: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigurarPauta()
        {
            // Verificar se o DataGrid está inicializado
            if (!_dataGridInitialized || PautaDataGrid == null)
            {
                MessageBox.Show("DataGrid não está pronto para ser configurado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Limpar as colunas existentes de forma segura
                Dispatcher.Invoke(() => PautaDataGrid.Columns.Clear());

                // Adicionar colunas fixas
                var numeroColumn = new DataGridTextColumn
                {
                    Header = "Número",
                    Binding = new Binding("Numero"),
                    IsReadOnly = true,
                    Width = 80
                };
                PautaDataGrid.Columns.Add(numeroColumn);

                var nomeColumn = new DataGridTextColumn
                {
                    Header = "Nome do Aluno",
                    Binding = new Binding("Nome"),
                    IsReadOnly = true,
                    Width = 200
                };
                PautaDataGrid.Columns.Add(nomeColumn);

                var grupoColumn = new DataGridTextColumn
                {
                    Header = "Grupo",
                    Binding = new Binding("NomeGrupo"),
                    IsReadOnly = true,
                    Width = 100
                };
                PautaDataGrid.Columns.Add(grupoColumn);

                // Task filtering logic in ConfigurarPauta method
                var tarefasParaMostrar = _tarefas.ToList();
                if (!CheckShowAllTasks.IsChecked.Value && _filtroTarefaId.HasValue)
                {
                    var tarefaSelecionada = _tarefas.FirstOrDefault(t => t.Id == _filtroTarefaId.Value);
                    if (tarefaSelecionada != null)
                    {
                        tarefasParaMostrar = new List<Tarefa> { tarefaSelecionada };
                    }
                }
                // If checkbox is checked, show all tasks regardless of combo selection

                // Adicionar colunas dinâmicas para as tarefas
                foreach (var tarefa in tarefasParaMostrar)
                {
                    var tarefaColumn = new DataGridTextColumn
                    {
                        Header = $"{tarefa.Nome} ({tarefa.Peso:N0}%)",
                        Binding = new Binding($"Notas[{tarefa.Id}].Valor") { StringFormat = "F1" },
                        Width = 100,
                        IsReadOnly = false
                    };
                    PautaDataGrid.Columns.Add(tarefaColumn);
                }

                // Adicionar coluna de nota final
                var notaFinalColumn = new DataGridTextColumn
                {
                    Header = "Nota Final",
                    Binding = new Binding("NotaFinal") { StringFormat = "F1" },
                    IsReadOnly = true,
                    Width = 80
                };
                PautaDataGrid.Columns.Add(notaFinalColumn);

                // Adicionar coluna de situação
                var situacaoColumn = new DataGridTextColumn
                {
                    Header = "Situação",
                    Binding = new Binding("Situacao"),
                    IsReadOnly = true,
                    Width = 100
                };
                PautaDataGrid.Columns.Add(situacaoColumn);

                // Filtrar os alunos com base nas seleções
                var alunosFiltrados = _alunos.ToList();
                if (_filtroGrupoId.HasValue)
                {
                    alunosFiltrados = alunosFiltrados.Where(a => a.GrupoId == _filtroGrupoId.Value).ToList();
                }

                // Atualizar o DataGrid
                PautaDataGrid.ItemsSource = new ObservableCollection<Aluno>(alunosFiltrados);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao configurar a pauta: {ex.Message}\nStackTrace: {ex.StackTrace}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalcularNotaFinalDoAluno(Aluno aluno)
        {
            double somaPesos = 0;
            double somaPonderada = 0;

            foreach (var tarefa in _tarefas)
            {
                if (aluno.Notas.ContainsKey(tarefa.Id) && aluno.Notas[tarefa.Id].Atribuida)
                {
                    somaPesos += tarefa.Peso;
                    somaPonderada += aluno.Notas[tarefa.Id].Valor * tarefa.Peso;
                }
            }

            if (somaPesos > 0)
            {
                aluno.NotaFinal = Math.Round(somaPonderada / somaPesos, 1);
            }
            else
            {
                aluno.NotaFinal = 0;
            }

            // Atualizar situação
            aluno.Situacao = aluno.NotaFinal >= 9.5 ? "Aprovado" : "Reprovado";
        }

        private void AtualizarEstatisticas()
        {
            try
            {
                if (PautaDataGrid.ItemsSource == null)
                    return;

                var alunosComNotas = ((ObservableCollection<Aluno>)PautaDataGrid.ItemsSource)
                    .Where(a => a.NotaFinal > 0)
                    .ToList();

                if (alunosComNotas.Any())
                {
                    double media = alunosComNotas.Average(a => a.NotaFinal);
                    double maxima = alunosComNotas.Max(a => a.NotaFinal);
                    double minima = alunosComNotas.Min(a => a.NotaFinal);
                    double aprovados = alunosComNotas.Count(a => a.NotaFinal >= 9.5);
                    double taxaAprovacao = (aprovados / alunosComNotas.Count) * 100;

                    TxtMediaTurma.Text = media.ToString("F1");
                    TxtNotaMaxima.Text = maxima.ToString("F1");
                    TxtNotaMinima.Text = minima.ToString("F1");
                    TxtTaxaAprovacao.Text = $"{taxaAprovacao:F1}%";
                }
                else
                {
                    TxtMediaTurma.Text = "0,0";
                    TxtNotaMaxima.Text = "0,0";
                    TxtNotaMinima.Text = "0,0";
                    TxtTaxaAprovacao.Text = "0%";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar estatísticas: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Eventos

        // Eventos de navegação
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            // Navegar para Dashboard
            try
            {
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    mainWindow.Dashboard_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Dashboard: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            // Navegar para Perfil
            try
            {
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    mainWindow.Profile_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Perfil: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Alunos_Click(object sender, RoutedEventArgs e)
        {
            // Navegar para Alunos
            try
            {
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    mainWindow.Alunos_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Alunos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Tarefas_Click(object sender, RoutedEventArgs e)
        {
            // Navegar para Tarefas
            try
            {
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    mainWindow.Tarefas_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Tarefas: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Grupos_Click(object sender, RoutedEventArgs e)
        {
            // Navegar para Grupos
            try
            {
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    mainWindow.Grupos_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Grupos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Pauta_Click(object sender, RoutedEventArgs e)
        {
            // Já estamos na Pauta - não faz nada
        }

        private void Histograma_Click(object sender, RoutedEventArgs e)
        {
            // Navegar para Histograma
            try
            {
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    mainWindow.Histograma_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Histograma: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Ajuda_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar janela de ajuda
            MessageBox.Show("Sistema de Gestão de Avaliações - Ajuda\n\n" +
                "Esta tela permite visualizar e gerenciar as notas dos alunos em todas as tarefas avaliativas.",
                "Ajuda", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Sair_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Sair_Click(sender, e);
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar perfil do usuário
            try
            {
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    mainWindow.Profile_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Perfil: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Eventos de filtro
        private void ComboGrupos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboGrupos.SelectedItem != null)
            {
                var item = ComboGrupos.SelectedItem as ComboBoxItem;
                _filtroGrupoId = item.Tag as int?;

                // Verificar se o DataGrid já está inicializado antes de chamar ConfigurarPauta
                if (_dataGridInitialized)
                {
                    ConfigurarPauta();
                    AtualizarEstatisticas();
                }
            }
        }

        private void ComboTarefas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboTarefas.SelectedItem != null)
            {
                var item = ComboTarefas.SelectedItem as ComboBoxItem;
                _filtroTarefaId = item.Tag as int?;
                
                // When selecting "All Tasks", always check the "Show all tasks" checkbox
                if (!_filtroTarefaId.HasValue)
                {
                    CheckShowAllTasks.IsChecked = true;
                }
                
                // When selecting a specific task, only uncheck if it was checked
                if (_filtroTarefaId.HasValue && CheckShowAllTasks.IsChecked == true)
                {
                    CheckShowAllTasks.IsChecked = false;
                }

                if (_dataGridInitialized)
                {
                    ConfigurarPauta();
                    AtualizarEstatisticas();
                }
            }
        }

        private void CheckShowAllTasks_Checked(object sender, RoutedEventArgs e)
        {
            // When checked, show all tasks but maintain current task selection
            if (_dataGridInitialized)
            {
                ConfigurarPauta();
                AtualizarEstatisticas();
            }
        }

        private void CheckShowAllTasks_Unchecked(object sender, RoutedEventArgs e)
        {
            // When unchecked, ensure we have a specific task selected
            if (_filtroTarefaId == null && ComboTarefas.Items.Count > 1)
            {
                ComboTarefas.SelectedIndex = 1; // Select first actual task
                var item = ComboTarefas.SelectedItem as ComboBoxItem;
                _filtroTarefaId = item?.Tag as int?;
            }
            
            if (_dataGridInitialized)
            {
                ConfigurarPauta();
                AtualizarEstatisticas();
            }
        }


        private void BtnAtribuirNotas_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? tarefaId = null;
                int? grupoId = null;

                if (ComboTarefas.SelectedItem != null)
                {
                    var item = ComboTarefas.SelectedItem as ComboBoxItem;
                    tarefaId = item.Tag as int?;
                }

                if (ComboGrupos.SelectedItem != null)
                {
                    var item = ComboGrupos.SelectedItem as ComboBoxItem;
                    grupoId = item.Tag as int?;
                }

                AtribuirNotas atribuirNotas = new AtribuirNotas();

                if (tarefaId.HasValue || grupoId.HasValue)
                {
                   
                }

                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    mainWindow.LoadView(atribuirNotas, "Atribuir Notas", "Atribua notas aos alunos");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir a tela de atribuição de notas: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

private void BtnSalvarAlteracoes_Click(object sender, RoutedEventArgs e)
{
    try
    {
        // Recalculate all final grades before saving
        foreach (var aluno in _alunos)
        {
            CalcularNotaFinalDoAluno(aluno);
        }

        // Actually save the changes to the XML file
        SalvarAvaliacoes();
        
        if (_dataGridInitialized)
        {
            ConfigurarPauta();
            AtualizarEstatisticas();
        }
        
        MessageBox.Show("Alterações salvas com sucesso!", "Salvar",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Erro ao salvar alterações: {ex.Message}", "Erro ao salvar",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Exportação para Excel não disponível nesta versão.", "Funcionalidade indisponível",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCalcularNotas_Click(object sender, RoutedEventArgs e)
        {
            // Recalcular todas as notas finais
            foreach (var aluno in _alunos)
            {
                CalcularNotaFinalDoAluno(aluno);
            }

            if (_dataGridInitialized)
            {
                ConfigurarPauta();
                AtualizarEstatisticas();
            }

            MessageBox.Show("Notas finais recalculadas com sucesso!", "Calcular Notas",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }


private void PautaDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
{
    if (e.EditAction == DataGridEditAction.Commit)
    {
        var aluno = e.Row.Item as Aluno;
        var coluna = e.Column as DataGridTextColumn;

        if (coluna != null && aluno != null)
        {
            var binding = coluna.Binding as Binding;
            if (binding != null && binding.Path.Path.StartsWith("Notas["))
            {
                string path = binding.Path.Path;
                int startIndex = path.IndexOf('[') + 1;
                int endIndex = path.IndexOf(']');
                if (int.TryParse(path.Substring(startIndex, endIndex - startIndex), out int tarefaId))
                {
                    var editElement = e.EditingElement as TextBox;
                    if (editElement != null && double.TryParse(editElement.Text, out double novoValor))
                    {
                        if (novoValor < 0 || novoValor > 20)
                        {
                            MessageBox.Show("A nota deve estar entre 0 e 20.", "Valor inválido",
                                MessageBoxButton.OK, MessageBoxImage.Warning);

                            e.Cancel = true;
                            return;
                        }

                        if (aluno.Notas.ContainsKey(tarefaId))
                        {
                            aluno.Notas[tarefaId].Valor = novoValor;
                            aluno.Notas[tarefaId].Atribuida = true;
                        }
                        else
                        {
                            aluno.Notas[tarefaId] = new Nota
                            {
                                TarefaId = tarefaId,
                                Valor = novoValor,
                                Atribuida = true
                            };
                        }

                        CalcularNotaFinalDoAluno(aluno);
                        
                        // Save changes to XML file after each edit
                        try
                        {
                            SalvarAvaliacoes();
                            // No need for a MessageBox after every edit
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erro ao salvar avaliação: {ex.Message}", "Erro",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        AtualizarEstatisticas();
                    }
                }
            }
        }
    }
}

        private void PautaDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Colorir linhas com base na situação do aluno
            var aluno = e.Row.DataContext as Aluno;
            if (aluno != null)
            {
                if (App.IsDarkTheme)
                {
                    // Dark theme colors
                    e.Row.Background = aluno.Situacao == "Aprovado"
                        ? new SolidColorBrush(Color.FromRgb(40, 100, 60))  // Dark green
                        : aluno.Situacao == "Reprovado"
                            ? new SolidColorBrush(Color.FromRgb(100, 40, 40))  // Dark red
                            : new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    // Light theme colors
                    e.Row.Background = aluno.Situacao == "Aprovado"
                        ? new SolidColorBrush(Color.FromRgb(240, 255, 240))  // Light green
                        : aluno.Situacao == "Reprovado"
                            ? new SolidColorBrush(Color.FromRgb(255, 240, 240))  // Light red
                            : new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        private void DataGridCell_Loaded(object sender, RoutedEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell != null && cell.Column is DataGridTextColumn column)
            {
                // Only apply color to grade columns (exclude student info columns)
                if (column.Header.ToString().Contains("(") || column.Header.ToString() == "Nota Final")
                {
                    // Get the text content
                    var textBlock = cell.Content as TextBlock;
                    if (textBlock != null && double.TryParse(textBlock.Text, out double grade))
                    {
                        // Apply colors based on the grade and current theme
                        if (App.IsDarkTheme)
                        {
                            cell.Background = grade >= 9.5 
                                ? new SolidColorBrush(Color.FromRgb(80, 200, 120))  // Dark mode pass (darker green)
                                : new SolidColorBrush(Color.FromRgb(255, 107, 107)); // Dark mode fail (bright red)
                        }
                        else
                        {
                            cell.Background = grade >= 9.5 
                                ? new SolidColorBrush(Color.FromRgb(144, 238, 144))  // Light mode pass (light green)
                                : new SolidColorBrush(Color.FromRgb(255, 182, 182)); // Light mode fail (light red)
                        }

                        // Set text color for better contrast
                        textBlock.Foreground = App.IsDarkTheme
                            ? new SolidColorBrush(Colors.White)
                            : new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }

        #endregion
    }
}