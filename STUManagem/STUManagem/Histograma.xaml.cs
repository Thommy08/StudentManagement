using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using labmockups.MODELS;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using System.Text;

namespace trabalhoLAB
{
    public partial class Histograma : UserControl
    {
        // Obtém a MainWindow para navegar entre views
        private MainWindow GetMainWindow()
        {
            return Window.GetWindow(this) as MainWindow;
        }
        private List<double> notasAtuais = new List<double>();
        private string tarefaSelecionada = "Todas as Tarefas";
        private string grupoSelecionado = "Todos os Grupos";
        private bool _isInitializing = true;
        private List<labmockups.MODELS.Avaliacao> _avaliacoes = new List<labmockups.MODELS.Avaliacao>();
        private static readonly string _appDataPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "trabalhoLAB");
        private static readonly string _avaliacoesFilePath = System.IO.Path.Combine(_appDataPath, "avaliacoes.xml");
        
        // UI element references
        private TextBlock _mediaValue;
        private TextBlock txtMediaTurma;
        private TextBlock _medianaValue;
        private TextBlock _desvioPadraoValue;
        private TextBlock _numAlunosValue;
        private TextBlock _aprovadosValue;
        private TextBlock _infoTipoValue;
        private TextBlock _txtTaxaAprovacao;

        public Histograma()
        {
            InitializeComponent();
            
            // Initialize UI element references
            _mediaValue = (TextBlock)this.FindName("MediaValue");
            _medianaValue = (TextBlock)this.FindName("MedianaValue");
            _desvioPadraoValue = (TextBlock)this.FindName("DesvioPadraoValue");
            _numAlunosValue = (TextBlock)this.FindName("NumAlunosValue");
            _aprovadosValue = (TextBlock)this.FindName("AprovadosValue");
            _infoTipoValue = (TextBlock)this.FindName("InfoTipoValue");
            txtNotaMaxima = (TextBlock)this.FindName("TxtNotaMaxima");
            _txtTaxaAprovacao = (TextBlock)this.FindName("TxtTaxaAprovacao");
            
            // Find and reference TxtMediaTurma
            txtMediaTurma = (TextBlock)this.FindName("TxtMediaTurma");
            
            // Register Loaded event
            this.Loaded += Histograma_Loaded;
        }

        private void Histograma_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isInitializing = true;
                CarregarAvaliacoes();
                CarregarDados();
                _isInitializing = false;
                
                // Initial update
                BtnGerarHistograma_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar histograma: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private TextBlock _txtNotaMinima;
        private TextBlock GetTxtNotaMinima()
        {
            if (_txtNotaMinima == null)
            {
                _txtNotaMinima = (TextBlock)this.FindName("TxtNotaMinima");
            }
            return _txtNotaMinima;
        }

        private void SetTxtNotaMinima(TextBlock value)
        {
            _txtNotaMinima = value;
        }
        private TextBlock txtNotaMaxima;

        public TextBlock GetTxtNotaMaxima()
        {
            return txtNotaMaxima;
        }

