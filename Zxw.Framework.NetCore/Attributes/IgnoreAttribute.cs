using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Zxw.Framework.NetCore.Attributes
{
    public class IgnoreAttribute:Attribute, IFilterMetadata
    {
    }
}
