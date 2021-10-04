using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelo
{    public class Produto
    {
        public int Codigo { get; set; }
        public DateTime DataCriacao { get; set; }
        public string Descricao { get; set; }
        public decimal ValorUnitario { get; set; }
        public bool Excluido { get; set; }
        public DateTime DataExcluido { get; set; }
    }
}
