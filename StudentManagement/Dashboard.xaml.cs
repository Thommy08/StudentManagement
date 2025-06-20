using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace trabalhoLAB
{
    /// <summary>
    /// Lógica interna para Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        public Dashboard()
        {
            InitializeComponent();

            // Preencher contadores
            TotalAlunosCounter.Text = App.ListaAlunos.Count.ToString();
            TotalTarefasCounter.Text = App.ListaTarefas.Count.ToString();
            TotalGruposCounter.Text = App.ListaGrupos.Count.ToString();

            // Preencher DataGrid de tarefas
            TarefasDataGrid.ItemsSource = App.ListaTarefas;
        }
    }
}
