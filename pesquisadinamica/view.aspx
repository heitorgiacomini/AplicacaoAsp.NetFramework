
                                                            <label for="ddlTipoPesquisa">Selecione Tipo de Pesquisa</label>
                                                            <asp:DropDownList ID="ddlTipoPesquisa" class="form-control"
                                                                runat="server" OnSelectedIndexChanged="ddlTipoPesquisa_SelectedIndexChanged" AutoPostBack="true">
                                                                <asp:ListItem Text="Selecione..." Value="Vazio" />
                                                                <asp:ListItem Text="Pesquisar Pela View" Value="view" />
                                                                <asp:ListItem Text="Pesquisar Pela Layer" Value="layer" />
                                                                <asp:ListItem Text="Pesquisar Pelo Banco de Dados Inteiro" Value="banco" />
                                                            </asp:DropDownList>
                                                            <label for="ddlTabelaBanco">Tabelas</label>
                                                            <asp:DropDownList ID="ddlTabelaBanco" class="form-control" runat="server"
                                                                OnSelectedIndexChanged="ddlTabelaBanco_SelectedIndexChanged" AutoPostBack="true">
                                                                <asp:ListItem Text="Selecione..." Value="Vazio" />
                                                            </asp:DropDownList>
                                                            <label for="ddlColunasDaTabela">Colunas</label>
                                                            <asp:DropDownList ID="ddlColunasDaTabela" class="form-control" runat="server">
                                                                <asp:ListItem Text="Selecione..." Value="Vazio" />
                                                            </asp:DropDownList>
                                                            <label for="txttermodapesquisa">Termo da pesquisa</label>
                                                            <asp:TextBox ID="txttermodapesquisa" placeholder="Informe o que deseja encontrar" CssClass="form-control" runat="server" />
                                                            <asp:Button ID="btnPesquisar" Text="Pesquisar" CssClass="btn btn-primary btnLoading"
                                                                OnClick="btnPesquisar_Click" runat="server" />
                                                        </div>
                                                    </div>



                                                    <div class="col-xs-9 col-md-9 col-lg-9 col-sm-9 checkbox checkboxDestacar">
                                                        <label>
                                                            <asp:CheckBox Text="Destacar Resultados?" ClientIDMode="Static" ID="cbDestacarResultados" runat="server" Visible="false" />
                                                        </label>
                                                    </div>
                                                </asp:Panel>
                                            </div>
                                        </div>
                                        <div class="panel panel-default">




                                            <div class="panel-heading">
                                                <span class="glyphicon glyphicon-th-list"></span>&nbsp
                                                Resultados
                                            </div>
                                            <div class="panel-body caixa-resultado-sidebar" id="DivExportarBuscaNovo" visible="false" clientidmode="Static" runat="server" style="padding-top: 0px; padding-bottom: 0px">
                                                <div class="row" style="height: 34px">
                                                    <div class="col-md-12" style="padding: 0px">
                                                        <div>
                                                            <iframe style="height: 34px;" id="iframeExportarBuscaNovo" clientidmode="static" class="iframe-busca" src="../../Exportacao/Busca.aspx"></iframe>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <asp:Repeater runat="server" ID="rpBuscaNovo" OnItemDataBound="rpBuscaNovo_ItemDataBound">
                                                        <ItemTemplate>
                                                            <div class="panel-group" id="accordionBusca" role="tablist" aria-multiselectable="false">
                                                                <div class="panel panel-default">
                                                                    <div class="panel-heading" role="tab" id="heading-EVAL-CODIGO-HEADING ">
                                                                        <h4 class="panel-title">
                                                                            <div class="row">
                                                                                <div class="col-sm-10 col-md-10 col-lg-10">
                                                                                    <a class="collapsed" role="button" data-toggle="collapse" data-parent="#accordionBusca" href="#collapseEVAL-CODIGO" aria-expanded="false" aria-controls="collapse-EVAL-CODIGO%"></a>
                                                                                </div>
                                                                                <div class="col-sm-2 col-md-2 col-lg-2">
                                                                                    <a href="#" onclick="VIZULIZAR(CAMADA)" data-dismiss="modal" style="font-size: 20px; padding: 0px">
                                                                                        <div class="glyphicon glyphicon-zoom-in"></div>
                                                                                    </a>
                                                                                </div>
                                                                            </div>
                                                                        </h4>
                                                                    </div>


                                                                    <div class="panel-body">
                                                                        <h5 style="line-height: inherit"><%#  Eval("Identificador")     %></h5>
                                                                        </b>
                                                                    <asp:Repeater runat="server" ID="RepetidorItens">
                                                                        <ItemTemplate>
                                                                            <ul>
                                                                                <li><%# Eval("Chave")  %> <%# Eval("Valor")  %></li>
                                                                            </ul>
                                                                        </ItemTemplate>
                                                                    </asp:Repeater>

                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </div>
                                            </div>
                                        </div>

