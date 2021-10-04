using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TreinamentoHeitor
{
    public partial class cliente : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    Controle.ClienteControle controle = new Controle.ClienteControle();
                    List<Modelo.Cliente> listRetorno = controle.BuscarTodosClientes();
                    repetidor.DataSource = listRetorno;
                    repetidor.DataBind();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected void Btnenviar_Click(object sender, EventArgs e)
        {
            try
            {
                Modelo.Cliente cliente = new Modelo.Cliente();
                cliente.Nome = txtnome.Text;
                cliente.DataNascimento = DateTime.Parse(txtdata.Text);
                cliente.Cpf = txtcpf.Text;
                cliente.Endereco = txtendereco.Text;

                Controle.ClienteControle controle = new Controle.ClienteControle();
                controle.Salvar(cliente);
            }
            catch (Exception error)
            {
                txterro.Text = error.Message;
            }
        }

        public object RetornaAlgumaCoisa()
        {
            //return "Teste";

            return null;
        }


        

    }
}