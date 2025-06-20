using System;
using System.Collections.Generic;

namespace labmockups.MODELS
{
    public class Grupo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public List<Aluno> Alunos { get; set; } = new List<Aluno>();

        public int NumeroAlunos { get; set; }

    }

}
