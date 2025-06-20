using labmockups.MODELS;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace trabalhoLAB
{
    public partial class Alunos : UserControl
    {
        // Agora usamos a coleção centralizada do App
        public ObservableCollection<Aluno> ListaAlunos => App.ListaAlunos;

        public Alunos()
        {
            InitializeComponent();

            // Set the DataContext to this instance so the DataGrid can bind to ListaAlunos
            this.DataContext = this;

            // Não precisamos mais carregar alunos aqui, pois App.cs já faz isso
        }

#region Navigation Methods

        // Obtém a MainWindow para navegar entre views
        private MainWindow GetMainWindow()
        {
            return Window.GetWindow(this) as MainWindow;
        }

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
            // Já está na página de Alunos, apenas atualiza a visualização
            AlunosDataGrid.Items.Refresh();
        }

        private void Tarefas_Click(object sender, RoutedEventArgs e)
        {
            // Navega para Tarefas
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Tarefas_Click(sender, e);
            }
        }

        private void Grupos_Click(object sender, RoutedEventArgs e)
        {
            // Navega para Grupos
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Grupos_Click(sender, e);
            }
        }

        private void Pauta_Click(object sender, RoutedEventArgs e)
        {
            // Navega para Pauta
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Pauta_Click(sender, e);
            }
        }

        private void Histograma_Click(object sender, RoutedEventArgs e)
        {
            // Navega para Histograma
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.Histograma_Click(sender, e);
            }
        }

        private void Ajuda_Click(object sender, RoutedEventArgs e)
        {
            // Show help dialog with more detailed help information
            string helpText =
                "Sistema de Gestão de Avaliações - Gestão de Alunos\n\n" +
                "Nesta seção, você pode:\n" +
                "• Adicionar novos alunos\n" +
                "• Editar informações de alunos existentes\n" +
                "• Remover alunos do sistema\n" +
                "• Importar listas de alunos de arquivos CSV ou Excel (XLSX)\n\n" +
                "Os alunos adicionados estarão disponíveis para serem atribuídos a grupos " +
                "na seção de Grupos.";

            MessageBox.Show(helpText, "Ajuda - Gestão de Alunos", MessageBoxButton.OK, MessageBoxImage.Information);
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

        #endregion

        #region Student Management Methods

        public void btnAdicionarAluno_Click(object sender, RoutedEventArgs e)
        {
            // Create window to add a new student
            var addWindow = new AlunoEditDialog();

            // Set the owner to center properly
            addWindow.Owner = Window.GetWindow(this);

            if (addWindow.ShowDialog() == true)
            {
                // Check if the student number already exists
                if (ListaAlunos.Any(a => a.Numero == addWindow.AlunoEditado.Numero))
                {
                    MessageBox.Show($"Um aluno com o número {addWindow.AlunoEditado.Numero} já existe.",
                        "Número duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Add the new student to the collection
                ListaAlunos.Add(addWindow.AlunoEditado);

                // Save changes using the App class
                App.SaveAlunos();
            }
        }

        public void btnEditarAluno_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected student from DataGrid
            var selectedAluno = AlunosDataGrid.SelectedItem as Aluno;

            if (selectedAluno == null)
            {
                MessageBox.Show("Por favor, selecione um aluno para editar.",
                    "Nenhum aluno selecionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create window to edit the student
            var editWindow = new AlunoEditDialog(selectedAluno);

            // Set the owner to center properly
            editWindow.Owner = Window.GetWindow(this);

            if (editWindow.ShowDialog() == true)
            {
                // Update the selected student with edited values
                // Normalize the name to ensure special characters display correctly
                selectedAluno.Nome = HttpUtility.HtmlDecode(editWindow.AlunoEditado.Nome).Normalize(NormalizationForm.FormC);
                selectedAluno.Email = editWindow.AlunoEditado.Email;

                // Save changes using the App class
                App.SaveAlunos();

                // Refresh the DataGrid to show updated data
                AlunosDataGrid.Items.Refresh();
            }
        }

        public void btnRemoverAluno_Click(object sender, RoutedEventArgs e)
        {
            // Verifica se existem alunos selecionados via checkbox (IsSelected = true)
            var alunosSelecionados = ListaAlunos.Where(a => a.IsSelected).ToList();

            // Se não houver alunos selecionados via checkbox, verifica se há um aluno selecionado no DataGrid
            if (alunosSelecionados.Count == 0)
            {
                var selectedAluno = AlunosDataGrid.SelectedItem as Aluno;
                if (selectedAluno != null)
                {
                    alunosSelecionados.Add(selectedAluno);
                }
            }

            // Se não houver alunos para remover, exibe mensagem
            if (alunosSelecionados.Count == 0)
            {
                MessageBox.Show("Por favor, selecione pelo menos um aluno para remover.",
                    "Nenhum aluno selecionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirma a remoção
            var mensagem = alunosSelecionados.Count == 1
                ? $"Tem certeza que deseja remover o aluno {alunosSelecionados[0].Nome} (Nº {alunosSelecionados[0].Numero})?"
                : $"Tem certeza que deseja remover {alunosSelecionados.Count} alunos selecionados?";

            var result = MessageBox.Show(
                mensagem,
                "Confirmar Remoção",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int removidos = 0;

                // Remove cada aluno selecionado
                foreach (var aluno in alunosSelecionados)
                {
                    // Se o aluno estava em um grupo, remova-o primeiro
                    if (aluno.Grupo != null)
                    {
                        App.RemoveAlunoFromGrupo(aluno);
                    }

                    // Remove o aluno da coleção
                    ListaAlunos.Remove(aluno);
                    removidos++;
                }

                // Salva as mudanças usando a classe App
                App.SaveAlunos();

                // Exibe mensagem de confirmação
                MessageBox.Show($"{removidos} aluno(s) removido(s) com sucesso.",
                    "Remoção Concluída", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void btnImportarLista_Click(object sender, RoutedEventArgs e)
        {
            // Create file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx",
                Title = "Selecione o arquivo com a lista de alunos"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Check file extension
                    string extension = System.IO.Path.GetExtension(openFileDialog.FileName).ToLower();

                    if (extension == ".csv")
                    {
                        ImportarCSV(openFileDialog.FileName);
                    }
                    else if (extension == ".xlsx")
                    {
                        ImportarXLSX(openFileDialog.FileName);
                    }

                    // Save changes using the App class
                    App.SaveAlunos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao importar o arquivo: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Implementação do método para selecionar todos os alunos
        private void chkSelecionarTodos_Click(object sender, RoutedEventArgs e)
        {
            // Verifica se o checkbox está marcado
            bool isChecked = (chkSelecionarTodos.IsChecked == true);

            // Aplica o valor a todos os alunos na lista
            foreach (var aluno in ListaAlunos)
            {
                aluno.IsSelected = isChecked;
            }

            // Atualiza a visualização do DataGrid
            AlunosDataGrid.Items.Refresh();
        }

        private void ImportarCSV(string filePath)
        {
            // Converter o arquivo de ISO-8859-1 (Latin-1) para UTF-8
            StringBuilder sb = new StringBuilder();
            string[] lines;

            // Primeiro ler o arquivo com codificação ISO-8859-1, que é comum em arquivos CSV exportados
            // de sistemas que usam caracteres europeus (acentos, cedilha, etc.)
            using (StreamReader reader = new StreamReader(filePath, Encoding.GetEncoding("iso-8859-1")))
            {
                sb.Append(reader.ReadToEnd());
            }

            // Agora processar o conteúdo convertido para UTF-8
            lines = sb.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            int alunosAdicionados = 0;
            int alunosExistentes = 0;

            foreach (var line in lines)
            {
                try
                {
                    // Ignorar linhas vazias
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Método mais simples e robusto para dividir CSV
                    string[] parts;
                    
                    // Usar regex para lidar com CSV corretamente, incluindo campos entre aspas
                    if (line.Contains("\""))
                    {
                        // Padrão regex que respeita campos entre aspas
                        var csv = new List<string>();
                        var regex = new Regex("(?:^|,)(\"(?:[^\"])*\"|[^,]*)", RegexOptions.Compiled);
                        var matches = regex.Matches(line);
                        
                        foreach (Match match in matches)
                        {
                            string value = match.Value;
                            if (value.StartsWith(","))
                                value = value.Substring(1);
                            if (value.StartsWith("\"") && value.EndsWith("\""))
                                value = value.Substring(1, value.Length - 2);
                                
                            csv.Add(value);
                        }
                        
                        parts = csv.ToArray();
                    }
                    else
                    {
                        parts = line.Split(',');
                    }

                    if (parts.Length >= 3)
                    {
                        string nome = parts[0].Trim();
                        string numeroStr = parts[1].Trim();
                        string email = parts[2].Trim();

                        // Validar os dados
                        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(numeroStr) || string.IsNullOrWhiteSpace(email))
                            continue;

                        // Tentar converter o número
                        if (!int.TryParse(numeroStr, out int numero))
                            continue;

                        // Verificar se o aluno já existe
                        if (!ListaAlunos.Any(a => a.Numero == numero))
                        {
                            // Criar novo aluno
                            var aluno = new Aluno
                            {
                                // Garantir que os caracteres especiais sejam preservados corretamente
                                // usando normalização e escapando possíveis sequências de escape
                                Nome = HttpUtility.HtmlDecode(nome).Normalize(NormalizationForm.FormC),
                                Numero = numero,
                                Email = email,
                                GrupoId = null, // Sem grupo atribuído por padrão
                                IsSelected = false // Inicializa como não selecionado
                            };

                            ListaAlunos.Add(aluno);
                            alunosAdicionados++;
                        }
                        else
                        {
                            alunosExistentes++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erro ao processar linha: {ex.Message}");
                }
            }

            // Salvar os alunos importados
            App.SaveAlunos();

            MessageBox.Show($"Importação concluída!\nAlunos adicionados: {alunosAdicionados}\nAlunos já existentes: {alunosExistentes}",
                "Importação Concluída", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImportarXLSX(string filePath)
        {
            // Adiciona o using correto para Excel interop
            // using Excel = Microsoft.Office.Interop.Excel;
            dynamic excelApp = null;
            dynamic workbook = null;
            dynamic worksheet = null;

            try
            {
                excelApp = Activator.CreateInstance(Type.GetTypeFromProgID("Excel.Application"));
                workbook = excelApp.Workbooks.Open(filePath);
                worksheet = workbook.Sheets[1];

                dynamic usedRange = worksheet.UsedRange;
                int rowCount = usedRange.Rows.Count;

                int alunosAdicionados = 0;
                int alunosExistentes = 0;

                for (int row = 1; row <= rowCount; row++)
                {
                    try
                    {
                        string nome = Convert.ToString(worksheet.Cells[row, 1].Value)?.Trim() ?? string.Empty;
                        string numeroStr = Convert.ToString(worksheet.Cells[row, 2].Value)?.Trim() ?? string.Empty;
                        string email = Convert.ToString(worksheet.Cells[row, 3].Value)?.Trim() ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(numeroStr) || string.IsNullOrWhiteSpace(email))
                            continue;

                        if (!int.TryParse(numeroStr, out int numero))
                            continue;

                        if (!ListaAlunos.Any(a => a.Numero == numero))
                        {
                            var aluno = new Aluno
                            {
                                // Adicionar a normalização e decode para nomes do Excel também
                                Nome = HttpUtility.HtmlDecode(nome).Normalize(NormalizationForm.FormC),
                                Numero = numero,
                                Email = email,
                                GrupoId = null, // Sem grupo atribuído por padrão
                                IsSelected = false // Inicializa como não selecionado
                            };

                            ListaAlunos.Add(aluno);
                            alunosAdicionados++;
                        }
                        else
                        {
                            alunosExistentes++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Erro ao processar linha {row}: {ex.Message}");
                    }
                }

                MessageBox.Show($"Importação concluída!\nAlunos adicionados: {alunosAdicionados}\nAlunos já existentes: {alunosExistentes}",
                    "Importação Concluída", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao processar arquivo Excel: {ex.Message}",
                    "Erro na Importação", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (workbook != null)
                {
                    workbook.Close(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                }

                if (worksheet != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                }

                if (excelApp != null)
                {
                    excelApp.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                }
            }
        }

        #endregion
    }

    // Dialog for adding/editing students
    public class AlunoEditDialog : Window
    {
        public Aluno AlunoEditado { get; private set; }

        // Controls for the form
        private TextBox txtNumero;
        private TextBox txtNome;
        private TextBox txtEmail;
        private Button btnSalvar;
        private Button btnCancelar;

        public AlunoEditDialog(Aluno alunoExistente = null)
        {
            // Set up the window with modern styling
            Title = alunoExistente == null ? "Adicionar Aluno" : "Editar Aluno";
            Width = 450;
            Height = 320;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            SetResourceReference(BackgroundProperty, "AppCard");
            ResizeMode = ResizeMode.NoResize;

            // Create the layout
            var grid = new Grid();
            grid.Margin = new Thickness(30);

            // Define rows with more spacing
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            // Define columns
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Style for Labels
            Style labelStyle = new Style(typeof(Label));
            labelStyle.Setters.Add(new Setter(Label.ForegroundProperty, DynamicResourceExtension("AppText")));
            labelStyle.Setters.Add(new Setter(Label.FontSizeProperty, 14.0));
            labelStyle.Setters.Add(new Setter(Label.VerticalAlignmentProperty, VerticalAlignment.Center));

            // Style for TextBoxes
            Style textBoxStyle = new Style(typeof(TextBox));
            textBoxStyle.Setters.Add(new Setter(TextBox.BackgroundProperty, DynamicResourceExtension("AppInput")));
            textBoxStyle.Setters.Add(new Setter(TextBox.ForegroundProperty, DynamicResourceExtension("AppText")));
            textBoxStyle.Setters.Add(new Setter(TextBox.BorderBrushProperty, DynamicResourceExtension("AppInputBorder")));
            textBoxStyle.Setters.Add(new Setter(TextBox.BorderThicknessProperty, new Thickness(1)));
            textBoxStyle.Setters.Add(new Setter(TextBox.PaddingProperty, new Thickness(8)));
            textBoxStyle.Setters.Add(new Setter(TextBox.FontSizeProperty, 14.0));
            textBoxStyle.Setters.Add(new Setter(TextBox.HeightProperty, 36.0));

            // Create form elements with modern styling
            var lblNumero = new Label { Content = "Número:", Style = labelStyle };
            Grid.SetRow(lblNumero, 0);
            Grid.SetColumn(lblNumero, 0);
            grid.Children.Add(lblNumero);

            txtNumero = new TextBox { Style = textBoxStyle, Margin = new Thickness(0, 7, 0, 7) };
            Grid.SetRow(txtNumero, 0);
            Grid.SetColumn(txtNumero, 1);
            grid.Children.Add(txtNumero);

            var lblNome = new Label { Content = "Nome:", Style = labelStyle };
            Grid.SetRow(lblNome, 1);
            Grid.SetColumn(lblNome, 0);
            grid.Children.Add(lblNome);

            txtNome = new TextBox { Style = textBoxStyle, Margin = new Thickness(0, 7, 0, 7) };
            Grid.SetRow(txtNome, 1);
            Grid.SetColumn(txtNome, 1);
            grid.Children.Add(txtNome);

            var lblEmail = new Label { Content = "Email:", Style = labelStyle };
            Grid.SetRow(lblEmail, 2);
            Grid.SetColumn(lblEmail, 0);
            grid.Children.Add(lblEmail);

            txtEmail = new TextBox { Style = textBoxStyle, Margin = new Thickness(0, 7, 0, 7) };
            Grid.SetRow(txtEmail, 2);
            Grid.SetColumn(txtEmail, 1);
            grid.Children.Add(txtEmail);

            // Button styles
            // Helper method to create DynamicResourceExtension
            DynamicResourceExtension DynamicResourceExtension(string resourceKey)
            {
                return new DynamicResourceExtension(resourceKey);
            }

            Style buttonBaseStyle = new Style(typeof(Button));
            buttonBaseStyle.Setters.Add(new Setter(Button.FontSizeProperty, 13.0));
            buttonBaseStyle.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.SemiBold));
            buttonBaseStyle.Setters.Add(new Setter(Button.HeightProperty, 36.0));
            buttonBaseStyle.Setters.Add(new Setter(Button.WidthProperty, 100.0));
            buttonBaseStyle.Setters.Add(new Setter(Button.MarginProperty, new Thickness(5)));

            // Buttons panel
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            btnSalvar = new Button
            {
                Content = "Salvar",
                Style = buttonBaseStyle,
                BorderThickness = new Thickness(0)
            };
            btnSalvar.SetResourceReference(Button.BackgroundProperty, "AppAccent");
            btnSalvar.SetResourceReference(Button.ForegroundProperty, "AppTextOnPrimary");
            btnSalvar.Click += BtnSalvar_Click;
            stackPanel.Children.Add(btnSalvar);

            btnCancelar = new Button
            {
                Content = "Cancelar",
                Style = buttonBaseStyle,
                BorderThickness = new Thickness(0)
            };
            btnCancelar.SetResourceReference(Button.BackgroundProperty, "AppBorder");
            btnCancelar.SetResourceReference(Button.ForegroundProperty, "AppText");
            btnCancelar.Click += BtnCancelar_Click;
            stackPanel.Children.Add(btnCancelar);

            Grid.SetRow(stackPanel, 4);
            Grid.SetColumn(stackPanel, 0);
            Grid.SetColumnSpan(stackPanel, 2);
            grid.Children.Add(stackPanel);

            // Set the content
            Content = grid;

            // If editing an existing student, fill the form
            if (alunoExistente != null)
            {
                txtNumero.Text = alunoExistente.Numero.ToString();
                txtNome.Text = alunoExistente.Nome;
                txtEmail.Text = alunoExistente.Email;

                // Disable the number field as it shouldn't be edited
                txtNumero.IsEnabled = false;

                // Create a copy of the existing student for editing
                AlunoEditado = new Aluno
                {
                    Numero = alunoExistente.Numero,
                    Nome = alunoExistente.Nome,
                    Email = alunoExistente.Email,
                    GrupoId = alunoExistente.GrupoId,
                    IsSelected = alunoExistente.IsSelected
                };
            }
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtNumero.Text) ||
                string.IsNullOrWhiteSpace(txtNome.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Todos os campos são obrigatórios.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create or update student object with the input values
            if (AlunoEditado == null)
            {
                AlunoEditado = new Aluno();
            }

            AlunoEditado.Numero = int.Parse(txtNumero.Text);
            // Normalizar o nome para garantir que caracteres especiais sejam exibidos corretamente
            AlunoEditado.Nome = HttpUtility.HtmlDecode(txtNome.Text).Normalize(NormalizationForm.FormC);
            AlunoEditado.Email = txtEmail.Text;

            // Set DialogResult to true to indicate success
            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Set DialogResult to false to indicate cancellation
            DialogResult = false;
            Close();
        }
    }
}