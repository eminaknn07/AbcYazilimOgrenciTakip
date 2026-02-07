using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbcYazilim.OgrenciTakip.Model.Attributes
{
    /// <summary>
    /// Veritabanında benzersiz (unique) olması gereken alanları işaretlemek ve doğrulama hatası durumunda kullanıcıya sunulacak bilgileri tanımlamak için kullanılır.
    /// </summary>
    /// <remarks>
    /// Bu nitelik, özellikle "Kod", "TC Kimlik No" veya "Kullanıcı Adı" gibi mükerrer kayıt kontrolü gerektiren alanlar için tasarlanmıştır. 
    /// Yansıma (Reflection) aracılığıyla çalışma zamanında okunarak, hatalı durumda hangi arayüz kontrolüne odaklanılacağını ve hangi açıklama metninin gösterileceğini belirler.
    /// </remarks>
    public class Kod:Attribute
    {/// <summary>
     /// Hata mesajında gösterilecek olan alanın anlaşılır açıklaması (Örn: "Kod").
     /// </summary>
        public string Description { get; }

        /// <summary>
        /// Doğrulama başarısız olduğunda odaklanılacak (Focus) olan arayüz kontrolünün adı.
        /// </summary>
        public string ControlName { get; }

        /// <summary>
        /// Yeni bir Kod niteliği örneği oluşturur.
        /// </summary>
        /// <param name="description">Hata mesajı için açıklama.</param>
        /// <param name="controlName">Hata durumunda odaklanılacak kontrolün adı.</param
        public Kod(string description, string controlName)
        {
            Description = description;
            ControlName = controlName;
        }
    }
}
