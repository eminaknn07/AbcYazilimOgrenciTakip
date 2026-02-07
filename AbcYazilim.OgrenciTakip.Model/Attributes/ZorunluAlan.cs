using System;

namespace AbcYazilim.OgrenciTakip.Model.Attributes
{
    /// <summary>
    /// Veritabanı ve iş mantığı açısından doldurulması zorunlu olan alanları işaretlemek için kullanılır.
    /// </summary>
    /// <remarks>
    /// Bu nitelik, çalışma zamanında (runtime) yansıma (reflection) kullanılarak okunur. 
    /// Eğer işaretlenen alan boş (null, empty veya sayısal 0) ise, kullanıcıya <see cref="Description"/> 
    /// bilgisi üzerinden hata mesajı gösterilir ve odak (focus) <see cref="ControlName"/> ile 
    /// belirtilen arayüz kontrolüne yönlendirilir.
    /// </remarks>
    public class ZorunluAlan : Attribute
    {
        /// <summary>
        /// Boş bırakılan alanın kullanıcı dostu ismi (Örn: "TC Kimlik No", "Okul Adı").
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Hata durumunda imlecin (focus) konumlandırılacağı kontrolün adı.
        /// </summary>
        public string ControlName { get; }

        /// <summary>
        /// Yeni bir ZorunluAlan niteliği örneği oluşturur.
        /// </summary>
        /// <param name="description">Hata mesajında yer alacak açıklama metni.</param>
        /// <param name="controlName">Hata anında odaklanılacak bileşen adı.</param>
        public ZorunluAlan(string description, string controlName)
        {
            Description = description;
            ControlName = controlName;
        }
    }
}
