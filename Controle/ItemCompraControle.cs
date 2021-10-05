using Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controle
{
    public class ItemCompraControle
    {
        public void Desabilitaritem(Modelo.ItemCompra modelitem)
        {
            try
            {
                DAL.ItemCompraDAO itemdao = new DAL.ItemCompraDAO();
                itemdao.DesativarItem(modelitem);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void DesabilitarItemsPeloCodigoCompra(Modelo.ItemCompra modelitem)
        {
            try
            {
                DAL.ItemCompraDAO itemdao = new DAL.ItemCompraDAO();
                itemdao.DesativarItemsPeloCodigodaCompra(modelitem);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void SalvarCompra(Modelo.Compra itemcompra)
        {
            try
            {
                DAL.CompraDAO daocompra = new DAL.CompraDAO();
                daocompra.Salvar(itemcompra);
            }
            catch (Exception)
            {
                throw;
            }

        }
        public void SalvarObjetoCompra(Modelo.Compra modelocompra, int codigo)
        {
            try
            {
                DAL.ItemCompraDAO itemdao = new DAL.ItemCompraDAO();
                itemdao.SalvarObjetoCompraComCodigo(modelocompra, codigo);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
