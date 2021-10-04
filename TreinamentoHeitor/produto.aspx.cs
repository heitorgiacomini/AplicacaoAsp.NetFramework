using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TreinamentoHeitor
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    Controle.ProdutoControle controle = new Controle.ProdutoControle();
                    List<Modelo.Produto> listRetorno = controle.BuscarTodosProdutos();
                    repetidor.DataSource = listRetorno;
                    repetidor.DataBind();
                }
            }
            catch (Exception)
            {
                throw;
                //esse throw vai jogar pra quem?

            }
        }
        protected void cadastrarProduto_Click(object sender, EventArgs e)
        {
            try
            {
                Modelo.Produto modeloproduto = new Modelo.Produto();
                modeloproduto.Descricao = txtdescricao.Text;
                modeloproduto.ValorUnitario = decimal.Parse(txtvalor.Text);

                Controle.ProdutoControle controleproduto = new Controle.ProdutoControle();
                controleproduto.salvar(modeloproduto);
            }
            catch (Exception error)
            {
                txterro.Text = error.Message;
            }

        }
    }
}