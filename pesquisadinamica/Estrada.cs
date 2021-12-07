using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidade.Imobiliario
{
    public class Estrada : Geografico
    {
        public Estrada()
        {
            this.DadosEstrada = new RelacaoEstrada();
        }        

        public string Descricao { get; set; }
        public bool? StatusPavimentacao { get; set; }
        //true se sim, false se nao
        public string TipoPista { get; set; }
        //TipoPista recebe s se simples, d se dupla
        public string NivelAdministrativo { get; set;}
        //NivelAdministrativo armazena m=>municipal, e=> estadual,f=>federal
        public RelacaoEstrada DadosEstrada { get; set; }
    }
}
