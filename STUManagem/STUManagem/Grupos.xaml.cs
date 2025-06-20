using labmockups.MODELS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace trabalhoLAB
{
    public partial class Grupos : UserControl
    {
        private ObservableCollection<Grupo> _listaGrupos;
        private Grupo? _grupoSelecionado;
        private bool _modoEdicao;
        private Grupo? _grupoEmEdicao;

        public Grupos()
        {
            InitializeComponent();
            _listaGrupos = App.ListaGrupos;
            DgGrupos.ItemsSource = _listaGrupos;

            // Desabilitar botões até que um grupo seja selecionado
            BtnEditarGrupo.IsEnabled = false;
            BtnRemoverGrupo.IsEnabled = false;
            BtnGerenciarAlunos.IsEnabled = false;

            // Carregar foto do professor
            LoadProfilePhoto();
        }

        // Obtém a MainWindow para navegar entre views
        private MainWindow? GetMainWindow()
        {
            return Window.GetWindow(this) as MainWindow;
        }

        // Método para carregar a foto do perfil do professor
        private void LoadProfilePhoto()
        {
            try
            {
                // Corrija: remova a barra invertida no final da linha e use FindName para acessar os controles do XAML
                var profileImage = this.FindName("ProfileImage") as System.Windows.Controls.Image;
                var profileInitials = this.FindName("ProfileInitials") as System.Windows.Controls.TextBlock;

                if (profileImage == null || profileInitials == null)
                    return;

                if (!string.IsNullOrEmpty(App.ProfessorAtual.Foto) && System.IO.File.Exists(App.ProfessorAtual.Foto))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(App.ProfessorAtual.Foto);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    profileImage.Source = bitmap;
                    profileImage.Visibility = Visibility.Visible;
                    profileInitials.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Se não há foto, mostrar as iniciais
                    profileImage.Visibility = Visibility.Collapsed;
                    profileInitials.Visibility = Visibility.Visible;

                    // Se tiver o nome completo do professor, poderia mostrar as iniciais
                    // Por enquanto estamos usando o ícone padrão "👤"
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro ao carregar a imagem, mantém o ícone padrão
                MessageBox.Show($"Erro ao carregar a foto do perfil: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                var profileImage = this.FindName("ProfileImage") as System.Windows.Controls.Image;
                var profileInitials = this.FindName("ProfileInitials") as System.Windows.Controls.TextBlock;
                if (profileImage != null) profileImage.Visibility = Visibility.Collapsed;
                if (profileInitials != null) profileInitials.Visibility = Visibility.Visible;
            }
        }

        // Navegação
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            // Navega para o Dashboard
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Dashboard_Click(sender, e);
            }
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            // Navega para o Perfil
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Profile_Click(sender, e);
            }
        }

        private void Alunos_Click(object sender, RoutedEventArgs e)
        {
            // Navega para o Alunos
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Alunos_Click(sender, e);
            }
        }

        private void Tarefas_Click(object sender, RoutedEventArgs e)
        {
            // Navega para o Tarefas
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Tarefas_Click(sender, e);
            }
        }

        private void Grupos_Click(object sender, RoutedEventArgs e)
        {
            // Já estamos na tela de grupos, não faz nada
        }

        private void Pauta_Click(object sender, RoutedEventArgs e)
        {
            // Navega para o Pauta
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Pauta_Click(sender, e);
            }
        }

        private void Histograma_Click(object sender, RoutedEventArgs e)
        {
            // Navega para o Histograma
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Histograma_Click(sender, e);
            }
        }

        private void Ajuda_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sistema de Gestão de Avaliações\nVersão 1.0\n\nPara obter ajuda, contate o suporte técnico.",
                "Ajuda", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Sair_Click(object sender, RoutedEventArgs e)
        {
            // Delegar para a MainWindow a ação de sair
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Sair_Click(sender, e);
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Navega para o Perfil
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Profile_Click(sender, e);
            }
        }

        // Evento quando um item é selecionado no DataGrid
        private void DgGrupos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _grupoSelecionado = DgGrupos.SelectedItem as Grupo;
            BtnEditarGrupo.IsEnabled = _grupoSelecionado != null;
            BtnRemoverGrupo.IsEnabled = _grupoSelecionado != null;
            BtnGerenciarAlunos.IsEnabled = _grupoSelecionado != null;
        }

        // Botão para adicionar novo grupo
        private void BtnAdicionarGrupo_Click(object sender, RoutedEventArgs e)
        {
            IniciarNovoGrupo();
            TxtNomeGrupo.Text = string.Empty;
            BorderFormulario.Visibility = Visibility.Visible;
        }

        // Botão para editar grupo selecionado
        private void BtnEditarGrupo_Click(object sender, RoutedEventArgs e)
        {
            if (_grupoSelecionado == null)
            {
                MessageBox.Show("Por favor, selecione um grupo para editar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IniciarEdicaoGrupo();
            TxtNomeGrupo.Text = _grupoSelecionado.Nome;
            BorderFormulario.Visibility = Visibility.Visible;
        }

        // Botão para remover grupo selecionado
        private void BtnRemoverGrupo_Click(object sender, RoutedEventArgs e)
        {
            if (_grupoSelecionado == null)
            {
                MessageBox.Show("Por favor, selecione um grupo para remover.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirmação antes de remover
            MessageBoxResult result = MessageBox.Show(
                $"Tem a certeza que deseja remover o grupo '{_grupoSelecionado.Nome}'?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                RemoverGrupo();
                MessageBox.Show("Grupo removido com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Botão para gerir alunos do grupo
        private void BtnGerenciarAlunos_Click(object sender, RoutedEventArgs e)
        {
            if (_grupoSelecionado == null)
            {
                MessageBox.Show("Por favor, selecione um grupo para gerenciar alunos.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Obtenha a MainWindow e use o método LoadView para trocar o conteúdo principal
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.LoadView(new GerirAlunosGrupo(_grupoSelecionado), 
                    $"Gerenciar Alunos - {_grupoSelecionado.Nome}", 
                    "Adicione ou remova alunos deste grupo");
            }
        }

        // Salvar informações do grupo (novo ou editado)
        private void BtnSalvarGrupo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNomeGrupo.Text))
            {
                MessageBox.Show("Por favor, informe o nome do grupo!", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_modoEdicao)
            {
                // Atualiza o grupo existente
                SalvarEdicaoGrupo(TxtNomeGrupo.Text);
                MessageBox.Show("Grupo atualizado com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Cria um novo grupo
                SalvarNovoGrupo(TxtNomeGrupo.Text);
                MessageBox.Show("Grupo criado com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            BorderFormulario.Visibility = Visibility.Collapsed;
        }

        // Cancelar operação de adição/edição
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            BorderFormulario.Visibility = Visibility.Collapsed;
        }

        // Métodos auxiliares movidos do ViewModel para o code-behind
        private void IniciarNovoGrupo()
        {
            _modoEdicao = false;
            _grupoSelecionado = null;
        }

        private void IniciarEdicaoGrupo()
        {
            if (_grupoSelecionado == null) return;

            _modoEdicao = true;
            _grupoEmEdicao = _grupoSelecionado;
        }

        private void SalvarNovoGrupo(string nome)
        {
            var novoGrupo = new Grupo
            {
                Id = GerarNovoId(),
                Nome = nome,
                Alunos = new List<Aluno>(), // Garante que a lista não é nula
                NumeroAlunos = 0
            };

            _listaGrupos.Add(novoGrupo);
            _grupoSelecionado = novoGrupo;

            // Salvar para persistência
            App.SaveGrupos();
        }

        private void SalvarEdicaoGrupo(string nome)
        {
            if (_grupoEmEdicao != null)
            {
                _grupoEmEdicao.Nome = nome;

                // Força atualização da UI
                var index = _listaGrupos.IndexOf(_grupoEmEdicao);
                if (index >= 0)
                {
                    _listaGrupos[index] = _grupoEmEdicao;
                    DgGrupos.Items.Refresh();
                }

                // Salvar para persistência
                App.SaveGrupos();
            }
        }

        private void RemoverGrupo()
        {
            if (_grupoSelecionado != null)
            {
                // Remover alunos do grupo primeiro
                foreach (var aluno in _grupoSelecionado.Alunos.ToList())
                {
                    App.RemoveAlunoFromGrupo(aluno);
                }

                _listaGrupos.Remove(_grupoSelecionado);
                _grupoSelecionado = null;

                // Salvar para persistência
                App.SaveGrupos();
            }
        }

        private void AtualizarGrupo(Grupo grupo)
        {
            if (grupo == null) return;

            // Atualiza o grupo na coleção para atualizar a UI
            var index = _listaGrupos.IndexOf(grupo);
            if (index >= 0)
            {
                _listaGrupos[index] = grupo;
                DgGrupos.Items.Refresh();
            }

            // Salvar para persistência
            App.SaveGrupos();
        }

        private int GerarNovoId()
        {
            // Gerar um novo ID baseado no maior ID existente + 1
            return _listaGrupos.Count > 0 ? _listaGrupos.Max(g => g.Id) + 1 : 1;
        }
    }
}