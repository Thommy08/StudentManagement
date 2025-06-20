using labmockups.MODELS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace trabalhoLAB
{
    public partial class AtribuirNotas : UserControl
    {
        // Coleções para dados
        private ObservableCollection<AlunoNota> _listaAlunosNotas;
        private ObservableCollection<Tarefa> _listaTarefas;
        private ObservableCollection<Grupo> _listaGrupos;

        // Itens selecionados
        private Tarefa _tarefaSelecionada;
        private Grupo _grupoSelecionado;

        // Caminho para o arquivo de avaliações (mesmos caminhos usados em Pauta.xaml.cs)
        private static readonly string _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "trabalhoLAB");
        private static readonly string _avaliacoesFilePath = Path.Combine(_appDataPath, "avaliacoes.xml");

        // Modelo para representar um aluno com sua nota
        public class AlunoNota
        {
            public int Id { get; set; }
            public int Numero { get; set; }
            public string Nome { get; set; }
            public string Nota { get; set; }
            public int AlunoId { get; set; }
        }

        public AtribuirNotas()
        {
            InitializeComponent();

            // Inicializar coleções
            _listaAlunosNotas = new ObservableCollection<AlunoNota>();
            _listaTarefas = new ObservableCollection<Tarefa>(App.ListaTarefas);
            _listaGrupos = new ObservableCollection<Grupo>(App.ListaGrupos);

            // Configurar data sources
            dgAlunos.ItemsSource = _listaAlunosNotas;
            cmbTarefa.ItemsSource = _listaTarefas;
            cmbTarefa.DisplayMemberPath = "Titulo";
            cmbGrupo.ItemsSource = _listaGrupos;
            cmbGrupo.DisplayMemberPath = "Nome";

            // Adicionar event handlers
            cmbTarefa.SelectionChanged += CmbTarefa_SelectionChanged;
            cmbGrupo.SelectionChanged += CmbGrupo_SelectionChanged;
            rbMesmaNota.Checked += RbMesmaNota_Checked;
            rbNotasIndividuais.Checked += RbNotasIndividuais_Checked;
            txtValor.TextChanged += TxtValor_TextChanged;

            // Configurar estado inicial
            txtValor.IsEnabled = true;
            dgAlunos.IsReadOnly = true;
            LimparCampos();
        }

        #region Event Handlers

        // Evento disparado quando uma tarefa é selecionada
        private void CmbTarefa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _tarefaSelecionada = cmbTarefa.SelectedItem as Tarefa;
            AtualizarListaAlunos();
        }

        // Evento disparado quando um grupo é selecionado
        private void CmbGrupo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _grupoSelecionado = cmbGrupo.SelectedItem as Grupo;
            if (_grupoSelecionado != null)
            {
                // Atualizar informações do grupo na UI
                txtIdGrupo.Text = _grupoSelecionado.Id.ToString();
                txtNomeGrupo.Text = _grupoSelecionado.Nome;

                // Contar alunos do grupo
                var alunosDoGrupo = App.ListaAlunos.Where(a => a.GrupoId == _grupoSelecionado.Id).ToList();
                txtNumeroAlunos.Text = alunosDoGrupo.Count.ToString();
            }
            else
            {
                LimparInfoGrupo();
            }

            AtualizarListaAlunos();
        }

        // Evento disparado quando o modo "Mesma nota" é selecionado
        private void RbMesmaNota_Checked(object sender, RoutedEventArgs e)
        {
            if (rbMesmaNota.IsChecked == true)
            {
                txtValor.IsEnabled = true;
                dgAlunos.IsReadOnly = true;

                // Se houver um valor na caixa de texto, atribuir a todos os alunos
                if (!string.IsNullOrEmpty(txtValor.Text) && double.TryParse(txtValor.Text, out double nota))
                {
                    AtribuirMesmaNotaATodos(nota);
                }
            }
        }

        // Evento disparado quando o modo "Notas individuais" é selecionado
        private void RbNotasIndividuais_Checked(object sender, RoutedEventArgs e)
        {
            if (rbNotasIndividuais.IsChecked == true)
            {
                txtValor.IsEnabled = false;
                dgAlunos.IsReadOnly = false;
            }
        }

        // Evento disparado quando o valor da nota é alterado
        private void TxtValor_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Validar a entrada (apenas números e ponto/vírgula)
            Regex regex = new Regex(@"^[0-9]*(?:[\,\.][0-9]*)?$");
            if (!string.IsNullOrEmpty(txtValor.Text) && !regex.IsMatch(txtValor.Text))
            {
                string texto = txtValor.Text;
                txtValor.Text = string.Join("", texto.Where(c => char.IsDigit(c) || c == '.' || c == ','));
                txtValor.CaretIndex = txtValor.Text.Length;
                return;
            }

            // Se estiver no modo "Mesma nota para todos", atualizar todas as notas
            if (rbMesmaNota.IsChecked == true && !string.IsNullOrEmpty(txtValor.Text) &&
                double.TryParse(txtValor.Text, out double nota))
            {
                AtribuirMesmaNotaATodos(nota);
            }
        }

        // Evento do botão Cancelar
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();

            // Navegar de volta para a Pauta
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.LoadView(new Pauta(), "Pauta"); // Navegar para a Pauta
            }
        }

        // Evento do botão Atribuir Notas
        private void BtnAtribuirNotas_Click(object sender, RoutedEventArgs e)
        {
            if (_tarefaSelecionada == null || _grupoSelecionado == null)
            {
                MessageBox.Show("Por favor, selecione uma tarefa e um grupo para atribuir notas.",
                    "Campos Obrigatórios", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verificar se existem alunos na lista
            if (_listaAlunosNotas.Count == 0)
            {
                MessageBox.Show("Não há alunos para atribuir notas.",
                    "Sem Alunos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar todas as notas
            bool notasValidas = true;
            foreach (var alunoNota in _listaAlunosNotas)
            {
                if (string.IsNullOrEmpty(alunoNota.Nota))
                {
                    notasValidas = false;
                    MessageBox.Show($"O aluno {alunoNota.Nome} não tem uma nota atribuída.",
                        "Nota em Falta", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(alunoNota.Nota, out double valorNota) || valorNota < 0 || valorNota > 20)
                {
                    notasValidas = false;
                    MessageBox.Show($"A nota do aluno {alunoNota.Nome} é inválida. As notas devem ser valores entre 0 e 20.",
                        "Nota Inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (notasValidas)
            {
                // Salvar todas as notas
                SalvarNotas();

                MessageBox.Show("As notas foram atribuídas com sucesso!",
                    "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                LimparCampos();

                // Navegar de volta para a Pauta e forçar um recarregamento
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.LoadView(new Pauta(), "Pauta"); // Navegar para a Pauta
                }
            }
        }

        #endregion

        #region Helper Methods

        // Atualiza a lista de alunos baseado na tarefa e grupo selecionados
        private void AtualizarListaAlunos()
        {
            _listaAlunosNotas.Clear();

            // Se não temos tarefa ou grupo selecionado, não mostramos alunos
            if (_tarefaSelecionada == null || _grupoSelecionado == null)
                return;

            // Filtrar alunos do grupo selecionado
            var alunosDoGrupo = App.ListaAlunos.Where(a => a.GrupoId == _grupoSelecionado.Id).ToList();

            // Para cada aluno do grupo, criar um AlunoNota com dados existentes ou vazios
            foreach (var aluno in alunosDoGrupo)
            {
                // Verificar se já existe uma avaliação para este aluno nesta tarefa
                var notaExistente = ObterAvaliacaoAluno(aluno.Numero, _tarefaSelecionada.Id);

                _listaAlunosNotas.Add(new AlunoNota
                {
                    Id = _listaAlunosNotas.Count + 1,
                    Numero = aluno.Numero,
                    Nome = aluno.Nome,
                    Nota = notaExistente != null ? notaExistente.Valor.ToString() : string.Empty,
                    AlunoId = aluno.Numero
                });
            }
        }

        // Limpa todos os campos e seleções
        private void LimparCampos()
        {
            cmbTarefa.SelectedIndex = -1;
            cmbGrupo.SelectedIndex = -1;
            txtValor.Text = string.Empty;
            _tarefaSelecionada = null;
            _grupoSelecionado = null;
            _listaAlunosNotas.Clear();
            LimparInfoGrupo();
        }

        // Limpa as informações do grupo na UI
        private void LimparInfoGrupo()
        {
            txtIdGrupo.Text = "--";
            txtNomeGrupo.Text = "--";
            txtNumeroAlunos.Text = "--";
        }

        // Atribui a mesma nota a todos os alunos da lista
        private void AtribuirMesmaNotaATodos(double nota)
        {
            if (nota >= 0 && nota <= 20)
            {
                foreach (var alunoNota in _listaAlunosNotas)
                {
                    alunoNota.Nota = nota.ToString();
                }
            }
        }

        // Salva as notas de todos os alunos
        private void SalvarNotas()
        {
            foreach (var alunoNota in _listaAlunosNotas)
            {
                if (!string.IsNullOrEmpty(alunoNota.Nota) && double.TryParse(alunoNota.Nota, out double valorNota))
                {
                    SalvarAvaliacaoAluno(alunoNota.Numero, _tarefaSelecionada.Id, valorNota);
                }
            }

            // Persistir as avaliações no arquivo XML
            SalvarAvaliacoes();
        }

        // Obtém uma avaliação existente para um aluno e tarefa - CORRIGIDO
        private Avaliacao ObterAvaliacaoAluno(int alunoNumero, int tarefaId)
        {
            // Carregar avaliações do arquivo se necessário
            var avaliacoes = CarregarAvaliacoesDoArquivo();
            return avaliacoes.FirstOrDefault(a => a.AlunoId == alunoNumero && a.TarefaId == tarefaId);
        }

        // Carrega avaliações do arquivo XML - NOVO MÉTODO
        private List<Avaliacao> CarregarAvaliacoesDoArquivo()
        {
            try
            {
                if (File.Exists(_avaliacoesFilePath))
                {
                    var serializer = new XmlSerializer(typeof(List<labmockups.MODELS.Avaliacao>));
                    using var reader = new StreamReader(_avaliacoesFilePath, Encoding.UTF8);
                    return (List<labmockups.MODELS.Avaliacao>)serializer.Deserialize(reader) ?? new List<Avaliacao>();
                }
                return new List<Avaliacao>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar avaliações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Avaliacao>();
            }
        }

        // Salva ou atualiza uma avaliação - CORRIGIDO
        private void SalvarAvaliacaoAluno(int alunoNumero, int tarefaId, double valor)
        {
            // Carregar todas as avaliações existentes
            var todasAvaliacoes = CarregarAvaliacoesDoArquivo();

            // Verificar se já existe uma avaliação para este aluno e tarefa
            var avaliacaoExistente = todasAvaliacoes.FirstOrDefault(a => a.AlunoId == alunoNumero && a.TarefaId == tarefaId);

            if (avaliacaoExistente != null)
            {
                // Atualizar avaliação existente
                avaliacaoExistente.Valor = valor;
            }
            else
            {
                // Criar nova avaliação
                var novaAvaliacao = new Avaliacao
                {
                    Id = todasAvaliacoes.Count > 0 ? todasAvaliacoes.Max(a => a.Id) + 1 : 1,
                    AlunoId = alunoNumero,
                    TarefaId = tarefaId,
                    Valor = valor,
                    // Obtém referências aos objetos relacionados
                    Aluno = App.ListaAlunos.FirstOrDefault(a => a.Numero == alunoNumero),
                    Tarefa = _tarefaSelecionada
                };

                todasAvaliacoes.Add(novaAvaliacao);
            }

            // Salvar imediatamente todas as avaliações
            SalvarAvaliacoesNoArquivo(todasAvaliacoes);
        }

        // Salva todas as avaliações no arquivo XML - CORRIGIDO
        private void SalvarAvaliacoes()
        {
            // Este método agora é chamado apenas no final, pois cada avaliação individual já é salva
            // Podemos usar para fazer uma validação final ou log se necessário
            return;
        }

        // Novo método para salvar diretamente no arquivo - NOVO MÉTODO
        private void SalvarAvaliacoesNoArquivo(List<Avaliacao> avaliacoes)
        {
            try
            {
                // Criar diretório se não existir
                if (!Directory.Exists(_appDataPath))
                {
                    Directory.CreateDirectory(_appDataPath);
                }

                // Configurar serialização
                var settings = new System.Xml.XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    Indent = true,
                    OmitXmlDeclaration = false
                };

                // Serializar avaliações para o arquivo XML
                var serializer = new XmlSerializer(typeof(List<labmockups.MODELS.Avaliacao>));
                using var writer = System.Xml.XmlWriter.Create(_avaliacoesFilePath, settings);
                serializer.Serialize(writer, avaliacoes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar avaliações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}