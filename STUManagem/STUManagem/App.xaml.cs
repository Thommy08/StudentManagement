using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using System.Text;
using labmockups.MODELS;

namespace trabalhoLAB
{
    public partial class App : Application
    {
        private static Professor _professorAtual;
        private static ObservableCollection<Aluno> _listaAlunos;
        private static ObservableCollection<Grupo> _listaGrupos;
        private static ObservableCollection<Tarefa> _listaTarefas;
        private static bool _isLoggedIn = false;
        private static bool _isDarkTheme = false;

        private static readonly string _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "trabalhoLAB");
        private static readonly string _alunosFilePath;
        private static readonly string _gruposFilePath;
        private static readonly string _tarefasFilePath;
        private static readonly string _professorFilePath;

        static App()
        {
            if (!Directory.Exists(_appDataPath))
                Directory.CreateDirectory(_appDataPath);

            _alunosFilePath = Path.Combine(_appDataPath, "alunos.xml");
            _gruposFilePath = Path.Combine(_appDataPath, "grupos.xml");
            _tarefasFilePath = Path.Combine(_appDataPath, "tarefas.xml");
            _professorFilePath = Path.Combine(_appDataPath, "professor.xml");

            _listaAlunos = new ObservableCollection<Aluno>();
            _listaGrupos = new ObservableCollection<Grupo>();
            _listaTarefas = new ObservableCollection<Tarefa>();
        }

