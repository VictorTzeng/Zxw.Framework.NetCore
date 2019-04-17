using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Zxw.Framework.NetCore.DbContextCore;

namespace Zxw.Framework.NetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FromDbContextFactoryAttribute: Attribute, IBindingSourceMetadata
    {
        public string DbContextTagName { get; set; }
        public BindingSource BindingSource => BindingSource.Services;

        public FromDbContextFactoryAttribute(string tagName)
        {
            DbContextTagName = tagName;
        }

        public override bool Match(object obj)
        {
            return base.Match(obj);
        }
    }
}