        public void SetTxtNotaMaxima(TextBlock value)
        {
            txtNotaMaxima = value;
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar avaliações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                _avaliacoes = new List<labmockups.MODELS.Avaliacao>();
            }
        }

        private void CarregarDados()
        {
            PopularCombos();
            AtualizarHistograma();
        }

        private void PopularCombos()
        {
            try
            {
                // Tarefa Selector
                TarefaSelector.Items.Clear();
                TarefaSelector.Items.Add(new ComboBoxItem() { Content = "Todas as Tarefas", IsSelected = true });

                // Adiciona tarefas do sistema sem duplicatas
                var tarefasUnicas = App.ListaTarefas
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .OrderBy(t => t.Titulo);

                foreach (var tarefa in tarefasUnicas)
                {
                    if (!TarefaSelector.Items.Cast<ComboBoxItem>()
                        .Any(item => item.Content.ToString() == tarefa.Titulo))
                    {
                        TarefaSelector.Items.Add(new ComboBoxItem() { Content = tarefa.Titulo });
                    }
                }

                // Adiciona opção de Nota Final
                TarefaSelector.Items.Add(new ComboBoxItem() { Content = "Nota Final" });

                // Grupo Selector
                GrupoSelector.Items.Clear();
                GrupoSelector.Items.Add(new ComboBoxItem() { Content = "Todos os Grupos", IsSelected = true });

                // Adiciona grupos sem duplicatas
                var gruposUnicos = App.ListaGrupos
                    .GroupBy(g => g.Id)
                    .Select(g => g.First())
                    .OrderBy(g => g.Nome);

                foreach (var grupo in gruposUnicos)
                {
                    if (!GrupoSelector.Items.Cast<ComboBoxItem>()
                        .Any(item => item.Content.ToString() == grupo.Nome))
                    {
                        GrupoSelector.Items.Add(new ComboBoxItem() { Content = grupo.Nome });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao popular combos: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AtualizarHistograma()
        {
            ObterNotasFiltradas();
            DesenharHistograma();
            AtualizarEstatisticas();
        }

        private void ObterNotasFiltradas()
        {
            try
            {
                notasAtuais.Clear();

                // Return if there are no tasks
                if (App.ListaTarefas.Count == 0 && tarefaSelecionada != "Todas as Tarefas")
                {
                    return;
                }

                // Filtrar por grupo se necessário
                var alunosFiltrados = App.ListaAlunos.ToList();

                if (grupoSelecionado != "Todos os Grupos")
                {
                    var grupo = App.ListaGrupos.FirstOrDefault(g => g.Nome == grupoSelecionado);
                    if (grupo != null)
                    {
                        alunosFiltrados = alunosFiltrados.Where(a => a.GrupoId == grupo.Id).ToList();
                    }
                }

                // Para cada aluno, obter as notas de acordo com a tarefa selecionada
                foreach (var aluno in alunosFiltrados)
                {
                    var notas = ObterNotaAluno(aluno, tarefaSelecionada);
                    notasAtuais.AddRange(notas);
                }

                // Sort the notes for better visualization
                notasAtuais.Sort();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao filtrar notas: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<double> ObterNotaAluno(Aluno aluno, string tarefaSelecionada)
        {
            try
            {
                // Always load the latest evaluations if needed
                if (_avaliacoes.Count == 0)
                {
                    CarregarAvaliacoes();
                }

                // Get all grades for this student
                var avaliacoesDoAluno = _avaliacoes.Where(a => a.AlunoId == aluno.Numero).ToList();
                var notas = new List<double>();

                if (!avaliacoesDoAluno.Any())
                    return notas;

                // Calculate based on selection
                switch (tarefaSelecionada)
                {
                    case "Nota Final":
                        // Calculate weighted average
                        double somaPesos = 0;
                        double somaPonderada = 0;

                        foreach (var avaliacao in avaliacoesDoAluno)
                        {
                            var tarefa = App.ListaTarefas.FirstOrDefault(t => t.Id == avaliacao.TarefaId);
                            if (tarefa != null)
                            {
                                somaPesos += tarefa.Peso;
                                somaPonderada += avaliacao.Valor * tarefa.Peso;
                            }
                        }

                        if (somaPesos > 0)
                        {
                            notas.Add(Math.Round(somaPonderada / somaPesos, MidpointRounding.AwayFromZero));
                        }
                        break;

                    case "Todas as Tarefas":
                        // Return all individual grades
                        foreach (var avaliacao in avaliacoesDoAluno)
                        {
                            if (avaliacao.Valor > 0)
                            {
                                notas.Add(Math.Round(avaliacao.Valor, MidpointRounding.AwayFromZero));
                            }
                        }
                        break;

                    default:
                        // Find grade for specific task
                        var tarefaEspecifica = App.ListaTarefas.FirstOrDefault(t => t.Titulo == tarefaSelecionada);
                        if (tarefaEspecifica != null)
                        {
                            var avaliacaoEspecifica = avaliacoesDoAluno
                                .FirstOrDefault(a => a.TarefaId == tarefaEspecifica.Id);
                            if (avaliacaoEspecifica != null && avaliacaoEspecifica.Valor > 0)
                            {
                                notas.Add(Math.Round(avaliacaoEspecifica.Valor, MidpointRounding.AwayFromZero));
                            }
                        }
                        break;
                }

                return notas;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter nota do aluno: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<double>();
            }
        }

        private void DesenharHistograma()
        {
            try
            {
                HistogramCanvas.Children.Clear();

                if (notasAtuais.Count == 0)
                {
                    TextBlock mensagem = new TextBlock
                    {
                        Text = "Não há dados para exibir no histograma.",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0),
                        FontSize = 14,
                        Foreground = new SolidColorBrush(Colors.Gray)
                    };

                    Canvas.SetLeft(mensagem, (HistogramCanvas.ActualWidth - 200) / 2);
                    Canvas.SetTop(mensagem, (HistogramCanvas.ActualHeight - 20) / 2);
                    
                    HistogramCanvas.Children.Add(mensagem);
                    return;
                }

                // New configuration values
                int numBins = 21;
                double startX = 40; // Reduce from 50
                double usableWidth = 600; // Reduce from 800
                double barWidth = 25; // Reduce from 35
                double spacing = 2; // Reduce from 4
                double verticalOffset = 40;
                double canvasHeight = HistogramCanvas.Height;
                double maxHeight = canvasHeight - 60;

                // Count frequencies
                int[] frequency = new int[21];
                foreach (double nota in notasAtuais)
                {
                    int gradeIndex = (int)Math.Ceiling(nota);
                    if (gradeIndex >= 0 && gradeIndex <= 20)
                    {
                        frequency[gradeIndex]++;
                    }
                }

                int maxFreq = frequency.Max();
                double scaleY = maxFreq > 0 ? maxHeight / maxFreq : 0;
                double xStep = usableWidth / 20.0; // Space between grade points

                // Draw bars and labels
                for (int i = 0; i <= 20; i++)
                {
                    double x = startX + (i * xStep);
                    double height = frequency[i] * scaleY;
                    double y = canvasHeight - height - verticalOffset;

                    // Draw bar
                    Rectangle bar = new Rectangle
                    {
                        Width = barWidth,
                        Height = height,
                        RadiusX = 3,
                        RadiusY = 3
                    };

                    LinearGradientBrush gradientBrush = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 1)
                    };
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(91, 91, 214), 0.0));
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(73, 73, 179), 1.0));
                    bar.Fill = gradientBrush;

                    Canvas.SetLeft(bar, x - (barWidth / 2));
                    Canvas.SetTop(bar, y);

                    // Add tooltip to each bar
                    ToolTip barTooltip = new ToolTip
                    {
                        Content = $"Nota: {i}"
                    };
                    bar.ToolTip = barTooltip;

                    // Draw grade number
                    TextBlock gradeLabel = new TextBlock
                    {
                        Text = i.ToString(),
                        FontSize = 10, // Reduce from 12
                        FontWeight = FontWeights.SemiBold,
                        Foreground = new SolidColorBrush(Colors.DarkGray),
                        TextAlignment = TextAlignment.Center,
                        Width = barWidth
                    };

                    Canvas.SetLeft(gradeLabel, x - (barWidth / 2));
                    Canvas.SetTop(gradeLabel, canvasHeight - 25);

                    // Draw frequency label if > 0
                    if (frequency[i] > 0)
                    {
                        TextBlock freqLabel = new TextBlock
                        {
                            Text = frequency[i].ToString(),
                            FontSize = 12,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = new SolidColorBrush(Colors.DarkSlateBlue),
                            TextAlignment = TextAlignment.Center,
                            Width = barWidth
                        };

                        Canvas.SetLeft(freqLabel, x - (barWidth / 2));
                        Canvas.SetTop(freqLabel, y - 20);
                        HistogramCanvas.Children.Add(freqLabel);
                    }

                    // Add tick mark
                    Line tick = new Line
                    {
                        X1 = x,
                        Y1 = canvasHeight - verticalOffset,
                        X2 = x,
                        Y2 = canvasHeight - verticalOffset + 5,
                        Stroke = new SolidColorBrush(Colors.DarkGray),
                        StrokeThickness = 1
                    };

                    HistogramCanvas.Children.Add(tick);
                    HistogramCanvas.Children.Add(bar);
                    HistogramCanvas.Children.Add(gradeLabel);
                }

                // Draw axes
                DrawAxis(usableWidth + startX, canvasHeight, maxFreq, scaleY, verticalOffset);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao desenhar histograma: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DrawAxis(double totalCanvasWidth, double canvasHeight, int maxFreq, double scaleY, double verticalOffset)
        {
            // Eixo Y
            Line yAxis = new Line
            {
                X1 = 50,
                Y1 = 10,
                X2 = 50,
                Y2 = canvasHeight - verticalOffset,
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 1
            };
            HistogramCanvas.Children.Add(yAxis);

            // Eixo X
            Line xAxis = new Line
            {
                X1 = 50,
                Y1 = canvasHeight - verticalOffset,
                X2 = 50 + totalCanvasWidth,
                Y2 = canvasHeight - verticalOffset,
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 1
            };
            HistogramCanvas.Children.Add(xAxis);
            
            // Adicionar rótulos dos eixos
            TextBlock yAxisLabel = new TextBlock
            {
                Text = "Frequência",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Gray),
                RenderTransform = new RotateTransform(-90)
            };
            Canvas.SetLeft(yAxisLabel, 10);
            Canvas.SetTop(yAxisLabel, canvasHeight / 2 + 20); // Adjusted position
            HistogramCanvas.Children.Add(yAxisLabel);

            TextBlock xAxisLabel = new TextBlock
            {
                Text = "Notas",
                FontSize = 12, // Reduce from 14
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Colors.Gray)
            };
            Canvas.SetLeft(xAxisLabel, 50 + (totalCanvasWidth) / 2 - 20);
            Canvas.SetTop(xAxisLabel, canvasHeight - 5); // Adjusted position
            HistogramCanvas.Children.Add(xAxisLabel);

            // Marcadores do eixo Y com valores mais legíveis
            int stepSize = Math.Max(1, maxFreq / 5);
            for (int i = 0; i <= maxFreq; i += stepSize)
            {
                double y = canvasHeight - verticalOffset - (i * scaleY);

                TextBlock label = new TextBlock
                {
                    Text = i.ToString(),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Gray)
                };

                Canvas.SetLeft(label, 30);
                Canvas.SetTop(label, y - 8);

                Line tick = new Line
                {
                    X1 = 45,
                    Y1 = y,
                    X2 = 50,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Colors.Gray),
                    StrokeThickness = 1
                };

                HistogramCanvas.Children.Add(label);
                HistogramCanvas.Children.Add(tick);
            }
        }

        private void AtualizarEstatisticas()
        {
            try
            {
                if (notasAtuais.Count == 0)
                {
                    // Update basic statistics with placeholders
                    if (_mediaValue != null) _mediaValue.Text = "-";
                    if (_medianaValue != null) _medianaValue.Text = "";
                    GetTxtNotaMinima().Text = "-";
                    GetTxtNotaMaxima().Text = "-";
                    if (_txtTaxaAprovacao != null) _txtTaxaAprovacao.Text = "0%";
                    
                    // Update additional statistics
                    if (_desvioPadraoValue != null) _desvioPadraoValue.Text = "-";
                    if (_numAlunosValue != null) _numAlunosValue.Text = "0";
                    if (_aprovadosValue != null) _aprovadosValue.Text = "0%";
                    if (_infoTipoValue != null) _infoTipoValue.Text = "Nenhum dado disponível";
                    
                    return;
                }

                // Calcular estatísticas básicas
                double media = notasAtuais.Average();
                double min = notasAtuais.Min();
                double max = notasAtuais.Max();

                // Calcular mediana
                var notasOrdenadas = notasAtuais.OrderBy(n => n).ToList();
                double mediana;
                int count = notasOrdenadas.Count;
                
                if (count % 2 == 0)
                {
                    mediana = (notasOrdenadas[count / 2 - 1] + notasOrdenadas[count / 2]) / 2;
                }
                else
                {
                    mediana = notasOrdenadas[count / 2];
                }

                // Calcular desvio padrão
                double somaDiferencasQuadrado = notasAtuais.Sum(nota => Math.Pow(nota - media, 2));
                double desvioPadrao = Math.Sqrt(somaDiferencasQuadrado / count);

                // Calcular taxa de aprovação
                int aprovados = notasAtuais.Count(n => n >= 9.5);
                double taxaAprovacao = (double)aprovados / count * 100;

                // Atualizar a UI - valores básicos
                if (_mediaValue != null) _mediaValue.Text = $"{media:0.0}";
                if (_medianaValue != null) _medianaValue.Text = ""; // Remove mediana display
                GetTxtNotaMinima().Text = min.ToString("0.0");
                GetTxtNotaMaxima().Text = max.ToString("0.0");
                if (_txtTaxaAprovacao != null) _txtTaxaAprovacao.Text = $"{taxaAprovacao:0.0}%";
                
                // Atualizar a UI - valores adicionais
                if (_desvioPadraoValue != null) _desvioPadraoValue.Text = desvioPadrao.ToString("0.0");
                if (_numAlunosValue != null) _numAlunosValue.Text = count.ToString();
                if (_aprovadosValue != null) _aprovadosValue.Text = $"{taxaAprovacao:0.0}%";

                // Adicionar informação sobre o tipo de dados sendo exibidos
                if (_infoTipoValue != null)
                {
                    string infoTipo = "";
                    if (tarefaSelecionada == "Todas as Tarefas")
                    {
                        infoTipo = "Mostrando todas as notas individuais";
                    }
                    else if (tarefaSelecionada == "Nota Final")
                    {
                        infoTipo = "Mostrando médias finais ponderadas";
                    }
                    else
                    {
                        infoTipo = $"Mostrando notas da tarefa: {tarefaSelecionada}";
                    }

                    if (grupoSelecionado != "Todos os Grupos")
                    {
                        infoTipo += $" (Grupo: {grupoSelecionado})";
                    }

                    _infoTipoValue.Text = infoTipo;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar estatísticas: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event handler removed and consolidated with BtnGerarHistograma_Click

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Dashboard_Click(sender, e);
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Dashboard_Click(sender, e);
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

        private void Alunos_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Alunos_Click(sender, e);
            }
        }

        private void Tarefas_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Tarefas_Click(sender, e);
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
            // Already on Histograma view - do nothing
        }

        private void Ajuda_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ajuda do Sistema de Gestão de Avaliações\n\n" +
                            "Esta página mostra o histograma de distribuição de notas.\n\n" +
                            "- Selecione uma tarefa específica ou todas as tarefas\n" +
                            "- Filtre por grupo ou veja dados de todos os grupos\n" +
                            "- Clique em 'Atualizar' para aplicar os filtros\n\n" +
                            "As estatísticas de média, mediana, nota mínima e máxima são mostradas abaixo do gráfico.",
                            "Ajuda - Histograma", MessageBoxButton.OK, MessageBoxImage.Information);
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
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Profile_Click(sender, e);
            }
        }

        private void BtnGerarHistograma_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TarefaSelector.SelectedItem is ComboBoxItem tarefaItem &&
                    GrupoSelector.SelectedItem is ComboBoxItem grupoItem)
                {
                    tarefaSelecionada = tarefaItem.Content.ToString();
                    grupoSelecionado = grupoItem.Content.ToString();

                    // Reload latest data and update histogram
                    CarregarAvaliacoes();
                    AtualizarHistograma();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar histograma: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TarefaSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitializing && IsLoaded)
            {
                BtnGerarHistograma_Click(sender, null);
            }
        }

        private void GrupoSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitializing && IsLoaded)
            {
                BtnGerarHistograma_Click(sender, null);
            }
        }
    }
}
