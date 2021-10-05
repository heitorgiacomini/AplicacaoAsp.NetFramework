using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelo
{
    public class Compra
    { //tipo int fica 0 quando nao inicializado
        public int Codigo { get; set; }
        public string NomeCliente{ get; set; }
        public int CodCliente { get; set; }
        public DateTime DataCompra { get; set; }
        public decimal ValorTotal{ get; set; }
        public bool Excluido { get; set; }
        public DateTime DataExclusao{ get; set; }
        public List<Modelo.ItemCompra> AuxItems { get; set; }
    }
}
