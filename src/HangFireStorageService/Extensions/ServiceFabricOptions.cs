using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireStorageService.Extensions
{
    public class ServiceFabricOptions
    {
        /// <summary>
        /// 服务名称的前缀
        /// </summary>
        public string Prefix { get; set; } = "Default";
    }
}
