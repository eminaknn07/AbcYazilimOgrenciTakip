using System;

namespace AbcYazilim.Dal.Interfaces
{
    /// <summary>
    /// Veritabanı işlemlerini merkezi bir noktadan yöneten ve atomik olarak kaydedilmesini sağlayan iş birimi arayüzü.
    /// </summary>
    /// <typeparam name="T">İşlem yapılacak temel varlık (entity) tipi.</typeparam>
    /// <remarks>
    /// Bu arayüz, repository katmanı ile veritabanı bağlamı (context) arasında bir köprü görevi görür.
    /// <see cref="IDisposable"/> kalıtımı sayesinde kaynakların güvenli bir şekilde serbest bırakılmasını sağlar.
    /// </remarks>
    public interface IUnitOfWork<T>:IDisposable where T : class
    {
        /// <summary>
        /// İlgili varlık tipi için veri erişim operasyonlarını barındıran repository nesnesine erişim sağlar.
        /// </summary>
        IRepository<T> Rep { get; }
        /// <summary>
        /// Yapılan tüm ekleme, güncelleme ve silme işlemlerini tek bir işlem (transaction) altında veritabanına kaydeder.
        /// </summary>
        /// <returns>Kayıt işlemi başarılı ve veritabanı bütünlüğü korunmuşsa true, hata oluşmuşsa false döner.</returns>
        /// <remarks>
        /// Başarısız durumlarda metodun içindeki hata yönetimi mekanizması kullanıcıya gerekli uyarıları iletir.
        /// </remarks>
        bool Save();

    }
}
