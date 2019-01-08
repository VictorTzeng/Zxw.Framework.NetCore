using System;
using ICanPay.Core;

namespace Zxw.Framework.Pay
{
    public abstract class PayBase<TGateWay> :IPay 
        where TGateWay : GatewayBase
    {
        private IGateways _gateways;

        public virtual GatewayBase Gateway => _gateways.Get<TGateWay>();

        protected PayBase(IGateways getGateways)
        {
            _gateways = getGateways;
        }
        public virtual void BillDownload(IAuxiliary auxiliary)
        {
            Gateway.BillDownload(auxiliary);
        }

        public virtual string CreateOrder(IOrder order, GatewayTradeType tradeType, Action<object, PaymentSucceedEventArgs> succeed = null, Action<object, PaymentFailedEventArgs> failed = null)
        {
            if (tradeType == GatewayTradeType.Barcode)
            {
                if (succeed == null)
                {
                    throw new ArgumentNullException(nameof(succeed));
                }
                if (failed == null)
                {
                    throw new ArgumentNullException(nameof(failed));
                }
                Gateway.PaymentSucceed += succeed;
                Gateway.PaymentFailed += failed;
            }
            return Gateway.Payment(order);
        }

        public virtual INotify Cancel(IAuxiliary auxiliary)
        {
            return Gateway.Cancel(auxiliary);
        }

        public virtual INotify Close(IAuxiliary auxiliary)
        {
            return Gateway.Close(auxiliary);
        }

        public virtual INotify Query(IAuxiliary auxiliary)
        {
            return Gateway.Query(auxiliary);
        }

        public virtual INotify Refund(IAuxiliary auxiliary)
        {
            return Gateway.Refund(auxiliary);
        }

        public virtual INotify RefundQuery(IAuxiliary auxiliary)
        {
            return Gateway.RefundQuery(auxiliary);
        }
    }
}
