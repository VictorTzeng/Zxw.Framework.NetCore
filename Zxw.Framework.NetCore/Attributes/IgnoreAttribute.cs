using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Zxw.Framework.NetCore.Attributes
{
    public class IgnoreAttribute:Attribute, IFilterMetadata
    {
    }
}
