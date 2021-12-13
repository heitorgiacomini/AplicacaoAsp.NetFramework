

        protected void ddlTabelaBanco_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                Tabela tabela = new Tabela();
                tabela.NomeTabela = ddlTabelaBanco.SelectedItem.ToString();
                tabela.ApelidoTabela = ddlTabelaBanco.SelectedValue.ToString();
                tabela = TabelaCTR.PesquisarColunas(tabela);

                List<ListItem> listadropdown = new List<ListItem>();
                foreach (Coluna coluna in tabela.Coluna)
                {
                    listadropdown.Add(new ListItem { Text = coluna.Nome, Value = coluna.Nome });
                }

                listadropdown = TiraItensIndesejados(listadropdown);

                listadropdown = listadropdown.OrderBy(x => x.Value != "selecione").ThenBy(x => x.Text).ToList();

                ddlColunasDaTabela.DataSource = listadropdown;
                ddlColunasDaTabela.DataTextField = "Text";
                ddlColunasDaTabela.DataValueField = "Value";
                ddlColunasDaTabela.DataBind();

                rpBusca.DataSource = null;
                rpBusca.DataBind();

                DivExportarBusca.Visible = false;
            }
            catch (Exception )
            {
                throw ;
            }
        }
        List<ListItem> TiraItensIndesejados(List<ListItem> colunasnototal)
        {
            try
            {
                List<String> colunasexcluidas = new List<String> {
                    "codcliente",
                    "codfuncionariocriacao",
                    "codfuncionarioexclusao",
                    "codfuncionarioultimaalteracao",
                    "datacriacao",
                    "dataexclusao",
                    "dataultimaalteracao",
                    "excluido",
                    "geo",
                    "geom",
                    "ipcriacao",
                    "ipexclusao",
                    "ipultimaalteracao"};

                List < ListItem > colunasaceitas = new List<ListItem>();
                foreach (ListItem item in colunasnototal)
                {
                    if (!colunasexcluidas.Contains(item.Value.ToString()))
                    {
                        //colunasaceitas.Remove(item);
                        colunasaceitas.Add(item);
                    }
                }
                return colunasaceitas;
            }
            catch (Exception er)
            {

                throw er;
            }
        }

        List<String> TiraColunasIndesejadas(List<String> colunasnototal)
        {
            try
            {
                List<String> colunasexcluidas = new List<String> { "codcliente",
                    "codfucionariocriacao",
                    "codfuncionarioexclusao",
                    "codfuncionarioultimaalteracao",
                    "datacriacao",
                    "dataexclusao",
                    "dataultimaalteracao",
                    "excluido",
                    "geo",
                    "geom",
                    "ipcriacao",
                    "ipexclusao",
                    "ipultimaalteracao"};

                //var result = colunasnototal.Where(p => colunasexcluidas.All(p2 => p2 != p));
                //colunasaceitas = colunasaceitas.AddRange(colunasnototal.Where(p => colunasexcluidas.All(p2 => p2 != p)));
                IEnumerable<String> colunasaceitasienumerable = colunasnototal.Except(colunasexcluidas);

                return new List<string>(colunasaceitasienumerable);
                //colunasnototal = colunasnototal.all;
                //var colunasaceitas = colunasnototal.Except(colunasexcluidas);
            }
            catch (Exception er)
            {

                throw er;
            }
        }

        protected void btnPesquisar_Click(object sender, EventArgs e)
        {
            try
            {
                Tabela tabela = PegaTabelaEColunaSelecionadas();
                List<String> colunasnototal = new List<String>();
                foreach (ListItem item in ddlColunasDaTabela.Items)
                {
                    colunasnototal.Add(item.ToString());
                }
                List<String> colunasaceitas = TiraColunasIndesejadas(colunasnototal);

                //List<Dictionary<string, string>> resultadoselect =
                //TabelaCTR.PesquisaCamposCustomizados(colunasaceitas, tabela, txttermodapesquisa.Text.Trim());

                List<LinhaRegistro> resultadoselect =
                TabelaCTR.PesquisaCamposCustomizadosPelaTabela(colunasaceitas, tabela, txttermodapesquisa.Text.Trim());

                if (resultadoselect.Count > 0)
                {
                    DivExportarBuscaNovo.Visible = true;
                    rpBuscaNovo.DataSource = resultadoselect;
                    rpBuscaNovo.DataBind();
                }
                else
                {
                    //rpBuscaNovo.DataSource = null;
                    //rpBuscaNovo.DataBind();
                }
                // upPesquisa.Update();
            }
            catch (Exception er)
            {
                throw er;
            }
        }

        Tabela PegaTabelaEColunaSelecionadas()
        {
            Tabela tabela = new Tabela();
            tabela.ApelidoTabela = ddlTabelaBanco.SelectedItem.ToString();
            tabela.NomeTabela = ddlTabelaBanco.SelectedValue.ToString();
            Coluna coluna = new Coluna();
            coluna.Nome = ddlColunasDaTabela.SelectedItem.ToString();
            coluna.ApelidoNome = ddlColunasDaTabela.SelectedValue.ToString();
            tabela.Coluna.Add(coluna);
            return tabela;
        }
        protected void ddlTipoPesquisa_SelectedIndexChanged(object sender, EventArgs e)
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
            List<Tabela> tabelasdapesquisa = new List<Tabela>();
            switch (ddlTipoPesquisa.SelectedValue.ToString())
            {
                case "view":
                    tabelasdapesquisa = TabelaCTR.PesquisarTabelasPelaView(
                        int.Parse(funcionarioCliente.CodCliente.ToString()));
                    break;
                case "layer":
                    tabelasdapesquisa = TabelaCTR.PesquisarTabelasPeloLayerName(
                        int.Parse(funcionarioCliente.CodCliente.ToString()));
                    break;
                case "banco":
                    tabelasdapesquisa = TabelaCTR.PesquisarTabelasPeloBancoTodo(
                        int.Parse(funcionarioCliente.CodCliente.ToString()));
                    break;
            }

            List<ListItem> listadropdown = new List<ListItem>();
            foreach (Tabela itemtabela in tabelasdapesquisa)
            {
                listadropdown.Add(new ListItem { Text = itemtabela.NomeTabela, Value = itemtabela.ApelidoTabela });
            }

            listadropdown = listadropdown.OrderBy(x => x.Value != "selecione").ThenBy(x => x.Text).ToList();

            ddlTabelaBanco.DataSource = listadropdown;
            ddlTabelaBanco.DataTextField = "Text";
            ddlTabelaBanco.DataValueField = "Value";
            ddlTabelaBanco.DataBind();
            rpBusca.DataSource = null;
            rpBusca.DataBind();

            DivExportarBusca.Visible = false;
        }

        protected void rpBuscaNovo_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            Repeater r = (Repeater)e.Item.FindControl("RepetidorItens");
            
            //List<Modelo.ItemCompra> listaitems = new List<Modelo.ItemCompra>();
            //Modelo.Compra compramodelo = (Modelo.Compra)e.Item.DataItem;
            //listaitems = compramodelo.AuxItems;

            List<ChaveValor> listachavevalor = new List<ChaveValor>();
            LinhaRegistro modeloregistrolinha = (LinhaRegistro)e.Item.DataItem;
            listachavevalor = modeloregistrolinha.Registro;

            r.DataSource = listachavevalor;
            r.DataBind();
        }
    }





}
