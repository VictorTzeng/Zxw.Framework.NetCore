using System;
using ICanPay.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.Pay
{
    public static class PayExtensions
    {
        public static IServiceCollection RegisterPayMerchant<TGateway, TMerchant>(this IServiceCollection services,
            TMerchant merchant)
            where TGateway : GatewayBase, new()
            where TMerchant : class, IMerchant
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (merchant == null)
                throw new ArgumentNullException(nameof(merchant));
            services.AddICanPay(m =>
            {
                var gateways = AutofacContainer.Resolve<IGateways>() ?? new Gateways();
                var gateway = (TGateway) Activator.CreateInstance(typeof(TGateway), (object) merchant);
                gateways.Add(gateway);
                //switch (merchant.GetType().FullName)
                //{
                //    case "ICanPay.Alipay.Merchant":
                //        gateways.Add(new ICanPay.Alipay.AliPayGateway((ICanPay.Alipay.Merchant) merchant));
                //        break;
                //    case "ICanPay.Wechatpay.Merchant":
                //        gateways.Add(new ICanPay.Wechatpay.WechatpayGateway((ICanPay.Wechatpay.Merchant) merchant));
                //        break;
                //    case "ICanPay.Unionpay.Merchant":
                //        gateways.Add(new ICanPay.Unionpay.UnionpayGateway((ICanPay.Unionpay.Merchant) merchant));
                //        break;
                //    default:
                //        throw new Exception("");
                //}

                return gateways;
            });
            return services;
        }

        public static IApplicationBuilder UsePay(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            return app.UseICanPay();
        }
    }
}
