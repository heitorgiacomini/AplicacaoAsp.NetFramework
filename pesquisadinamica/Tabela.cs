using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidade.Comum
{
    public class Tabela
    {
        public Tabela()
        {
            this.Coluna = new List<Coluna>();
        }
        public String SchemaTabela{ get; set; }
        public String NomeTabela { get; set; }
        public String ApelidoTabela { get; set; }
        public List<Coluna> Coluna { get; set; }
        public List<Dictionary<string, string>> ListaDicionarioSelect{ get; set; }
       
    }
}
