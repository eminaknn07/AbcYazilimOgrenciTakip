using AbcYazilim.OgrenciTakip.Model.Entities.Base;
using System;
using System.Linq.Expressions;

namespace AbcYazilim.OgrenciTakip.UI.Win.Functions
{
    public class FilterFunctions
    {
        /// <summary>
        /// Varlıkların durumuna (Aktif/Pasif) göre filtreleme yapacak olan bir LINQ ifadesi (Expression) döndürür.
        /// </summary>
        /// <typeparam name="T">Filtreleme yapılacak ve <see cref="BaseEntityDurum"/> arayüzünden türetilmiş varlık tipi.</typeparam>
        /// <param name="aktifKartlariGoster">True ise aktif kayıtları, false ise pasif kayıtları filtrelemek için kullanılır.</param>
        /// <returns>Veritabanı sorgularında (WHERE şartı olarak) kullanılmaya hazır bir lambda ifadesi döner.</returns>
        /// <remarks>
        /// Bu metot, özellikle liste ekranlarında kullanıcıların aktif/pasif kayıtlar arasında geçiş yapmasını sağlayan 
        /// filtreleme mantığını standardize eder.
        /// </remarks>
        public static Expression<Func<T,bool>>Filter<T>(bool aktifKartlariGoster) where T : BaseEntityDurum
        {
           return x=>x.Durum==aktifKartlariGoster;
        }
        /// <summary>
        /// Varlığın benzersiz kimlik numarasına (ID) göre filtreleme yapacak olan bir LINQ ifadesi (Expression) döndürür.
        /// </summary>
        /// <typeparam name="T">Filtreleme yapılacak ve <see cref="BaseEntityDurum"/> arayüzünden türetilmiş varlık tipi.</typeparam>
        /// <param name="id">Sorgulanacak olan kaydın benzersiz ID değeri.</param>
        /// <returns>Veritabanı sorgularında belirli bir kayda odaklanmak için kullanılacak lambda ifadesini döndürür.</returns>
        /// <remarks>
        /// Genellikle "Single" veri çekme işlemlerinde veya belirli bir kaydın varlığını kontrol ederken 
        /// standart bir filtreleme kalıbı oluşturmak için kullanılır.
        /// </remarks>
        public static Expression<Func<T,bool>>Filter<T>(long id) where T : BaseEntityDurum
        {
            return x=>x.Id==id;
        }
    }
}
