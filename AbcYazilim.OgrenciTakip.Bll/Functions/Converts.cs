using AbcYazilim.OgrenciTakip.Model.Entities.Base.Interfaces;
using System;
using System.Linq;

namespace AbcYazilim.OgrenciTakip.Bll.Functions
{
    public static class Converts
    {
        /// <summary>
        /// <see cref="IBaseEntity"/> arayüzünü uygulayan bir kaynağı, yansıma (reflection) yöntemini kullanarak hedef tipe dönüştürür.
        /// </summary>
        /// <typeparam name="TTarget">Verilerin aktarılacağı hedef nesne tipi.</typeparam>
        /// <param name="source">Veri kaynağı olan ve <see cref="IBaseEntity"/> arayüzünden türetilmiş nesne.</param>
        /// <returns>Dönüştürme başarılı ise verileri doldurulmuş <typeparamref name="TTarget"/> nesnesi, kaynak null ise varsayılan değer döner.</returns>
        /// <remarks>
        /// Metot, kaynak ve hedef nesnelerdeki aynı isimli özellikleri (properties) eşleştirir. 
        /// Boş string ("") değerleri veritabanı uyumluluğu için null olarak ayarlar.
        /// </remarks>
        public static TTarget EntityConvert<TTarget>(this IBaseEntity source)
        {
            if(source==null) return default(TTarget);
            var hedef=Activator.CreateInstance<TTarget>();
            var kaynakProp = source.GetType().GetProperties();
            var hedefProp=typeof(TTarget).GetProperties();

            foreach (var kp in kaynakProp)
            {
                var value = kp.GetValue(source);
                var hp=hedefProp.FirstOrDefault(x => x.Name == kp.Name);
                if(hp!=null)
                    hp.SetValue(hedef, ReferenceEquals(value,"")?null:value);
            }
            return hedef;
        }
    }
}
