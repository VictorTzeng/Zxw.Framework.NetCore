using System;
using ICanPay.Alipay;
using ICanPay.Core;

namespace Zxw.Framework.Pay.AliPay
{
    /// <summary>
    /// 支付宝支付
    /// </summary>
    public class AliPay:PayBase<AlipayGateway>
    {
        public AliPay(IGateways getGateways) : base(getGateways)
        {
        }

        public void BillDownload(string billType, string billDate)
        {
            base.BillDownload(new Auxiliary() {BillDate = billDate, BillType = billType});
        }

        public string CreateOrder(Order order, GatewayTradeType tradeType,
            Action<object, PaymentSucceedEventArgs> succeed = null,
            Action<object, PaymentFailedEventArgs> failed = null)
        {
            return base.CreateOrder(order, tradeType, succeed, failed);
        }

        public Notify Cancel(string orderNo)
        {
            return (Notify)base.Cancel(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }

        public Notify Close(string orderNo)
        {
            return (Notify)base.Close(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }

        public Notify Query(string orderNo)
        {
            return (Notify)base.Query(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }

        public Notify Refund(string orderNo)
        {
            return (Notify)base.Refund(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }

        public Notify RefundQuery(string orderNo, string refundNo)
        {
            return (Notify)base.RefundQuery(new Auxiliary()
            {
                OutTradeNo = orderNo,
                OutRefundNo = refundNo
            });
        }
    }
}
