using SistemaCompra.Domain.Core;
using SistemaCompra.Domain.Core.Model;
using SistemaCompra.Domain.ProdutoAggregate;
using SistemaCompra.Domain.SolicitacaoCompraAggregate.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaCompra.Domain.SolicitacaoCompraAggregate
{
    public class SolicitacaoCompra : Entity
    {
        internal const int VALOR_ACIMA_TOTAL_GERAL_CONDICAO_GERAL = 50000;

        public UsuarioSolicitante UsuarioSolicitante { get; private set; }
        public NomeFornecedor NomeFornecedor { get; private set; }
        public IList<Item> Itens { get; private set; }
        public DateTime Data { get; private set; }
        public Money TotalGeral { get; private set; }
        public Situacao Situacao { get; private set; }        public CondicaoPagamento CondicaoPagamento { get; private set; }

        private SolicitacaoCompra() { }

        public SolicitacaoCompra(string usuarioSolicitante, string nomeFornecedor)
        {
            Id = Guid.NewGuid();
            UsuarioSolicitante = new UsuarioSolicitante(usuarioSolicitante);
            NomeFornecedor = new NomeFornecedor(nomeFornecedor);
            Data = DateTime.Now;
            Situacao = Situacao.Solicitado;
            Itens = new List<Item>();
            TotalGeral = new Money();
        }

        public void AdicionarItem(Produto produto, int qtde)
        {
            Itens.Add(new Item(produto, qtde));
        }

        public void AdicionarItensComprasSolicitadas(IEnumerable<Item> itens)
        {
            if (ItemExistente(itens))
            {
                foreach (var item in itens)
                {
                    AdicionarItem(item.Produto, item.Qtde);
                }

                CalcularValorTotalGeral(itens);
            }
        }

        public void RegistrarCompra(IEnumerable<Item> itens)
        {
            AdicionarItensComprasSolicitadas(itens);

            ObterCondicoesPagamento(TotalGeral);

            AddEvent(new CompraRegistradaEvent(Id, itens, TotalGeral.Value));
        }

        public void ObterCondicoesPagamento(Money totalGeral)
        {
            if (totalGeral.Value > VALOR_ACIMA_TOTAL_GERAL_CONDICAO_GERAL)
            {
                CondicaoPagamento = new CondicaoPagamento(30);
            }
        }

        public bool ItemExistente(IEnumerable<Item> itens)
        {
            if (itens.Count() == 0)
                throw new BusinessRuleException("A solicitação de compra deve possuir itens!");

            return true;
        }

        private void CalcularValorTotalGeral(IEnumerable<Item> itens)
        {
            if (ItemExistente(itens))
            {
                foreach (Item item in itens)
                {
                    this.TotalGeral = new Money(item.Subtotal.Value);
                }
            }
        }
    }
}
