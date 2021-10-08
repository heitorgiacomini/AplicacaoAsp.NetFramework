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
                if (e.CommandName.ToString() == "Editar")
                {
                    List<Modelo.Compra> listacompra = new List<Modelo.Compra>();
                    Controle.CompraControle controlecompra = new Controle.CompraControle();
                    int codigo = int.Parse(e.CommandArgument.ToString());
                    listacompra = controlecompra.SelectTodasCompras(codigo);
                    RepetidorCentral.DataSource = listacompra;
                    RepetidorCentral.DataBind();
                }
                else if (e.CommandName.ToString() == "Desabilitar")
                {                    
                    Controle.CompraControle controlecompra = new Controle.CompraControle();
                    int codigo = int.Parse(e.CommandArgument.ToString());
                    Modelo.Compra comprix = new Modelo.Compra();
                    comprix.Codigo = codigo;
                    controlecompra.DesabilitarCompra(comprix);   
                }

            }
            catch (Exception)
            {
                throw;
            }

        }
        protected bool VerificaSeJaExisteNaLista(List<Modelo.ItemCompra> listaitens, Modelo.ItemCompra modeloitem)
        {
            try
            {
                foreach (Modelo.ItemCompra itensdalista in listaitens)
                {
                    if(itensdalista.codProduto == modeloitem.codProduto)
                    {
                        return true;
                    }
                }
                return false;
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
                        linha.subTotal = decimal.Parse(((Label)subitem.FindControl("lblsubTotal")).Text);
                        linha.guid = ((Label)subitem.FindControl("lbguid")).Text;
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
                subItem.subTotal = subItem.auxValor * subItem.Quantidade;
                subItem.guid = Guid.NewGuid().ToString();

                List<Modelo.ItemCompra> listaItemComprados = ObterDados(); // listaProdutosComprados; // = controle.BuscarTodosClientes();
                if (VerificaSeJaExisteNaLista(listaItemComprados, subItem))
                {
                    listaItemComprados.First(x => x.codProduto == subItem.codProduto).Quantidade += subItem.Quantidade;
                }
                else
                {
                    listaItemComprados.Add(subItem);
                }

                Modelo.Compra modelocompra = new Modelo.Compra();
                modelocompra.AuxItems = listaItemComprados;
                DarBindRepetidorCentralFazSomaDoValorTotal(modelocompra);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void DarBindRepetidorCentralFazSomaDoValorTotal(Modelo.Compra modelocompra)
        {
            try
            {
                foreach (RepeaterItem item in RepetidorCentral.Items)
                {
                    Label labelCodigo = (Label)item.FindControl("lblCodigo");
                    modelocompra.Codigo = int.Parse(labelCodigo.Text);
                    modelocompra.NomeCliente = ((Label)item.FindControl("lblNomeCliente")).Text;
                    modelocompra.DataCompra = DateTime.Parse(((Label)item.FindControl("lblDataCompra")).Text);
                }                
                modelocompra.ValorTotal = modelocompra.AuxItems.Select(x => x.auxSubTotal).Sum();
                //modelocompra.ValorTotal = modelocompra.AuxItems.Select(x => x.subTotal).Sum();
                List<Modelo.Compra> listacompra = new List<Modelo.Compra>();
                listacompra.Add(modelocompra);
                RepetidorCentral.DataSource = listacompra;
                RepetidorCentral.DataBind();
            }
            catch (Exception)
            {
                throw;
            }

        }
        protected void RepetidorItems_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            try
            {               
                List<Modelo.ItemCompra> listaItemComprados = ObterDados();
                Modelo.ItemCompra subItem = new Modelo.ItemCompra();
                
                string guidproduto = e.CommandArgument.ToString();
                subItem = listaItemComprados.Where(x => x.guid == guidproduto).FirstOrDefault();
                listaItemComprados.Remove(subItem);

                Modelo.Compra modelocompra = new Modelo.Compra();
                
                modelocompra.AuxItems = listaItemComprados;

                DarBindRepetidorCentralFazSomaDoValorTotal(modelocompra);
                //listacompra.Add(modelocompra);
                //RepetidorCentral.DataSource = listacompra;
                //RepetidorCentral.DataBind();
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
                compra.ValorTotal = comprasRealizadas.Select(x => x.subTotal).Sum();
                compra.AuxItems = comprasRealizadas;

                Controle.CompraControle compracontrole = new Controle.CompraControle();

                foreach (RepeaterItem item in RepetidorCentral.Items)
                {
                    compra.Codigo = int.Parse(((Label)item.FindControl("lblCodigo")).Text);
                }
                compracontrole.SalvarCompra(compra);
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


    }
}