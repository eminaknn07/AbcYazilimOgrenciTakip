using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbcYazilim.OgrenciTakip.Common.Enums
{
    public enum KartTuru:byte
    {
        [Description("Okul Kartı")]
        Okul=1,
        [Description("İl Kartı")]
        Il,
        [Description("İlçe Kartı")]
        Ilce
    }
}
