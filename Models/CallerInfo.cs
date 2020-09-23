using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakipsiClient.Models
{
    public class CallerInfo
    {
        public string DeviceSerialNumber { get; set; }
        public string LineNumber { get; set; }
        public string CallerNumber { get; set; }
        public DateTime CallDateTime { get; set; }
    }
}