        public static bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => _isLoggedIn = value;
        }

        public static bool IsDarkTheme
        {
            get => _isDarkTheme;
            set => _isDarkTheme = value;
        }

        public static Professor ProfessorAtual
        {
            get
            {
                if (_professorAtual == null)
                {
                    LoadProfessor();
                }
                return _professorAtual;
            }
            set => _professorAtual = value;
        }

        public static ObservableCollection<Aluno> ListaAlunos
        {
            get
            {
                if (_listaAlunos.Count == 0)
                    LoadAlunos();
                return _listaAlunos;
            }
        }

        public static ObservableCollection<Grupo> ListaGrupos
        {
            get
            {
                if (_listaGrupos.Count == 0)
                    LoadGrupos();
                return _listaGrupos;
            }
        }

        public static ObservableCollection<Tarefa> ListaTarefas
        {
            get
            {
                if (_listaTarefas.Count == 0)
                    LoadTarefas();
                return _listaTarefas;
            }
        }

        public static void SalvarDadosProfessor()
        {
            try
            {
                if (!Directory.Exists(_appDataPath))
                {
                    Directory.CreateDirectory(_appDataPath);
                }

                ProfessorAtual.LastModified = DateTime.Now;

                var settings = new System.Xml.XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    Indent = true,
                    OmitXmlDeclaration = false
                };

                using (var writer = System.Xml.XmlWriter.Create(_professorFilePath, settings))
                {
                    var serializer = new XmlSerializer(typeof(Professor));
                    serializer.Serialize(writer, ProfessorAtual);
                }

                IsLoggedIn = !string.IsNullOrEmpty(ProfessorAtual.Email);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar dados do professor: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void LoadProfessor()
        {
            try
            {
                if (File.Exists(_professorFilePath))
                {
                    using (var reader = new StreamReader(_professorFilePath, Encoding.UTF8))
                    {
                        var content = reader.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            using (var stringReader = new StringReader(content))
                            {
                                var serializer = new XmlSerializer(typeof(Professor));
                                _professorAtual = (Professor)serializer.Deserialize(stringReader);
                            }
                        }
                        else
                        {
                            _professorAtual = CreateDefaultProfessor();
                        }
                    }
                }
                else
                {
                    _professorAtual = CreateDefaultProfessor();
                    SalvarDadosProfessor();
                }
                IsLoggedIn = !string.IsNullOrEmpty(_professorAtual.Email);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar professor: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                _professorAtual = CreateDefaultProfessor();
            }
        }

        private static Professor CreateDefaultProfessor()
        {
            return new Professor
            {
                Nome = "Usuário Exemplo",
                Email = "usuario@exemplo.com",
                Foto = "",
                LastModified = DateTime.Now
            };
        }

        public static void LoadAlunos()
        {
            try
            {
                if (File.Exists(_alunosFilePath))
                {
                    using (var reader = new StreamReader(_alunosFilePath, Encoding.UTF8))
                    {
                        var content = reader.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            using (var stringReader = new StringReader(content))
                            {
                                var serializer = new XmlSerializer(typeof(List<Aluno>));
                                var alunos = (List<Aluno>)serializer.Deserialize(stringReader);
                                _listaAlunos.Clear();
                                foreach (var aluno in alunos)
                                    _listaAlunos.Add(aluno);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar alunos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                _listaAlunos.Clear();
            }
        }

        public static void SaveAlunos()
        {
            try
            {
                if (!Directory.Exists(_appDataPath))
                {
                    Directory.CreateDirectory(_appDataPath);
                }

                var settings = new System.Xml.XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    Indent = true,
                    OmitXmlDeclaration = false
                };

                using (var writer = System.Xml.XmlWriter.Create(_alunosFilePath, settings))
                {
                    var serializer = new XmlSerializer(typeof(List<Aluno>));
                    serializer.Serialize(writer, _listaAlunos.ToList());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar alunos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void LoadGrupos()
        {
            try
            {
                if (File.Exists(_gruposFilePath))
                {
                    using (var reader = new StreamReader(_gruposFilePath, Encoding.UTF8))
                    {
                        var content = reader.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            using (var stringReader = new StringReader(content))
                            {
                                var serializer = new XmlSerializer(typeof(List<Grupo>));
                                var grupos = (List<Grupo>)serializer.Deserialize(stringReader);
                                _listaGrupos.Clear();
                                foreach (var grupo in grupos)
                                    _listaGrupos.Add(grupo);
                            }
                        }
                    }
                    LinkStudentsWithGroups();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar grupos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                _listaGrupos.Clear();
            }
        }

        public static void SaveGrupos()
        {
            try
            {
                if (!Directory.Exists(_appDataPath))
                {
                    Directory.CreateDirectory(_appDataPath);
                }

                var settings = new System.Xml.XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    Indent = true,
                    OmitXmlDeclaration = false
                };

                using (var writer = System.Xml.XmlWriter.Create(_gruposFilePath, settings))
                {
                    var serializer = new XmlSerializer(typeof(List<Grupo>));
                    serializer.Serialize(writer, _listaGrupos.ToList());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar grupos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void LoadTarefas()
        {
            try
            {
                if (File.Exists(_tarefasFilePath))
                {
                    using (var reader = new StreamReader(_tarefasFilePath, Encoding.UTF8))
                    {
                        var content = reader.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            using (var stringReader = new StringReader(content))
                            {
                                var serializer = new XmlSerializer(typeof(List<Tarefa>));
                                var tarefas = (List<Tarefa>)serializer.Deserialize(stringReader);
                                _listaTarefas.Clear();
                                foreach (var tarefa in tarefas)
                                    _listaTarefas.Add(tarefa);
                            }
                        }
                    }
                }
                // Removed default task creation - only load existing tasks
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar tarefas: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                _listaTarefas.Clear();
            }
        }

        public static void SaveTarefas()
        {
            try
            {
                if (!Directory.Exists(_appDataPath))
                {
                    Directory.CreateDirectory(_appDataPath);
                }

                var settings = new System.Xml.XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    Indent = true,
                    OmitXmlDeclaration = false
                };

                using (var writer = System.Xml.XmlWriter.Create(_tarefasFilePath, settings))
                {
                    var serializer = new XmlSerializer(typeof(List<Tarefa>));
                    serializer.Serialize(writer, _listaTarefas.ToList());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar tarefas: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void LinkStudentsWithGroups()
        {
            foreach (var aluno in _listaAlunos)
            {
                if (aluno.GrupoId.HasValue)
                {
                    var grupo = _listaGrupos.FirstOrDefault(g => g.Id == aluno.GrupoId.Value);
                    if (grupo != null)
                    {
                        aluno.Grupo = grupo;
                        if (!grupo.Alunos.Contains(aluno))
                            grupo.Alunos.Add(aluno);
                    }
                }
            }

            foreach (var grupo in _listaGrupos)
                grupo.NumeroAlunos = grupo.Alunos.Count;
        }

        public static void AddAlunoToGrupo(Aluno aluno, Grupo grupo)
        {
            if (aluno.GrupoId.HasValue && aluno.GrupoId.Value != grupo.Id)
            {
                var oldGrupo = _listaGrupos.FirstOrDefault(g => g.Id == aluno.GrupoId.Value);
                if (oldGrupo != null)
                {
                    oldGrupo.Alunos.Remove(aluno);
                    oldGrupo.NumeroAlunos = oldGrupo.Alunos.Count;
                }
            }

            aluno.GrupoId = grupo.Id;
            aluno.Grupo = grupo;

            if (!grupo.Alunos.Contains(aluno))
                grupo.Alunos.Add(aluno);
            grupo.NumeroAlunos = grupo.Alunos.Count;

            SaveAlunos();
            SaveGrupos();
        }

        public static void RemoveAlunoFromGrupo(Aluno aluno)
        {
            if (aluno.GrupoId.HasValue)
            {
                var grupo = _listaGrupos.FirstOrDefault(g => g.Id == aluno.GrupoId.Value);
                grupo?.Alunos.Remove(aluno);
                if (grupo != null)
                    grupo.NumeroAlunos = grupo.Alunos.Count;

                aluno.GrupoId = null;
                aluno.Grupo = null;

                SaveAlunos();
                SaveGrupos();
            }
        }

        public static void LimparDadosSessao()
        {
            _professorAtual = new Professor();
            _listaAlunos.Clear();
            _listaGrupos.Clear();
            _listaTarefas.Clear();
            IsLoggedIn = false;

            try
            {
                // Delete all data files
                string[] filesToDelete = {
                    _professorFilePath,
                    _alunosFilePath,
                    _gruposFilePath,
                    _tarefasFilePath,
                    Path.Combine(_appDataPath, "avaliacoes.xml") // Add grades file
                };

                foreach (string file in filesToDelete)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
                // Ignorar falhas ao apagar
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoadProfessor();
            LoadAlunos();
            LoadGrupos();
            LoadTarefas();
            LinkStudentsWithGroups();
            LoadThemeSettings();
        }

        private void LoadThemeSettings()
        {
            try
            {
                string settingsPath = Path.Combine(_appDataPath, "theme_settings.txt");
                if (File.Exists(settingsPath))
                {
                    string theme = File.ReadAllText(settingsPath).Trim();
                    _isDarkTheme = theme.Equals("dark", StringComparison.OrdinalIgnoreCase);
                }
                
                // Apply theme resources
                ApplyTheme();
            }
            catch (Exception ex)
            {
                // If there's an error, default to light theme
                _isDarkTheme = false;
                System.Diagnostics.Debug.WriteLine($"Error loading theme settings: {ex.Message}");
            }
        }
        
        private void SaveThemeSettings()
        {
            try
            {
                string settingsPath = Path.Combine(_appDataPath, "theme_settings.txt");
                File.WriteAllText(settingsPath, _isDarkTheme ? "dark" : "light");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving theme settings: {ex.Message}");
            }
        }
        
        public static void ToggleTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            ApplyTheme();
            
            // Save the theme preference
            ((App)Current).SaveThemeSettings();
        }
        
        private static void ApplyTheme()
        {
            var resources = Application.Current.Resources;
            
            // Update all dynamic theme resources based on current theme
            if (_isDarkTheme)
            {
                // Apply dark theme resources
                resources["AppBackground"] = resources["DarkThemeBackground"];
                resources["AppText"] = resources["DarkThemeText"];
                resources["AppTextSecondary"] = resources["DarkThemeTextSecondary"];
                resources["AppSidebar"] = resources["DarkThemeSidebar"];
                resources["AppAccent"] = resources["DarkThemeAccent"];
                resources["AppCard"] = resources["DarkThemeCard"];
                resources["AppBorder"] = resources["DarkThemeBorder"];
                resources["AppInput"] = resources["DarkThemeInput"];
                resources["AppInputBorder"] = resources["DarkThemeInputBorder"];
                resources["AppAltRow"] = resources["DarkThemeAltRow"];
                resources["AppSuccess"] = resources["DarkThemeSuccess"];
                resources["AppWarning"] = resources["DarkThemeWarning"];
                resources["AppError"] = resources["DarkThemeError"];
                resources["AppHover"] = resources["DarkThemeHover"];
            }
            else
            {
                // Apply light theme resources
                resources["AppBackground"] = resources["LightThemeBackground"];
                resources["AppText"] = resources["LightThemeText"];
                resources["AppTextSecondary"] = resources["LightThemeTextSecondary"];
                resources["AppSidebar"] = resources["LightThemeSidebar"];
                resources["AppAccent"] = resources["LightThemeAccent"];
                resources["AppCard"] = resources["LightThemeCard"];
                resources["AppBorder"] = resources["LightThemeBorder"];
                resources["AppInput"] = resources["LightThemeInput"];
                resources["AppInputBorder"] = resources["LightThemeInputBorder"];
                resources["AppAltRow"] = resources["LightThemeAltRow"];
                resources["AppSuccess"] = resources["LightThemeSuccess"];
                resources["AppWarning"] = resources["LightThemeWarning"];
                resources["AppError"] = resources["LightThemeError"];
                resources["AppHover"] = resources["LightThemeHover"];
            }
        }
    }
}
