using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TreinamentoHeitor
{
    public partial class compra : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    ExibeTodasCompras();
                    Controle.ClienteControle controle = new Controle.ClienteControle();
                    List<Modelo.Cliente> listRetorno = controle.BuscarTodosClientes();
                    ddlTest.DataSource = listRetorno;
                    ddlTest.DataBind();

                    Controle.ProdutoControle controleproduto = new Controle.ProdutoControle();
                    List<Modelo.Produto> listRetornop = controleproduto.BuscarTodosProdutos();
                    listProduto.DataSource = listRetornop;
                    listProduto.DataBind();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ExibeTodasCompras()
        {
            try
            {
                Controle.CompraControle controlecompra = new Controle.CompraControle();
                List<Modelo.Compra> listacompra = new List<Modelo.Compra>();
                listacompra = controlecompra.SelectTodasCompras();
                RepetidorTabela.DataSource = listacompra;
                RepetidorTabela.DataBind();

            }
            catch (Exception)
            {
                throw;
            }
        }
        protected decimal ValorProduto(int id)
        {
            try
            {
                Controle.ProdutoControle pesquisa = new Controle.ProdutoControle();
                Modelo.Produto produto = new Modelo.Produto();
                produto = pesquisa.BuscarPorCodigo(id);
                return produto.ValorUnitario;
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void RepetidorTabela_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            try
            {
                List<Modelo.Compra> listacompra = new List<Modelo.Compra>();
                Controle.CompraControle controlecompra = new Controle.CompraControle();
                int codigo = int.Parse(e.CommandArgument.ToString());
                listacompra = controlecompra.SelectTodasCompras(codigo);
                RepetidorCentral.DataSource = listacompra;
                RepetidorCentral.DataBind();
            }
            catch (Exception)
            {
                throw;
            }

        }

        protected void BtnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                Modelo.Compra compra = new Modelo.Compra();
                compra.CodCliente = int.Parse(ddlTest.SelectedValue);
                List<Modelo.ItemCompra> comprasRealizadas = ObterDados();
                compra.ValorTotal = comprasRealizadas.Select(x => x.auxValorTotal).Sum();
                compra.AuxItems = comprasRealizadas;

                Controle.CompraControle compracontrole = new Controle.CompraControle();
                compracontrole.SalvarCompra(compra);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<Modelo.ItemCompra> ObterDados()
        {
            try
            {
                List<Modelo.ItemCompra> listaVerificacao = new List<Modelo.ItemCompra>();
                foreach (RepeaterItem item in RepetidorCentral.Items)
                {
                    Repeater subrepeater = (Repeater)item.FindControl("RepetidorItems");
                    foreach (RepeaterItem subitem in subrepeater.Items)
                    {
                        Modelo.ItemCompra linha = new Modelo.ItemCompra();
                        Label labelCodigo = (Label)subitem.FindControl("lbCodigo");
                        linha.Codigo = int.Parse(labelCodigo.Text);
                        linha.codProduto = int.Parse(((Label)subitem.FindControl("lbcodProduto")).Text);
                        linha.auxDescricao = ((Label)subitem.FindControl("lblauxDescricao")).Text;
                        linha.auxValor = decimal.Parse(((Label)subitem.FindControl("lblauxValor")).Text);
                        linha.Quantidade = int.Parse(((TextBox)subitem.FindControl("lblQuantidade")).Text);
                        listaVerificacao.Add(linha);
                    }
                }
                return listaVerificacao;
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void BtnAddProduto_Click(object sender, EventArgs e)
        {
            try
            {
                Modelo.ItemCompra subItem = new Modelo.ItemCompra();
                subItem.codProduto = int.Parse(listProduto.SelectedValue);
                subItem.auxDescricao = listProduto.SelectedItem.Text;
                subItem.auxValor = ValorProduto(int.Parse(listProduto.SelectedValue));
                subItem.Quantidade = int.Parse(txtqtdproduto.Text);
                List<Modelo.ItemCompra> listaItemComprados = ObterDados(); // listaProdutosComprados; // = controle.BuscarTodosClientes();
                listaItemComprados.Add(subItem);

                List<Modelo.Compra> listacompra = new List<Modelo.Compra>();
                Modelo.Compra modelocompra = new Modelo.Compra();
                modelocompra.AuxItems = listaItemComprados;
                listacompra.Add(modelocompra);
                RepetidorCentral.DataSource = listacompra;
                RepetidorCentral.DataBind();

            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void RepetidorTabela_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                Repeater r = (Repeater)e.Item.FindControl("RepetidorItems");
                List<Modelo.ItemCompra> listaitems = new List<Modelo.ItemCompra>();
                Modelo.Compra compramodelo = (Modelo.Compra)e.Item.DataItem;
                listaitems = compramodelo.AuxItems;
                r.DataSource = listaitems;
                r.DataBind();
            }
            catch (Exception)
            {

                throw;
            }
        }

        protected void RepetidorItems_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            //throw Exce

        }
    }
}