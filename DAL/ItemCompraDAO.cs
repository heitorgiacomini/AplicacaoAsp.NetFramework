using Modelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ItemCompraDAO : Conexao
    {
        //public List<Modelo.ItemCompra> SelectItems()
        //{
        //    try
        //    {
        //        //List<Modelo.Compra> listaretorno = new List<Modelo.Compra>();

        //        List<Modelo.ItemCompra> listaretorno = new List<Modelo.ItemCompra>();
        //        SqlCommand comando = CriarComando("Select * From ItemCompra Where Excluido = 0");
        //        using (SqlDataReader reader = comando.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                Modelo.ItemCompra c = new Modelo.ItemCompra();
        //                c.Codigo = (int)reader["Codigo"];
        //                c.codProduto = (int)reader["codProduto"];
        //                c.codCompra = (int)reader["codCompra"];
        //                c.Quantidade = (int)reader["Quantidade"];
        //                (DateTime)reader["DataCriacao"]; 
        //                c.Descricao = (string)reader["Descricao"];
        //                c.ValorUnitario = (decimal)reader["ValorUnitario"];
        //                listaretorno.Add(c);
        //            }
        //        }
        //        comando.Connection.Close();
        //        return listaretorno;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        
        public void SalvarItem(Modelo.ItemCompra item)
        {
            try
            {
                //"Insert into Produto (Descricao, ValorUnitario) values (@descricao, @valorunitario)";
                SqlCommand comando = CriarComando("Insert into ItemCompra (codProduto,codCompra,Quantidade) values (@codProduto, @codCompra, @Quantidade)");
                comando.Parameters.AddWithValue("@codProduto", item.codProduto);
                comando.Parameters.AddWithValue("@codCompra", item.codCompra);
                comando.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                comando.ExecuteNonQuery();
                comando.Connection.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void SalvarObjetoCompraComCodigo(Modelo.Compra modelocompra, int codigocompra)
        {
            //Modelo.ItemCompra item = new Modelo.ItemCompra();
            try
            {
                foreach (Modelo.ItemCompra linha in modelocompra.AuxItems)
                {
                    linha.codCompra = codigocompra;
                    SalvarItem(linha);
                    //Modelo.ItemCompra linha = new Modelo.ItemCompra();
                    //Label labelCodigo = (Label)item.FindControl("lbCodigo");
                    //linha.Codigo = int.Parse(labelCodigo.Text);
                    //linha.codProduto = int.Parse(((Label)item.FindControl("lbcodProduto")).Text);
                    //linha.auxDescricao = ((Label)item.FindControl("lblauxDescricao")).Text;
                    //linha.auxValor = decimal.Parse(((Label)item.FindControl("lblauxValor")).Text);
                    //linha.Quantidade = int.Parse(((Label)item.FindControl("lblQuantidade")).Text);
                    //listaVerificacao.Add(linha);

                    //SalvarItem();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
