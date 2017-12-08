using System;
using ICanPay.Core;
using ICanPay.Wechatpay;

namespace Zxw.Framework.Pay.WeChatPay
{
    /// <summary>
    /// 微信支付
    /// </summary>
    public class WechatPay : PayBase<WechatpayGateway>
    {
        public WechatPay(IGateways getGateways) : base(getGateways)
        {
        }
        /// <summary>
        /// 微信对账单下载
        /// </summary>
        public void BillDownload(string billType, string billDate)
        {
            base.BillDownload(new Auxiliary() { BillDate = billDate, BillType = billType });
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
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public Notify Cancel(string orderNo)
        {
            return (Notify)base.Cancel(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }
        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public Notify Close(string orderNo)
        {
            return (Notify)base.Close(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }
        /// <summary>
        /// 订单查询
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
        /// <returns></returns>
        public Notify RefundQuery(string orderNo)
        {
            return (Notify)base.RefundQuery(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }
    }
}
