using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelo
{
    public class ItemCompra
    {
        public int Codigo { get; set; }
        public int codProduto { get; set; }
        public int codCompra { get; set; }
        public long Quantidade { get; set; }
        public bool Excluido { get; set; }
        public decimal subTotal { get; set; }
        public DateTime DataExclusao { get; set; }
        public string auxDescricao { get; set; }
        public decimal auxValor { get; set; }
        public decimal auxSubTotal { get { return Quantidade * auxValor; } }
        public string guid { get; set; }
    }
}
