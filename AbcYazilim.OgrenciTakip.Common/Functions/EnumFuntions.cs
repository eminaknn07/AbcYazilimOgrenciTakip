using System;
using System.ComponentModel;

namespace AbcYazilim.OgrenciTakip.Common.Functions
{
    /// <summary>
    /// Enum (Numaralandırma) tipleri üzerinde öznitelik okuma ve adlandırma işlemlerini kolaylaştıran yardımcı sınıf.
    /// </summary>
    public static class EnumFuntions
    {
        /// <summary>
        /// Bir enum değerine atanmış olan belirli bir tipteki özniteliği (attribute) geri döndürür.
        /// </summary>
        /// <typeparam name="T">Aranan öznitelik tipi (Örn: <see cref="DescriptionAttribute"/>).</typeparam>
        /// <param name="value">Özniteliği okunacak olan enum değeri.</param>
        /// <returns>Bulunursa ilgili öznitelik nesnesini, bulunamazsa null döndürür.</returns>
        private static T GetAttribute<T>(this Enum value) where T : Attribute 
        {
            if(value == null) return null;
            var memberInfo = value.GetType().GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(T),false);
            return (T)attributes[0];
        }
        /// <summary>
        /// Enum değerinin <see cref="DescriptionAttribute"/> ile tanımlanmış açıklamasını döndürür. 
        /// Eğer açıklama tanımlanmamışsa enum değerinin kendi adını döndürür.
        /// </summary>
        /// <param name="value">Metne dönüştürülecek olan enum değeri.</param>
        /// <returns>Enumun açıklama metni veya string karşılığı.</returns>
        /// <remarks>
        /// Bu metot sayesinde "KartTuru.Okul" değeri arayüzde doğrudan "Okul Kartı" gibi kullanıcı dostu bir metin olarak gösterilebilir.
        /// </remarks>
        public static string ToName(this Enum value)
        {
            if( value == null) return null;
            var attribute = value.GetAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString():attribute.Description;
        }
    }
}
