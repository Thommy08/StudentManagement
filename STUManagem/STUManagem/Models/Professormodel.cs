using System;
using System.Xml.Serialization;

namespace labmockups.MODELS
{
    [Serializable]
    public class Professor
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Foto { get; set; }
        public DateTime LastModified { get; set; }

        public Professor()
        {
            Nome = "";
            Email = "";
            Foto = "";
            LastModified = DateTime.Now;
        }
    }
}