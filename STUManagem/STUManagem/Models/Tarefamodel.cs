using System;
using System.Collections.Generic;

namespace labmockups.MODELS
{
    public class Tarefa
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataEntrega { get; set; }
        public float Peso { get; set; }
    }
}

