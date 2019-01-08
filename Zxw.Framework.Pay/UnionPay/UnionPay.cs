using System;
using ICanPay.Core;
using ICanPay.Unionpay;

namespace Zxw.Framework.Pay.UnionPay
{
    /// <summary>
    /// 银联支付
    /// </summary>
    public class UnionPay:PayBase<UnionpayGateway>
    {
        public UnionPay(IGateways getGateways) : base(getGateways)
        {
        }

        /// <summary>
        /// 银联对账单下载
        /// </summary>
        /// <param name="billType"></param>
        /// <param name="billDate"></param>
        public void BillDownload(string billType, string billDate)
        {
            base.BillDownload(new Auxiliary() { BillDate = billDate, TxnTime = billType });
        }

        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="tradeType"></param>
        /// <param name="succeed"></param>
        /// <param name="failed"></param>
        /// <returns></returns>
        public string CreateOrder(Order order, GatewayTradeType tradeType,
            Action<object, PaymentSucceedEventArgs> succeed = null,
            Action<object, PaymentFailedEventArgs> failed = null)
        {
            return base.CreateOrder(order, tradeType, succeed, failed);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="tradeNo"></param>
        /// <param name="outRefundNo"></param>
        /// <param name="refundAmount"></param>
        /// <returns></returns>
        public Notify Cancel(string tradeNo, string outRefundNo, double? refundAmount)
        {
            return (Notify)base.Cancel(new Auxiliary()
            {
                TradeNo = tradeNo,
                OutRefundNo = outRefundNo,
                TxnTime = DateTime.Now.ToString("yyyyMMddHHmmss"),
                RefundAmount = refundAmount
            });
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public Notify Query(string orderNo)
        {
            return (Notify)base.Query(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }
        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public Notify Refund(string orderNo)
        {
            return (Notify)base.Refund(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }
        /// <summary>
        /// 订单退款查询
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="refundNo"></param>
        /// <returns></returns>
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
