<%@ Page Title="Cadastro de Cliente" Language="C#" MasterPageFile="~/Template/ContainerPrincipal.Master" AutoEventWireup="true" CodeBehind="cliente.aspx.cs" Inherits="TreinamentoHeitor.cliente" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <asp:Label Text="Nome: " runat="server" />
    <asp:TextBox runat="server" ID="txtnome" />
    <br>
    <asp:Label Text="Data: " runat="server" />
    <asp:TextBox runat="server" ID="txtdata" />
    <br>
    <asp:Label Text="Endereço: " runat="server" />
    <asp:TextBox runat="server" ID="txtendereco" />
    <br>
    <asp:Label Text="CPF: " runat="server" />
    <asp:TextBox runat="server" ID="txtcpf" />
    <br>
    <asp:Button Text="Enviar" runat="server" ID="Btnenviar" OnClick="Btnenviar_Click" />
    <asp:Label ID="txterro" runat="server" />
    <br>
    <hr>
    <h1>Todos os Clientes</h1>
    <br>
    <table border="1">
        <thead>
            <tr>
                <th>Codigo</th>
                <th>Nome</th>
                <th>DataNascimento</th>
                <th>CPF</th>
                <th>Endereco</th>
            </tr>
        </thead>
        <tbody>
            <asp:Repeater runat="server" ID="repetidor">
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("Codigo")  %></td>
                        <td><%# Eval("Nome")  %></td>
                        <td><%# Eval("DataNascimento")  %></td>
                        <td><%# Eval("Cpf")  %></td>
                        <td><%# Eval("Endereco")  %></td>                        
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
        </tbody>
    </table>
</asp:Content>
