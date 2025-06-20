using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using labmockups.MODELS;


namespace trabalhoLAB
{
    public partial class GerirAlunosGrupo : UserControl
    {
        private Grupo _grupo;
        private ObservableCollection<Aluno> _alunosSemGrupo;
        private ObservableCollection<Aluno> _alunosDoGrupo;
        private List<Aluno> _todosAlunosSemGrupo; // Lista completa para pesquisa

        public GerirAlunosGrupo(Grupo grupo)
        {
            InitializeComponent();
            _grupo = grupo;

            // Inicializa as coleções
            _alunosDoGrupo = new ObservableCollection<Aluno>(grupo.Alunos ?? new List<Aluno>());

            // Garante que App.ListaAlunos não seja nulo
            var todosAlunos = App.ListaAlunos?.ToList() ?? new List<Aluno>();
            
            // Get only students that are not in any group (except the current group's students)
            _todosAlunosSemGrupo = todosAlunos.Where(a => 
                (a.GrupoId == null || a.GrupoId == _grupo.Id) &&
                !App.ListaGrupos.Any(g => g.Id != grupo.Id && g.Alunos?.Any(ga => ga.Numero == a.Numero) == true)
            ).ToList();
            
            _alunosSemGrupo = new ObservableCollection<Aluno>(_todosAlunosSemGrupo);

            // Define os sources das listas
            LstAlunosSemGrupo.ItemsSource = _alunosSemGrupo;
            LstAlunosDoGrupo.ItemsSource = _alunosDoGrupo;

            // Configura o título da janela
            LblTitulo.Text = $"Gerenciar Alunos - Grupo: {_grupo.Nome}";

            // Inicializa o contador de alunos
            AtualizarContadorAlunos();
        }

        private void BtnAdicionarAluno_Click(object sender, RoutedEventArgs e)
        {
            if (LstAlunosSemGrupo.SelectedItem is Aluno alunoSelecionado)
            {
                try
                {
                    // Check if student is already in any group
                    if (App.ListaGrupos?.Any(g => g.Id != _grupo.Id && g.Alunos?.Any(a => a.Numero == alunoSelecionado.Numero) == true) == true)
                    {
                        MessageBox.Show("Este aluno já está atribuído a outro grupo.", "Aviso",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Adiciona o aluno ao grupo
                    App.AddAlunoToGrupo(alunoSelecionado, _grupo);

                    // Atualiza as coleções
                    _alunosSemGrupo.Remove(alunoSelecionado);
                    _todosAlunosSemGrupo.Remove(alunoSelecionado);
                    _alunosDoGrupo.Add(alunoSelecionado);

                    // Atualiza o contador
                    AtualizarContadorAlunos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao adicionar aluno ao grupo: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecione um aluno para adicionar ao grupo.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRemoverAluno_Click(object sender, RoutedEventArgs e)
        {
            if (LstAlunosDoGrupo.SelectedItem is Aluno alunoSelecionado)
            {
                try
                {
                    // Remove o aluno do grupo
                    App.RemoveAlunoFromGrupo(alunoSelecionado);

                    // Atualiza as coleções
                    _alunosDoGrupo.Remove(alunoSelecionado);
                    _alunosSemGrupo.Add(alunoSelecionado);
                    _todosAlunosSemGrupo.Add(alunoSelecionado);

                    // Atualiza o contador
                    AtualizarContadorAlunos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao remover aluno do grupo: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecione um aluno para remover do grupo.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Obtém a MainWindow para navegar entre views
        private MainWindow GetMainWindow()
        {
            return Window.GetWindow(this) as MainWindow;
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            // Quando usado como UserControl, navegamos de volta para a view anterior
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                // Navegar de volta para a tela de Grupos
                mainWindow.Grupos_Click(sender, e);
            }
        }

        private void TxtPesquisaAlunos_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = TxtPesquisaAlunos.Text?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(filtro))
            {
                // Restaura a lista completa
                LstAlunosSemGrupo.ItemsSource = _alunosSemGrupo;
            }
            else
            {
                // Filtra a lista a partir da lista completa
                var alunosFiltrados = _todosAlunosSemGrupo.Where(a =>
                    (a.Nome?.ToLower().Contains(filtro) == true) ||
                    a.Numero.ToString().Contains(filtro)
                ).ToList();

                LstAlunosSemGrupo.ItemsSource = alunosFiltrados;
            }
        }

        private void AtualizarContadorAlunos()
        {
            LblTotalAlunos.Text = $"Total de alunos no grupo: {_alunosDoGrupo.Count}";
        }
    }
}