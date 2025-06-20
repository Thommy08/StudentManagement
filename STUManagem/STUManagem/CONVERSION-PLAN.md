# Plan for Converting Windows to UserControls

This document outlines the plan for converting all remaining Window-based views to UserControls in the application, to enable single-window navigation with a ContentControl.

## Completed Conversions

- [x] MainWindow.xaml - Updated with ContentControl
- [x] Alunos.xaml - Converted to UserControl
- [x] AtribuirNotas.xaml - Converted to UserControl
- [x] GerirAlunosGrupo.xaml - Converted to UserControl

## Remaining Views to Convert

1. **Grupos.xaml** - Grupo management
2. **Histograma.xaml** - Grade distribution visualization 
3. **Login.xaml** - User authentication
4. **Pauta.xaml** - Grade management
5. **Perfil.xaml** - User profile management
6. **Tarefas.xaml** - Task management

## Special Considerations

### Login.xaml
- The Login view is used as a dialog in MainWindow.xaml.cs
- It should be handled differently since it's displayed as a modal dialog
- A wrapper Window should be created to host the Login UserControl

### Histograma.xaml
- Contains visualization components that might need special handling
- Ensure all chart/graph components continue to function properly

### Pauta.xaml
- Contains complex data binding for grades
- May have dependencies on Window lifecycle events

## Implementation Guide

### Step 1: For Each XAML File

1. Change the root element from `<Window>` to `<UserControl>`
2. Remove Window-specific properties:
   - Title
   - Height, Width
   - WindowStartupLocation
   - ResizeMode
   - Any other Window-specific attributes
3. Change `<Window.Resources>` to `<UserControl.Resources>`
4. Change closing tag from `</Window>` to `</UserControl>`

### Step 2: For Each Code-Behind File

1. Change the base class from `Window` to `UserControl`:
   ```csharp
   public partial class ViewName : UserControl
   ```

2. Add the GetMainWindow helper method:
   ```csharp
   private MainWindow GetMainWindow()
   {
       return Window.GetWindow(this) as MainWindow;
   }
   ```

3. Update navigation methods:
   - Replace creating new Windows with calls to MainWindow navigation methods
   - Example:
   ```csharp
   // Old code
   private void Dashboard_Click(object sender, RoutedEventArgs e)
   {
       MainWindow mainWindow = new MainWindow();
       mainWindow.Show();
       this.Close();
   }
   
   // New code
   private void Dashboard_Click(object sender, RoutedEventArgs e)
   {
       var mainWindow = GetMainWindow();
       if (mainWindow != null)
       {
           mainWindow.Dashboard_Click(sender, e);
       }
   }
   ```

4. Replace any `Close()` calls with appropriate navigation:
   ```csharp
   // Old code
   this.Close();
   
   // New code
   var mainWindow = GetMainWindow();
   if (mainWindow != null)
   {
       mainWindow.SomeNavigationMethod(sender, e);
   }
   ```

5. Update dialog handling to use `Window.GetWindow(this)` for ownership:
   ```csharp
   // Old code
   var dialog = new SomeDialog();
   dialog.Owner = this;
   
   // New code
   var dialog = new SomeDialog();
   dialog.Owner = Window.GetWindow(this);
   ```

### Step 3: Special Handling for Login

In MainWindow.xaml.cs, update the ShowLoginDialog method:

```csharp
private bool ShowLoginDialog()
{
    // Create the Login UserControl
    Login loginControl = new Login();
    
    // Create a new window to host the UserControl
    Window loginDialog = new Window
    {
        Title = "Login",
        Content = loginControl,
        SizeToContent = SizeToContent.WidthAndHeight,
        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        Owner = this,
        ResizeMode = ResizeMode.NoResize
    };
    
    // Show the dialog
    bool? loginResult = loginDialog.ShowDialog();
    
    // Process result...
    // Return appropriate value
}
```

## Testing Recommendations

1. **Functionality Testing**:
   - Test each view individually to ensure all functionality works correctly
   - Test navigation between views using the menu options
   - Test data binding and data updates

2. **Visual Testing**:
   - Verify that the layout of each view appears correct
   - Check for any visual glitches or alignment issues
   - Ensure responsive behavior is maintained

3. **Edge Cases**:
   - Test error handling
   - Test with different data scenarios
   - Verify correct behavior when switching between views rapidly

4. **Dialog Testing**:
   - Test modal dialog functionality
   - Verify that dialog ownership is correct
   - Check dialog results are properly processed

## Implementation Order

1. Start with simpler views (those with fewer dependencies)
2. Handle views with dialogs last, especially the Login view
3. Follow this order:
   - Grupos.xaml
   - Tarefas.xaml
   - Perfil.xaml
   - Pauta.xaml
   - Histograma.xaml
   - Login.xaml (special handling)

## Additional Notes

- Update the constructor of each UserControl to handle any initialization that previously depended on Window events
- Remove any window-specific event handlers that are no longer applicable
- Ensure that message boxes and dialogs display correctly from within UserControls
- Update any references to window-specific properties like Width, Height with UserControl equivalents

By following this plan, all views will be converted to UserControls, enabling a single-window navigation model with ContentControl.

