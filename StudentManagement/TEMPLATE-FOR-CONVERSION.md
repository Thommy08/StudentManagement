# Template for Converting Windows to UserControls

Follow these steps for each view file that needs to be converted from a Window to a UserControl.

## 1. XAML File Changes

### Step 1: Change the root element
Change:
```xml
<Window x:Class="trabalhoLAB.ViewName"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:trabalhoLAB"
        mc:Ignorable="d"
        Title="Sistema de Gestão de Avaliações" Height="800" Width="1500"
        WindowStartupLocation="CenterScreen"
        Background="#F5F5F5">
```

To:
```xml
<UserControl x:Class="trabalhoLAB.ViewName"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:trabalhoLAB"
        mc:Ignorable="d"
        Background="#F5F5F5">
```

### Step 2: Change the closing tag
Change:
```xml
</Window>
```

To:
```xml
</UserControl>
```

### Step 3: Update any Window resources
Change:
```xml
<Window.Resources>
    <!-- resources -->
</Window.Resources>
```

To:
```xml
<UserControl.Resources>
    <!-- resources -->
</UserControl.Resources>
```

## 2. Code-Behind File Changes

### Step 1: Add System.Windows.Input namespace if not present
```csharp
using System.Windows.Input;
```

### Step 2: Change the base class
Change:
```csharp
public partial class ViewName : Window
```

To:
```csharp
public partial class ViewName : UserControl
```

### Step 3: Add GetMainWindow helper method
```csharp
// Obtém a MainWindow para navegar entre views
private MainWindow GetMainWindow()
{
    return Window.GetWindow(this) as MainWindow;
}
```

### Step 4: Update navigation methods
Change:
```csharp
private void Dashboard_Click(object sender, RoutedEventArgs e)
{
    MainWindow mainWindow = new MainWindow();
    mainWindow.Show();
    this.Close();
}
```

To:
```csharp
private void Dashboard_Click(object sender, RoutedEventArgs e)
{
    // Navega para o Dashboard
    var mainWindow = GetMainWindow();
    if (mainWindow != null)
    {
        mainWindow.Dashboard_Click(sender, e);
    }
}
```

### Step 5: Update dialog handling
Change:
```csharp
var dialog = new SomeDialog();
dialog.Owner = this;
```

To:
```csharp
var dialog = new SomeDialog();
dialog.Owner = Window.GetWindow(this);
```

### Step 6: Replace Close() calls with navigation
Change:
```csharp
this.Close();
```

To:
```csharp
// Remove this line or replace with appropriate navigation
// e.g., mainWindow.LoadView(new SomeView());
```

### Step 7: Update Sair_Click method
Change:
```csharp
private void Sair_Click(object sender, RoutedEventArgs e)
{
    // Ask for confirmation before exiting
    MessageBoxResult result = MessageBox.Show(
        "Deseja realmente sair da aplicação?",
        "Confirmar Saída",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);

    if (result == MessageBoxResult.Yes)
    {
        // Save changes
        App.SaveChanges();

        // Exit the application
        System.Windows.Application.Current.Shutdown();
    }
}
```

To:
```csharp
private void Sair_Click(object sender, RoutedEventArgs e)
{
    // Delegar para a MainWindow a ação de sair
    var mainWindow = GetMainWindow();
    if (mainWindow != null)
    {
        mainWindow.Sair_Click(sender, e);
    }
}
```

## Special Case: Login View

The Login view is special because it's used as a dialog in the MainWindow. For the Login view:

1. Convert it to a UserControl as described above
2. In MainWindow.xaml.cs, update the ShowLoginDialog method:

```csharp
private bool ShowLoginDialog()
{
    // Criar o UserControl de login
    Login loginControl = new Login();
    
    // Criar uma nova janela de diálogo para exibir o UserControl de login
    Window loginDialog = new Window
    {
        Title = "Login",
        Content = loginControl,
        SizeToContent = SizeToContent.WidthAndHeight,
        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        Owner = this,
        ResizeMode = ResizeMode.NoResize
    };
    
    // Mostrar a janela de diálogo
    bool? loginResult = loginDialog.ShowDialog();
    
    // Process result...
}
```

