using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidade.Comum
{
    public class LinhaRegistro
    {
        public LinhaRegistro()
        {
            this.Registro = new List<ChaveValor>();
        }
        public String Identificador { get; set; }
        public List<ChaveValor> Registro{ get; set; }

    }
}
