using System;
using ICanPay.Core;

namespace Zxw.Framework.Pay
{
    public interface IPay
    {
        void BillDownload(string billType, string billDate);

        string CreateOrder(IOrder order, GatewayTradeType tradeType,
            Action<object, PaymentSucceedEventArgs> succeed = null,
            Action<object, PaymentFailedEventArgs> failed = null);

        INotify Cancel(string orderNo);

        INotify Close(string orderNo);

        INotify Query(string orderNo);

        INotify Refund(string orderNo);

        INotify RefundQuery(string orderNo, string refundNo);
    }

    public interface IAliPay : IPay
    {
        
    }

    public interface IUnoinPay : IPay
    {

    }

    public interface IWeChatPay : IPay
    {

    }
}
