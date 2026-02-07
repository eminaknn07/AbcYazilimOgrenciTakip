using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AbcYazilim.OgrenciTakip.Bll.Functions
{
    /// <summary>
    /// Uygulama genelindeki veri doğrulama (validation) kurallarını merkezi olarak yöneten yardımcı sınıftır.
    /// </summary>
    /// <remarks>
    /// Bu sınıf, nesne özelliklerine (Property) tanımlanmış olan özel nitelikleri (Attribute) 
    /// kullanarak dinamik doğrulama yapar. Kayıt işlemi öncesinde verilerin iş kurallarına 
    /// uygunluğunu denetleyerek hatalı veri girişini engeller ve kullanıcıya standart 
    /// hata mesajları dönülmesini sağlar.
    /// </remarks>
    public static class ValidationFunctions
    {

        /// <summary>
        /// Belirtilen tipteki (Entity) tüm property'leri tarayarak, üzerlerinde tanımlanmış olan belirli bir Attribute türünü toplar.
        /// </summary>
        /// <typeparam name="TAttribute">Aranan nitelik (Attribute) türü.</typeparam>
        /// <param name="entityType">Üzerinde arama yapılacak olan sınıf türü.</param>
        /// <returns>Property bilgisi ve ilgili Attribute nesnesini içeren <see cref="PropertyAttribute{T}"/> listesi döner.</returns>
        /// <remarks>
        /// Bu metot şu özellikleri destekler:
        /// <list type="bullet">
        /// <item><description><b>Recursive Arama:</b> Sadece ana sınıftaki değil, sınıfın uyguladığı tüm Interface'lerdeki Attribute'ları da bulur.</description></item>
        /// <item><description><b>Inheritance Desteği:</b> <c>GetCustomAttributes(true)</c> kullanımı sayesinde kalıtım yoluyla gelen nitelikleri de yakalar.</description></item>
        /// <item><description><b>Toplu Eşleşme:</b> Bir property üzerinde birden fazla aynı türde Attribute varsa hepsini listeye ekler.</description></item>
        /// </list>
        /// </remarks>
        public static List<PropertyAttribute<TAttribute>> GetPropertyAttributesFromType<TAttribute> (this Type entityType) where TAttribute:Attribute
        {
            var list = new List<PropertyAttribute<TAttribute>>();
            var properties=entityType.GetProperties();

            foreach(var property in properties)
            {
                var attributes = property.GetCustomAttributes<TAttribute>(true).ToList();
                if(!attributes.Any()) continue;
                list.AddRange(attributes.Select(x => new PropertyAttribute<TAttribute>(property, x)));
            }
            var interfaces=entityType.GetInterfaces();
            foreach(var iface in interfaces)
            {
                list.AddRange(iface.GetPropertyAttributesFromType<TAttribute>());
            }
            return list;
        }

        /// <summary>
        /// Bir sınıfa ait özellik (Property) bilgisi ile o özelliğe atanmış olan nitelik (Attribute) nesnesini bir arada tutan yardımcı sınıftır.
        /// </summary>
        /// <typeparam name="TAttribute">İlgili özellik üzerinde aranan <see cref="Attribute"/> türü.</typeparam>
        /// <remarks>
        /// Bu sınıf, yansıma (reflection) işlemleri sırasında her bir özellik ve onun meta verisi arasındaki 
        /// bağlantıyı korumak için kullanılır. Özellikle dinamik doğrulama (validation) ve otomatik arayüz 
        /// oluşturma süreçlerinde, hangi kuralın hangi alan için geçerli olduğunu merkezi bir yapıda saklar.
        /// </remarks>
        public class PropertyAttribute<TAttribute>
        {
            /// <summary>
            /// Niteliğin tanımlı olduğu özellik bilgilerini (Ad, Tip vb.) barındırır.
            /// </summary>
            public PropertyInfo Property { get; }
            /// <summary>
            /// Özellik üzerine yerleştirilmiş olan özel nitelik nesnesini barındırır.
            /// </summary>
            public TAttribute Attribute { get; }

            /// <summary>
            /// Özellik ve Nitelik ikilisini eşleştirerek yeni bir örnek oluşturur.
            /// </summary>
            /// <param name="property">İlgili özellik bilgisi.</param>
            /// <param name="attribute">İlgili nitelik nesnesi.</param>
            public PropertyAttribute(PropertyInfo property, TAttribute attribute)
            {
                Attribute = attribute;
                Property = property;
            }
        }
    }
}
