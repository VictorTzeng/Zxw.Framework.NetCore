using System;
using ICanPay.Alipay;
using ICanPay.Core;

namespace Zxw.Framework.Pay.AliPay
{
    public class AliPay:IAliPay
    {
        private IGateways _gateways;

        public AliPay(IGateways getGateways)
        {
            _gateways = getGateways;
        }

        public void BillDownload(string billType, string billDate)
        {
            var gateway = _gateways.Get<AlipayGateway>();

            gateway.BillDownload(new Auxiliary
            {
                BillType = billType,
                BillDate = billDate
            });
        }

        public string CreateOrder(IOrder order, GatewayTradeType tradeType,
            Action<object, PaymentSucceedEventArgs> succeed = null,
            Action<object, PaymentFailedEventArgs> failed = null)
        {
            if (order.GetType() != typeof(Order))
            {
                throw new ArgumentException($"{nameof(order)} must be a ICanPay.AliPay.Order");
            }
            var gateway = _gateways.Get<AlipayGateway>(tradeType);
            if (tradeType == GatewayTradeType.Barcode)
            {
                if (succeed==null)
                {
                    throw new ArgumentNullException(nameof(succeed));
                }
                if (failed == null)
                {
                    throw new ArgumentNullException(nameof(failed));
                }
                gateway.PaymentSucceed += succeed;
                gateway.PaymentFailed += failed;
            }
            return gateway.Payment(order);
        }

        public INotify Cancel(string orderNo)
        {
            var gateway = _gateways.Get<AlipayGateway>();
            return (Notify)gateway.Cancel(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }

        public INotify Close(string orderNo)
        {
            var gateway = _gateways.Get<AlipayGateway>();
            return (Notify)gateway.Close(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }

        public INotify Query(string orderNo)
        {
            var gateway = _gateways.Get<AlipayGateway>();
            return (Notify)gateway.Query(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }

        public INotify Refund(string orderNo)
        {
            var gateway = _gateways.Get<AlipayGateway>();
            return (Notify)gateway.Refund(new Auxiliary()
            {
                OutTradeNo = orderNo
            });
        }

        public INotify RefundQuery(string orderNo, string refundNo)
        {
            var gateway = _gateways.Get<AlipayGateway>();
            return (Notify)gateway.RefundQuery(new Auxiliary()
            {
                OutTradeNo = orderNo,
                OutRefundNo = refundNo
            });
        }
    }
}
