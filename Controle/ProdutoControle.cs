using Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controle
{
    public class ProdutoControle
    {
        public void salvar(Produto modeloproduto)
        {
            try
            {
                if (modeloproduto.ValorUnitario <= 0)
                {
                    throw new Exception("Preço Invalido");
                }
                if (modeloproduto.Descricao == "")
                {
                    throw new Exception("Descrição Vazia");
                }
                DAL.ProdutoDAO daoproduto = new DAL.ProdutoDAO();
                daoproduto.Inserir(modeloproduto);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Produto BuscarPorCodigo(int id)
        {
            DAL.ProdutoDAO daoproduto = new DAL.ProdutoDAO();
            return daoproduto.BuscarPorId(id);
        }
        public List<Produto> BuscarTodosProdutos()
        {
            try
            {
                DAL.ProdutoDAO produto = new DAL.ProdutoDAO();
                return produto.BuscarTodos();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
