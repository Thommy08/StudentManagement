using System;
using System.Xml.Serialization;

namespace labmockups.MODELS
{
    [Serializable]
    public class Aluno
    {
        public int Numero { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        [XmlIgnore]
        public Grupo Grupo { get; set; }
        public int? GrupoId { get; set; }
        public bool IsSelected { get; set; }

        public Aluno()
        {
        }
        public Aluno(int numero, string nome, string email)
        {
            Numero = numero;
            Nome = nome;
            Email = email;
        }
        public override string ToString()
        {
            return $"{Numero} - {Nome}";
        }
    }
}