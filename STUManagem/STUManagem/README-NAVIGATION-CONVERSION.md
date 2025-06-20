# Windows to UserControls Navigation Conversion

## Project Overview

This project involves converting the application from a multi-window navigation system to a single-window ContentControl-based navigation system. This architectural change improves user experience, simplifies navigation logic, and enables better state management across the application.

## Current Status

- ✅ MainWindow.xaml updated with ContentControl for hosting views
- ✅ LoadView navigation method implemented in MainWindow
- ✅ Alunos view converted to UserControl
- ✅ AtribuirNotas view converted to UserControl
- ✅ GerirAlunosGrupo view converted to UserControl
- ⏳ 6 views remain to be converted

## Reference Documents

Three key documents have been created to guide the conversion process:

1. **CONVERSION-PLAN.md**
   - Detailed step-by-step guide for converting each view
   - Special considerations for complex views
   - Testing recommendations and implementation order

2. **SUMMARY.md**
   - Overview of the architectural changes
   - Explanation of key components in the new navigation system
   - Potential challenges and benefits of the new architecture

3. **TEMPLATE-FOR-CONVERSION.md**
   - Template with code examples for converting views
   - Ready-to-use patterns for common conversion scenarios

## Quick Start Guide

To continue the conversion process:

1. Choose the next view to convert (recommended: Grupos.xaml)
2. Follow the steps in CONVERSION-PLAN.md:
   - Update the XAML file (change Window to UserControl)
   - Update the code-behind file (add GetMainWindow method, update navigation)
3. Test the converted view thoroughly
4. Update the checklist below

## Views Conversion Checklist

- [x] **MainWindow.xaml** - Updated with ContentControl
- [x] **Alunos.xaml** - Converted to UserControl
- [x] **AtribuirNotas.xaml** - Converted to UserControl
- [x] **GerirAlunosGrupo.xaml** - Converted to UserControl
- [ ] **Grupos.xaml** - To be converted
- [ ] **Tarefas.xaml** - To be converted
- [ ] **Perfil.xaml** - To be converted
- [ ] **Pauta.xaml** - To be converted
- [ ] **Histograma.xaml** - To be converted
- [ ] **Login.xaml** - To be converted (special case for dialog)

## Key Steps for Each Conversion

1. **XAML Changes**:
   - Change root element from `<Window>` to `<UserControl>`
   - Remove Window-specific properties
   - Update resources section

2. **Code-Behind Changes**:
   - Change base class from Window to UserControl
   - Add GetMainWindow helper method
   - Update navigation methods
   - Handle dialogs properly

3. **Testing**:
   - Verify UI appears correctly
   - Test all functionality
   - Ensure navigation works in both directions

## Special Considerations

- **Login View**: Requires special handling as a dialog
- **Histograma View**: Contains visualization components that need special attention
- **State Management**: Consider UserControl lifecycle differences from Windows

## Completion Criteria

The conversion project will be complete when:

1. All views have been converted to UserControls
2. Navigation works seamlessly through the ContentControl
3. All functionality works as expected in the new architecture
4. No references to the old window-based navigation remain

