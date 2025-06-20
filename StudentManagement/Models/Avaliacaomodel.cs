using System;
using System.Collections.Generic;

namespace labmockups.MODELS
{
    public class Avaliacao
    {
        public int Id { get; set; }

        public int TarefaId { get; set; }
        public Tarefa Tarefa { get; set; }

        public int AlunoId { get; set; }
        public Aluno Aluno { get; set; }

        public double Valor { get; set; }
    }

}
